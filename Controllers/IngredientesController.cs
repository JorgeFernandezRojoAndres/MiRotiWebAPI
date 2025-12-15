using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiRoti.Data;
using MiRoti.Models;
using Microsoft.AspNetCore.Authorization;

namespace MiRoti.Controllers
{
    [Authorize(Roles = "Admin,Cocinero")]
    public class IngredientesController : Controller
    {
        private readonly MiRotiContext _context;

        public IngredientesController(MiRotiContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var ingredientes = await _context.Ingredientes
                .Include(i => i.UnidadMedida)
                .OrderBy(i => i.Nombre)
                .ToListAsync();

            return View("~/Views/Shared/IngredientesIndex.cshtml", ingredientes);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.UnidadesMedida = await _context.UnidadesMedida.ToListAsync();
            return View("~/Views/Shared/IngredientesCreate.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ingrediente ingrediente)
        {
            if (ModelState.IsValid)
            {
                _context.Ingredientes.Add(ingrediente);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Ingrediente '{ingrediente.Nombre}' agregado correctamente";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.UnidadesMedida = await _context.UnidadesMedida.ToListAsync();
            return View("~/Views/Shared/IngredientesCreate.cshtml", ingrediente);
        }

        [HttpPost]
        public IActionResult CreateJson([FromBody] Ingrediente ingrediente)
        {
            if (ingrediente == null)
                return Json(new { success = false, message = "Datos inválidos" });

            if (string.IsNullOrWhiteSpace(ingrediente.Nombre))
                return Json(new { success = false, message = "El nombre es obligatorio" });

            if (ingrediente.UnidadMedidaId <= 0)
                return Json(new { success = false, message = "Debe seleccionar una unidad de medida" });

            if (ingrediente.CostoUnitario <= 0)
                return Json(new { success = false, message = "El costo debe ser mayor a 0" });

            _context.Ingredientes.Add(ingrediente);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var ingrediente = await _context.Ingredientes
                .Include(i => i.UnidadMedida)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (ingrediente == null)
                return NotFound();

            return View("~/Views/Shared/IngredientesEdit.cshtml", ingrediente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, decimal costoUnitario)
        {
            var ingrediente = await _context.Ingredientes.FindAsync(id);
            if (ingrediente == null)
                return NotFound();

            if (costoUnitario <= 0)
            {
                TempData["Error"] = "El costo debe ser mayor a 0";
                return RedirectToAction(nameof(Edit), new { id });
            }

            ingrediente.CostoUnitario = costoUnitario;
            
            try
            {
                _context.Update(ingrediente);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Precio de {ingrediente.Nombre} actualizado correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar el precio";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
