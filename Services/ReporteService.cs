using System.Collections.Generic;
using System.Linq;
using MiRoti.Data;
using MiRoti.Models;

namespace MiRoti.Services
{
    public class ReporteService
    {
        private readonly MiRotiContext _context;

        public ReporteService(MiRotiContext context)
        {
            _context = context;
        }

        // ✅ Reporte de ganancias por plato
        public IEnumerable<object> ObtenerReporteGanancias()
        {
            return _context.Platos
                .Select(p => new
                {
                    Plato = p.Nombre,
                    CostoTotal = p.CostoTotal,
                    PrecioVenta = p.PrecioVenta,
                    Ganancia = p.PrecioVenta - p.CostoTotal
                })
                .ToList();
        }

        // ✅ Reporte de costos de ingredientes
        public IEnumerable<object> ObtenerCostosIngredientes()
        {
            return _context.Ingredientes
                .Select(i => new
                {
                    Ingrediente = i.Nombre,
                    CostoUnitario = i.CostoUnitario,
                    Unidad = i.UnidadMedida.Nombre
                })
                .ToList();
        }
    }
}
