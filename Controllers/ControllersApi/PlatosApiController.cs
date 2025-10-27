using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiRoti.Data;
using MiRoti.Models;

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
                .Include(p => p.PlatoIngredientes)       // relación intermedia
                .ThenInclude(pi => pi.Ingrediente)       // carga los ingredientes
                .ToListAsync();

            return Ok(platos); // 🔹 faltaba devolver la respuesta
        }

        // ✅ GET: api/platos/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlato(int id)
        {
            var plato = await _context.Platos
                .Include(p => p.PlatoIngredientes)
                .ThenInclude(pi => pi.Ingrediente)
                .FirstOrDefaultAsync(p => p.Id == id);

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
