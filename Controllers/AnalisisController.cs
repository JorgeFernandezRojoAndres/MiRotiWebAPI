using Microsoft.AspNetCore.Mvc;
using MiRoti.Services;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace MiRoti.Controllers
{
    public class AnalisisController : Controller
    {
        private readonly ReporteService _reporteService;

        public AnalisisController(ReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        public IActionResult Index()
        {
            // 🔐 Validar sesión y rol
            var rol = HttpContext.Session.GetString("UsuarioRol");

            if (string.IsNullOrEmpty(rol) || rol != "Admin")
            {
                // 🚫 Si no hay sesión o no es Admin → redirige al Login
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            // 🔹 Obtener reporte de ganancias
            var reporte = _reporteService.ObtenerReporteGanancias().ToList();

            if (!reporte.Any())
            {
                ViewData["Reporte"] = reporte;
                ViewData["TotalVentas"] = 0m;
                ViewData["GananciaTotal"] = 0m;
                ViewData["PlatoMasVendido"] = "Sin datos";
                ViewData["TotalPedidos"] = 0;
                return View();
            }

            // ✅ Cálculos directos (manteniendo tu lógica original)
            var totalVentas = reporte.Sum(x => (decimal)x.GetType().GetProperty("PrecioVenta")!.GetValue(x, null)!);
            var gananciaTotal = reporte.Sum(x => (decimal)x.GetType().GetProperty("Ganancia")!.GetValue(x, null)!);
            var platoMasVendido = reporte
                .OrderByDescending(x => (decimal)x.GetType().GetProperty("Ganancia")!.GetValue(x, null)!)
                .First()
                .GetType().GetProperty("Plato")!.GetValue(reporte.First(), null)!.ToString();

            // 🔹 Pasar datos a la vista
            ViewData["Reporte"] = reporte;
            ViewData["TotalVentas"] = totalVentas;
            ViewData["GananciaTotal"] = gananciaTotal;
            ViewData["PlatoMasVendido"] = platoMasVendido;
            ViewData["TotalPedidos"] = reporte.Count;

            return View();
        }
    }
}
