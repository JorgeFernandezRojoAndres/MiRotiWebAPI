using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiRoti.Data;
using MiRoti.Models;

namespace MiRoti.Controllers
{
    public class PedidosController : Controller
    {
        private readonly MiRotiContext _context;

        public PedidosController(MiRotiContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var pedidos = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Detalles)
                .ToListAsync();

            return View(pedidos);
        }
    }
}
