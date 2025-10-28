using Microsoft.AspNetCore.Mvc;
using MiRoti.Services;
using System.Linq;

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

            // ✅ Cálculos directos
            var totalVentas = reporte.Sum(x => (decimal)x.GetType().GetProperty("PrecioVenta")!.GetValue(x, null)!);
            var gananciaTotal = reporte.Sum(x => (decimal)x.GetType().GetProperty("Ganancia")!.GetValue(x, null)!);
            var platoMasVendido = reporte
                .OrderByDescending(x => (decimal)x.GetType().GetProperty("Ganancia")!.GetValue(x, null)!)
                .First()
                .GetType().GetProperty("Plato")!.GetValue(reporte.First(), null)!.ToString();

            ViewData["Reporte"] = reporte;
            ViewData["TotalVentas"] = totalVentas;
            ViewData["GananciaTotal"] = gananciaTotal;
            ViewData["PlatoMasVendido"] = platoMasVendido;
            ViewData["TotalPedidos"] = reporte.Count;

            return View();
        }
    }
}
