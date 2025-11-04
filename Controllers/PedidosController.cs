using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiRoti.Data;
using MiRoti.Models;
using Microsoft.AspNetCore.Authorization;

namespace MiRoti.Controllers
{
    // 🔐 Solo los usuarios con rol "Admin" pueden acceder a este controlador
    [Authorize(Roles = "Admin")]
    public class PedidosController : Controller
    {
        private readonly MiRotiContext _context;

        public PedidosController(MiRotiContext context)
        {
            _context = context;
        }

        // 🔹 Listado de pedidos con sus relaciones
        public async Task<IActionResult> Index()
        {
            var pedidos = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Cadete)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Plato)
                .OrderByDescending(p => p.FechaHora)
                .ToListAsync();

            return View(pedidos);
        }
    }
}
