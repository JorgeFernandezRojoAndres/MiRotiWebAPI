using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiRoti.Data;
using MiRoti.Models;
using MiRoti.DTOs;
using System.Linq;

namespace MiRoti.ControllersApi
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosApiController : ControllerBase
    {
        private readonly MiRotiContext _context;

        public PedidosApiController(MiRotiContext context)
        {
            _context = context;
        }

        // ✅ GET: api/pedidos
        [HttpGet]
        public async Task<IActionResult> GetPedidos()
        {
            var pedidos = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Plato)
                .Select(p => new PedidoDTO
                {
                    Id = p.Id,
                    Cliente = p.Cliente.Nombre,
                    FechaHora = p.FechaHora,
                    Estado = p.Estado,
                    Total = p.Total,
                    Detalles = p.Detalles.Select(d => new DetallePedidoDTO
                    {
                        Plato = d.Plato.Nombre,
                        Cantidad = d.Cantidad,
                        Subtotal = d.Subtotal
                    }).ToList()
                })
                .ToListAsync();

            return Ok(pedidos);
        }

        // ✅ POST: api/pedidos
        [HttpPost]
        public async Task<IActionResult> PostPedido([FromBody] Pedido pedido)
        {
            pedido.FechaHora = DateTime.Now;
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            // devolver DTO recién creado
            var pedidoDto = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Plato)
                .Where(p => p.Id == pedido.Id)
                .Select(p => new PedidoDTO
                {
                    Id = p.Id,
                    Cliente = p.Cliente.Nombre,
                    FechaHora = p.FechaHora,
                    Estado = p.Estado,
                    Total = p.Total,
                    Detalles = p.Detalles.Select(d => new DetallePedidoDTO
                    {
                        Plato = d.Plato.Nombre,
                        Cantidad = d.Cantidad,
                        Subtotal = d.Subtotal
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetPedidos), new { id = pedido.Id }, pedidoDto);
        }
    }
}
