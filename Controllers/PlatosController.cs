using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiRoti.Data;
using MiRoti.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace MiRoti.Controllers
{
    // 🔐 Solo "Admin" y "Cocinero" pueden acceder al módulo de platos
    [Authorize(Roles = "Admin,Cocinero")]
    public class PlatosController : Controller
    {
        private readonly MiRotiContext _context;
        private readonly IWebHostEnvironment _env;

        public PlatosController(MiRotiContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // 🔹 Listado de platos con pedidos relacionados (opcional)
        public async Task<IActionResult> Index(bool mostrarInactivos = false)
        {
            ViewData["MostrarInactivos"] = mostrarInactivos;

            // 🧮 Calcular métricas para la vista (sobre el total, no sobre el filtro)
            ViewData["Activos"] = await _context.Platos.CountAsync(p => p.Disponible);
            ViewData["Inactivos"] = await _context.Platos.CountAsync(p => !p.Disponible);

            var query = _context.Platos.AsQueryable();
            if (!mostrarInactivos)
            {
                query = query.Where(p => p.Disponible);
            }

            var platos = await query
                .Include(p => p.Detalles)              // ✅ Carga los DetallePedido asociados
                    .ThenInclude(d => d.Pedido)        // ✅ Y cada pedido vinculado
                .Include(p => p.PlatoIngredientes)     // ✅ Opcional: carga los ingredientes
                .ToListAsync();

            var platosConPrecio = platos.Where(p => p.PrecioVenta != 0).ToList();
            ViewData["MargenPromedio"] = platosConPrecio
                .Select(p => (double)((p.PrecioVenta - p.CostoTotal) / p.PrecioVenta) * 100)
                .DefaultIfEmpty(0)
                .Average();

            return View(platos);
        }



        // 🔹 Formulario de creación
        public IActionResult Create() => View();
        
        // 🔹 Formulario de creación simplificado
        public IActionResult CreateSimple() => View();
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSimple(
            Plato plato,
            IFormFile? imagen,
            string? ingredientesData,
            decimal? costoTotalManual,
            decimal? manoObraPorcentaje)
        {
            if (string.IsNullOrWhiteSpace(plato.Nombre))
            {
                ModelState.AddModelError(string.Empty, "El nombre del plato es obligatorio.");
                return View(plato);
            }

            if (plato.PrecioVenta <= 0)
            {
                ModelState.AddModelError(string.Empty, "El precio de venta debe ser mayor a 0.");
                return View(plato);
            }

            var ingredientes = new List<IngredientesPlatoDto>();
            if (!string.IsNullOrWhiteSpace(ingredientesData))
            {
                try
                {
                    ingredientes = System.Text.Json.JsonSerializer.Deserialize<List<IngredientesPlatoDto>>(ingredientesData) ?? new List<IngredientesPlatoDto>();
                }
                catch
                {
                    ingredientes = new List<IngredientesPlatoDto>();
                }
            }

            if (costoTotalManual.HasValue && costoTotalManual.Value > 0)
            {
                plato.CostoTotal = costoTotalManual.Value;
            }
            else if (ingredientes.Any(i => i.IngredienteId > 0 && i.Cantidad > 0))
            {
                var totalIngredientes = ingredientes.Sum(i => i.Subtotal);
                var porcentaje = manoObraPorcentaje.GetValueOrDefault(0);
                if (porcentaje < 0 || porcentaje > 100)
                {
                    ModelState.AddModelError(string.Empty, "El porcentaje de mano de obra debe estar entre 0 y 100.");
                    return View(plato);
                }

                var manoObra = totalIngredientes * (porcentaje / 100m);
                plato.CostoTotal = totalIngredientes + manoObra;
            }

            if (plato.CostoTotal <= 0)
            {
                ModelState.AddModelError(string.Empty, "Debe ingresar un costo total manual o agregar ingredientes para calcular el costo total.");
                return View(plato);
            }

            ModelState.Remove(nameof(Plato.CostoTotal));
            TryValidateModel(plato);

            if (!ModelState.IsValid)
            {
                return View(plato);
            }

            try
            {
                // 📸 Imagen opcional (misma lógica que Create)
                if (imagen != null && imagen.Length > 0)
                {
                    var uploads = Path.Combine(_env.WebRootPath, "images", "platos");
                    Directory.CreateDirectory(uploads);

                    var fileName = Path.GetFileName(imagen.FileName);
                    var filePath = Path.Combine(uploads, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imagen.CopyToAsync(stream);
                    }

                    plato.ImagenUrl = $"/images/platos/{fileName}";
                }

                _context.Platos.Add(plato);
                await _context.SaveChangesAsync();

                if (ingredientes.Any(i => i.IngredienteId > 0 && i.Cantidad > 0))
                {
                    await GuardarIngredientesPlato(plato.Id, ingredientesData!);
                }

                TempData["Success"] = $"Plato '{plato.Nombre}' creado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Error al guardar el plato");
                return View(plato);
            }
        }
        
        // 🔹 Test page
        public IActionResult Test() => View();

        // 🔹 Crear nuevo plato (con imagen opcional)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Plato plato,
            IFormFile? imagen,
            string? ingredientesData,
            decimal? costoTotalManual,
            decimal? manoObraPorcentaje)
        {
            if (plato.PrecioVenta <= 0)
            {
                ModelState.AddModelError(string.Empty, "El precio de venta debe ser mayor a 0.");
            }

            // Si el cocinero no ingresa costo manual, se calcula en el backend con los ingredientes.
            if (costoTotalManual.HasValue && costoTotalManual.Value > 0)
            {
                plato.CostoTotal = costoTotalManual.Value;
            }
            else
            {
                List<IngredientesPlatoDto> ingredientes;
                try
                {
                    ingredientes = string.IsNullOrWhiteSpace(ingredientesData)
                        ? new List<IngredientesPlatoDto>()
                        : (System.Text.Json.JsonSerializer.Deserialize<List<IngredientesPlatoDto>>(ingredientesData) ?? new List<IngredientesPlatoDto>());
                }
                catch
                {
                    ingredientes = new List<IngredientesPlatoDto>();
                }

                if (!ingredientes.Any(i => i.IngredienteId > 0 && i.Cantidad > 0))
                {
                    ModelState.AddModelError(string.Empty, "Debe ingresar un costo total manual o agregar ingredientes para calcular el costo total.");
                }
                else
                {
                    var totalIngredientes = ingredientes.Sum(i => i.Subtotal);
                    var porcentaje = manoObraPorcentaje.GetValueOrDefault(0);
                    if (porcentaje < 0 || porcentaje > 100)
                    {
                        ModelState.AddModelError(string.Empty, "El porcentaje de mano de obra debe estar entre 0 y 100.");
                    }
                    else
                    {
                        var manoObra = totalIngredientes * (porcentaje / 100m);
                        plato.CostoTotal = totalIngredientes + manoObra;
                    }
                }
            }

            if (plato.CostoTotal <= 0)
            {
                ModelState.AddModelError(string.Empty, "El costo total debe ser mayor a 0.");
            }

            // Revalidar el modelo luego de setear CostoTotal (para evitar fallos por default 0 cuando no viene del formulario)
            ModelState.Remove(nameof(Plato.CostoTotal));
            TryValidateModel(plato);

            if (!ModelState.IsValid)
            {
                return View(plato);
            }

            // 📸 Si el cocinero sube una imagen
            if (imagen != null && imagen.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "images", "platos");
                Directory.CreateDirectory(uploads);

                var fileName = Path.GetFileName(imagen.FileName);
                var filePath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imagen.CopyToAsync(stream);
                }

                plato.ImagenUrl = $"/images/platos/{fileName}";
            }

            _context.Add(plato);
            await _context.SaveChangesAsync();
            
            // Guardar ingredientes si se proporcionaron
            if (!string.IsNullOrEmpty(ingredientesData))
            {
                await GuardarIngredientesPlato(plato.Id, ingredientesData);
            }
            
            return RedirectToAction(nameof(Index));
        }

        // 🔹 Editar un plato existente
        public async Task<IActionResult> Edit(int id)
        {
            var plato = await _context.Platos
                .Include(p => p.PlatoIngredientes)
                    .ThenInclude(pi => pi.Ingrediente)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plato == null)
                return NotFound();

            ViewData["IngredientesPlato"] = plato.PlatoIngredientes.Select(pi => new {
                IngredienteId = pi.IngredienteId,
                Nombre = pi.Ingrediente?.Nombre ?? string.Empty,
                Cantidad = pi.Cantidad,
                Subtotal = pi.Subtotal
            }).ToList();

            return View(plato);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Plato plato,
            IFormFile? imagen,
            string? ingredientesData,
            decimal? costoTotalManual,
            decimal? manoObraPorcentaje)
        {
            var existente = await _context.Platos.FindAsync(id);
            if (existente == null)
                return NotFound();

            var ingredientes = new List<IngredientesPlatoDto>();
            if (!string.IsNullOrWhiteSpace(ingredientesData))
            {
                try
                {
                    ingredientes = System.Text.Json.JsonSerializer.Deserialize<List<IngredientesPlatoDto>>(ingredientesData) ?? new List<IngredientesPlatoDto>();
                }
                catch
                {
                    ingredientes = new List<IngredientesPlatoDto>();
                }
            }

            // Permitir edición sin ingredientes: si no hay costo manual, se usa el costo enviado o se calcula si hay ingredientes.
            if (costoTotalManual.HasValue && costoTotalManual.Value > 0)
            {
                plato.CostoTotal = costoTotalManual.Value;
            }
            else if (ingredientes.Any(i => i.IngredienteId > 0 && i.Cantidad > 0))
            {
                var totalIngredientes = ingredientes.Sum(i => i.Subtotal);
                var porcentaje = manoObraPorcentaje.GetValueOrDefault(0);
                if (porcentaje < 0 || porcentaje > 100)
                {
                    ModelState.AddModelError(string.Empty, "El porcentaje de mano de obra debe estar entre 0 y 100.");
                }
                else
                {
                    var manoObra = totalIngredientes * (porcentaje / 100m);
                    plato.CostoTotal = totalIngredientes + manoObra;
                }
            }
            else
            {
                // Si el formulario no trae un costo válido, mantener el costo existente.
                plato.CostoTotal = plato.CostoTotal > 0 ? plato.CostoTotal : existente.CostoTotal;
            }

            ModelState.Remove(nameof(Plato.CostoTotal));
            TryValidateModel(plato);

            if (!ModelState.IsValid)
            {
                return View(plato);
            }

            existente.Nombre = plato.Nombre;
            existente.Descripcion = plato.Descripcion;
            existente.PrecioVenta = plato.PrecioVenta;
            existente.CostoTotal = plato.CostoTotal;
            existente.Disponible = plato.Disponible;

            // 📸 Si se sube una nueva imagen, reemplazar la anterior
            if (imagen != null && imagen.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "images", "platos");
                Directory.CreateDirectory(uploads);

                var fileName = Path.GetFileName(imagen.FileName);
                var filePath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imagen.CopyToAsync(stream);
                }

                existente.ImagenUrl = $"/images/platos/{fileName}";
            }

            _context.Update(existente);
            await _context.SaveChangesAsync();
            
            // Actualizar ingredientes
            if (ingredientesData != null)
            {
                // Eliminar ingredientes existentes
                var ingredientesExistentes = _context.PlatosIngredientes.Where(pi => pi.PlatoId == id);
                _context.PlatosIngredientes.RemoveRange(ingredientesExistentes);
                await _context.SaveChangesAsync();
                
                // Agregar nuevos ingredientes
                if (ingredientes.Any(i => i.IngredienteId > 0 && i.Cantidad > 0))
                {
                    await GuardarIngredientesPlato(id, ingredientesData);
                }
            }
            
            return RedirectToAction(nameof(Index));
        }

        // 🔹 Ver detalles de un plato (incluye imagen)
        public async Task<IActionResult> Details(int id)
        {
            var plato = await _context.Platos.FirstOrDefaultAsync(p => p.Id == id);
            if (plato == null)
                return NotFound();

            return View(plato);
        }

        // 🔹 Mostrar vista de confirmación antes de eliminar
        public async Task<IActionResult> Delete(int id)
        {
            var plato = await _context.Platos.FirstOrDefaultAsync(p => p.Id == id);
            if (plato == null)
                return NotFound();

            return View(plato);
        }

        // 🔹 Confirmar eliminación (ahora desactiva en lugar de borrar)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var plato = await _context.Platos.FindAsync(id);
            if (plato == null)
                return NotFound();

            try
            {
                // ⚠️ En lugar de eliminar físicamente, lo marcamos como inactivo
                plato.Disponible = false;
                _context.Update(plato);
                await _context.SaveChangesAsync();

                TempData["Info"] = "⚠️ El plato estaba asociado a pedidos y fue marcado como inactivo.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error al desactivar plato: {ex.Message}");
                TempData["Error"] = "No se pudo desactivar el plato. Verifique la conexión o los permisos.";
                return RedirectToAction(nameof(Index));
            }

            // 🧹 Intentar eliminar imagen del servidor si no está en uso
            if (!string.IsNullOrEmpty(plato.ImagenUrl))
            {
                try
                {
                    var imagePath = Path.Combine(_env.WebRootPath, plato.ImagenUrl.TrimStart('/').Replace("/", "\\"));
                    if (System.IO.File.Exists(imagePath))
                        System.IO.File.Delete(imagePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error al eliminar imagen: {ex.Message}");
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // 🔹 Reactivar un plato inactivo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivar(int id)
        {
            var plato = await _context.Platos.FindAsync(id);
            if (plato == null)
                return NotFound();

            plato.Disponible = true;

            try
            {
                _context.Update(plato);
                await _context.SaveChangesAsync();

                TempData["Info"] = $"✅ El plato '{plato.Nombre}' fue reactivado correctamente.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error al reactivar plato: {ex.Message}");
                TempData["Error"] = "No se pudo reactivar el plato. Verifique la conexión o los permisos.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task GuardarIngredientesPlato(int platoId, string ingredientesData)
        {
            try
            {
                var ingredientes = System.Text.Json.JsonSerializer.Deserialize<List<IngredientesPlatoDto>>(ingredientesData) ?? new List<IngredientesPlatoDto>();
                
                foreach (var ing in ingredientes)
                {
                    var platoIngrediente = new PlatoIngrediente
                    {
                        PlatoId = platoId,
                        IngredienteId = ing.IngredienteId,
                        Cantidad = ing.Cantidad,
                        Subtotal = ing.Subtotal
                    };
                    
                    _context.PlatosIngredientes.Add(platoIngrediente);
                }
                
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error guardando ingredientes: {ex.Message}");
            }
        }

        public class IngredientesPlatoDto
        {
            public int IngredienteId { get; set; }
            public double Cantidad { get; set; }
            public decimal Subtotal { get; set; }
        }

    }
}
