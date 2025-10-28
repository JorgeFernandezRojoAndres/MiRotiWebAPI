using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiRoti.Data;
using MiRoti.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace MiRoti.Controllers
{
    public class PlatosController : Controller
    {
        private readonly MiRotiContext _context;
        private readonly IWebHostEnvironment _env;

        public PlatosController(MiRotiContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // 🔹 Listado de platos
        public async Task<IActionResult> Index()
        {
            var platos = await _context.Platos.ToListAsync();
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

        // 🔹 Eliminar un plato (confirmación)
        public async Task<IActionResult> Delete(int id)
        {
            var plato = await _context.Platos.FirstOrDefaultAsync(p => p.Id == id);
            if (plato == null)
                return NotFound();

            return View(plato);
        }

        // 🔹 Confirmar eliminación
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var plato = await _context.Platos.FindAsync(id);
            if (plato == null)
                return NotFound();

            // 🧹 Eliminar imagen del servidor si existe
            if (!string.IsNullOrEmpty(plato.ImagenUrl))
            {
                var imagePath = Path.Combine(_env.WebRootPath, plato.ImagenUrl.TrimStart('/').Replace("/", "\\"));
                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            _context.Platos.Remove(plato);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}
