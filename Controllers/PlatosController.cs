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
      public async Task<IActionResult> Index()
{
    var platos = await _context.Platos
        .Include(p => p.Detalles)              // ✅ Carga los DetallePedido asociados
            .ThenInclude(d => d.Pedido)        // ✅ Y cada pedido vinculado
        .Include(p => p.PlatoIngredientes)     // ✅ Opcional: carga los ingredientes
        .ToListAsync();

    // 🧮 Calcular métricas para la vista
    ViewData["Activos"] = platos.Count(p => p.Disponible);
    ViewData["Inactivos"] = platos.Count(p => !p.Disponible);
    ViewData["MargenPromedio"] = platos.Any()
        ? platos.Average(p => (double)((p.PrecioVenta - p.CostoTotal) / p.PrecioVenta) * 100)
        : 0;

    return View(platos);
}



        // 🔹 Formulario de creación
        public IActionResult Create() => View();

        // 🔹 Crear nuevo plato (con imagen opcional)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Plato plato, IFormFile? imagen)
        {
            if (ModelState.IsValid)
            {
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
                return RedirectToAction(nameof(Index));
            }

            return View(plato);
        }

        // 🔹 Editar un plato existente
        public async Task<IActionResult> Edit(int id)
        {
            var plato = await _context.Platos.FindAsync(id);
            if (plato == null)
                return NotFound();

            return View(plato);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Plato plato, IFormFile? imagen)
        {
            var existente = await _context.Platos.FindAsync(id);
            if (existente == null)
                return NotFound();

            if (ModelState.IsValid)
            {
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
                return RedirectToAction(nameof(Index));
            }

            return View(plato);
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


    }
}
