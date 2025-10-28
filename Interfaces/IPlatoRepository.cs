using System.Collections.Generic;
using System.Threading.Tasks;
using MiRoti.Models;

namespace MiRoti.Interfaces
{
    public interface IPlatoRepository : IGenericRepository<Plato>
    {
        Task<IEnumerable<Plato>> GetPlatosConIngredientesAsync();
        Task<Plato?> GetPlatoConIngredientesPorIdAsync(int id);
    }
}
