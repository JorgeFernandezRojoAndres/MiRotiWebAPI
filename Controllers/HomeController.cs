using Microsoft.AspNetCore.Mvc;
using MiRoti.Data;
using System.Linq;

namespace MiRoti.Controllers
{
    public class HomeController : Controller
    {
        private readonly MiRotiContext _context;

        public HomeController(MiRotiContext context)
        {
            _context = context;
        }

        // 🔹 Vista principal del panel (HTML)
        [HttpGet]
        public IActionResult Index()
        {
            ViewData["TotalPlatos"] = _context.Platos.Count();
            ViewData["TotalPedidos"] = _context.Pedidos.Count();
            ViewData["VentasTotales"] = _context.Pedidos.Sum(p => (decimal?)p.Total) ?? 0;
            ViewData["ClientesActivos"] = _context.Clientes.Count();

            return View();
        }

        // 🔹 Endpoint raíz del backend (JSON / visible en Swagger)
        [HttpGet("api/info")]
        public IActionResult ApiInfo()
        {
            return Ok(new
            {
                Nombre = "MiRoti API",
                Version = "v1",
                Descripcion = "🍗 Sistema de gestión para rotiserías",
                Estado = "✅ Activa y funcionando"
            });
        }
    }
}
