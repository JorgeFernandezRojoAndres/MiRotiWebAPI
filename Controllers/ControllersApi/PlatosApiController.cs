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
    public class PlatosApiController : ControllerBase
    {
        private readonly MiRotiContext _context;

        public PlatosApiController(MiRotiContext context)
        {
            _context = context;
        }

        // ✅ GET: api/platos
        [HttpGet]
        public async Task<IActionResult> GetPlatos()
        {
            var platos = await _context.Platos
                .Include(p => p.PlatoIngredientes)
                    .ThenInclude(pi => pi.Ingrediente)
                        .ThenInclude(i => i.UnidadMedida)
                .Select(p => new PlatoDTO
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    PrecioVenta = p.PrecioVenta,
                    CostoTotal = p.CostoTotal,
                    Disponible = p.Disponible,
                    Ingredientes = p.PlatoIngredientes.Select(pi => new IngredienteDTO
                    {
                        Nombre = pi.Ingrediente.Nombre,
                        Cantidad = pi.Cantidad,
                        Subtotal = pi.Subtotal,
                        Unidad = pi.Ingrediente.UnidadMedida.Nombre
                    }).ToList()
                })
                .ToListAsync();

            return Ok(platos);
        }

        // ✅ GET: api/platos/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlato(int id)
        {
            var plato = await _context.Platos
                .Include(p => p.PlatoIngredientes)
                    .ThenInclude(pi => pi.Ingrediente)
                        .ThenInclude(i => i.UnidadMedida)
                .Where(p => p.Id == id)
                .Select(p => new PlatoDTO
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    PrecioVenta = p.PrecioVenta,
                    CostoTotal = p.CostoTotal,
                    Disponible = p.Disponible,
                    Ingredientes = p.PlatoIngredientes.Select(pi => new IngredienteDTO
                    {
                        Nombre = pi.Ingrediente.Nombre,
                        Cantidad = pi.Cantidad,
                        Subtotal = pi.Subtotal,
                        Unidad = pi.Ingrediente.UnidadMedida.Nombre
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (plato == null)
                return NotFound();

            return Ok(plato);
        }

        // ✅ POST: api/platos
        [HttpPost]
        public async Task<IActionResult> PostPlato([FromBody] Plato plato)
        {
            _context.Platos.Add(plato);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPlato), new { id = plato.Id }, plato);
        }
    }
}
