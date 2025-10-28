using Microsoft.EntityFrameworkCore;
using MiRoti.Data;
using MiRoti.Interfaces;
using MiRoti.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiRoti.Repositories
{
    public class PlatoRepository : GenericRepository<Plato>, IPlatoRepository
    {
        public PlatoRepository(MiRotiContext context) : base(context) { }

        public async Task<IEnumerable<Plato>> GetPlatosConIngredientesAsync()
        {
            return await _context.Platos
                .Include(p => p.PlatoIngredientes)
                    .ThenInclude(pi => pi.Ingrediente)
                        .ThenInclude(i => i.UnidadMedida)
                .ToListAsync();
        }

        public async Task<Plato?> GetPlatoConIngredientesPorIdAsync(int id)
        {
            return await _context.Platos
                .Include(p => p.PlatoIngredientes)
                    .ThenInclude(pi => pi.Ingrediente)
                        .ThenInclude(i => i.UnidadMedida)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
