using Microsoft.AspNetCore.Mvc;
using MiRoti.Data;
using MiRoti.Models;
using Microsoft.EntityFrameworkCore;

namespace MiRoti.Controllers
{
    public class PlatosController : Controller
    {
        private readonly MiRotiContext _context;

        public PlatosController(MiRotiContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var platos = await _context.Platos.ToListAsync();
            return View(platos);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Plato plato)
        {
            if (ModelState.IsValid)
            {
                _context.Add(plato);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(plato);
        }
    }
}
