using Microsoft.EntityFrameworkCore;
using MiRoti.Data;
using MiRoti.Interfaces;
using MiRoti.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiRoti.Repositories
{
    public class PedidoRepository : GenericRepository<Pedido>, IPedidoRepository
    {
        public PedidoRepository(MiRotiContext context) : base(context) { }

        // 🧩 Método que obtiene todos los pedidos con detalles
        public async Task<IEnumerable<Pedido>> GetPedidosConDetallesAsync()
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Plato)
                .ToListAsync();
        }

        // 🧩 Método que obtiene un pedido con detalles por su ID
        public async Task<Pedido?> GetPedidoConDetallesPorIdAsync(int id)
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Plato)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // 🧩 Método que obtiene los pedidos filtrados por estado
        public async Task<IEnumerable<Pedido>> GetPedidosPorEstadoAsync(string estado)
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Plato)
                .Where(p => p.Estado == estado)
                .ToListAsync();
        }
    }
}
