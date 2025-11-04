using Microsoft.AspNetCore.Mvc;
using MiRoti.Data;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace MiRoti.Controllers
{
    // 🔐 Solo los roles "Admin" y "Cocinero" pueden ver el panel principal
    [Authorize(Roles = "Admin,Cocinero")]
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
            // ✅ Protegemos con Try/Catch para evitar errores si la BD está vacía
            try
            {
                ViewData["TotalPlatos"] = _context.Platos.Count();
                ViewData["TotalPedidos"] = _context.Pedidos.Count();
                ViewData["VentasTotales"] = _context.Pedidos.Sum(p => (decimal?)p.Total) ?? 0;
                ViewData["ClientesActivos"] = _context.Clientes.Count();
            }
            catch
            {
                // En caso de error, evitar que la vista explote
                ViewData["TotalPlatos"] = 0;
                ViewData["TotalPedidos"] = 0;
                ViewData["VentasTotales"] = 0m;
                ViewData["ClientesActivos"] = 0;
            }

            return View();
        }

        // 🔹 Endpoint raíz del backend (JSON / visible en Swagger)
        [AllowAnonymous] // 👈 este se puede consultar sin autenticación
        [HttpGet("api/info")]
        public IActionResult ApiInfo()
        {
            return Ok(new
            {
                Nombre = "MiRoti API",
                Version = "v1.0",
                Descripcion = "🍗 Sistema integral de gestión para rotiserías",
                Estado = "✅ API activa y funcionando",
                FechaServidor = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }
    }
}
