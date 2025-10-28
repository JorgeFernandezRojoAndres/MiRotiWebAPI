using System.Collections.Generic;
using System.Threading.Tasks;
using MiRoti.Models;

namespace MiRoti.Interfaces
{
    public interface IPedidoRepository : IGenericRepository<Pedido>
    {
        Task<IEnumerable<Pedido>> GetPedidosConDetallesAsync();
        Task<Pedido?> GetPedidoConDetallesPorIdAsync(int id);
    }
}
