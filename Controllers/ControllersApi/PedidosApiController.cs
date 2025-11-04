using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiRoti.Data;
using MiRoti.Models;
using MiRoti.DTOs;
using System.Linq;
using System.Security.Claims;

namespace MiRoti.ControllersApi
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔐 Protege todas las rutas (requiere token JWT)
    public class PedidosApiController : ControllerBase
    {
        private readonly MiRotiContext _context;

        public PedidosApiController(MiRotiContext context)
        {
            _context = context;
        }

        // ===========================================================
        // ✅ GET: api/pedidos (solo Admin)
        // ===========================================================
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPedidos()
        {
            var pedidos = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Cadete)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Plato)
                .Select(p => new PedidoDTO
                {
                    Id = p.Id,
                    Cliente = p.Cliente.Nombre,
                    Cadete = p.Cadete != null ? p.Cadete.Nombre : "Sin asignar",
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
                .OrderByDescending(p => p.FechaHora)
                .ToListAsync();

            return Ok(pedidos);
        }

        // ===========================================================
        // ✅ POST: api/pedidos (Cliente crea un nuevo pedido)
        // ===========================================================
        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> PostPedido([FromBody] Pedido pedido)
        {
            try
            {
                // 🧩 Obtener el ID del cliente autenticado
                var clienteIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (clienteIdStr == null)
                    return Unauthorized(new { success = false, error = "No se pudo obtener el identificador del cliente." });
                var clienteId = int.Parse(clienteIdStr);

                pedido.ClienteId = clienteId;
                pedido.Estado = "Nuevo";
                pedido.FechaHora = DateTime.Now;

                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync();

                // 🔁 Devolver DTO del pedido recién creado
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

                return CreatedAtAction(nameof(GetPedidoById), new { id = pedido.Id }, pedidoDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        // ===========================================================
        // ✅ GET: api/pedidos/{id}
        // ===========================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPedidoById(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Cadete)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Plato)
                .Select(p => new PedidoDTO
                {
                    Id = p.Id,
                    Cliente = p.Cliente.Nombre,
                    Cadete = p.Cadete != null ? p.Cadete.Nombre : "Sin asignar",
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
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
                return NotFound();

            return Ok(pedido);
        }

        // ===========================================================
        // ✅ GET: api/pedidos/mis-pedidos (Cliente)
        // ===========================================================
        [HttpGet("mis-pedidos")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> GetMisPedidos()
        {
            var clienteIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (clienteIdStr == null)
                return Unauthorized(new { success = false, error = "No se pudo obtener el identificador del cliente." });
            var clienteId = int.Parse(clienteIdStr);

            var pedidos = await _context.Pedidos
                .Where(p => p.ClienteId == clienteId)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Plato)
                .OrderByDescending(p => p.FechaHora)
                .Select(p => new PedidoDTO
                {
                    Id = p.Id,
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

        // ===========================================================
        // ✅ GET: api/pedidos/asignados (Cadete)
        // ===========================================================
        [HttpGet("asignados")]
        [Authorize(Roles = "Cadete")]
        public async Task<IActionResult> GetPedidosAsignados()
        {
            var cadeteIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (cadeteIdStr == null)
                return Unauthorized(new { success = false, error = "No se pudo obtener el identificador del cadete." });
            var cadeteId = int.Parse(cadeteIdStr);

            var pedidos = await _context.Pedidos
                .Where(p => p.CadeteId == cadeteId && p.Estado != "Entregado")
                .Include(p => p.Cliente)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Plato)
                .OrderBy(p => p.FechaHora)
                .Select(p => new PedidoDTO
                {
                    Id = p.Id,
                    Cliente = p.Cliente.Nombre,
                    Estado = p.Estado,
                    Total = p.Total,
                    FechaHora = p.FechaHora,
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

        // ===========================================================
        // ✅ PUT: api/pedidos/{id}/entregar (Cadete)
        // ===========================================================
        [HttpPut("{id}/entregar")]
        [Authorize(Roles = "Cadete")]
        public async Task<IActionResult> EntregarPedido(int id)
        {
            var cadeteIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (cadeteIdStr == null)
                return Unauthorized(new { success = false, error = "No se pudo obtener el identificador del cadete." });
            var cadeteId = int.Parse(cadeteIdStr);
            var pedido = await _context.Pedidos.FirstOrDefaultAsync(p => p.Id == id && p.CadeteId == cadeteId);

            if (pedido == null)
                return NotFound(new { success = false, error = "Pedido no encontrado o no asignado a este cadete" });

            if (pedido.Estado == "Entregado")
                return BadRequest(new { success = false, error = "El pedido ya fue entregado" });

            pedido.Estado = "Entregado";
            await _context.SaveChangesAsync();

            return Ok(new { success = true, mensaje = "Pedido marcado como entregado" });
        }

        // ===========================================================
        // ✅ PUT: api/pedidos/{id}/estado (Admin o Cocinero)
        // ===========================================================
        [HttpPut("{id}/estado")]
        [Authorize(Roles = "Admin,Cocinero")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] string nuevoEstado)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
                return NotFound();

            pedido.Estado = nuevoEstado;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, mensaje = $"Estado actualizado a {nuevoEstado}" });
        }
    }
}
