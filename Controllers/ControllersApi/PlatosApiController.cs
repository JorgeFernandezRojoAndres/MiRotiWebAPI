using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiRoti.Data;
using MiRoti.Models;

namespace MiRoti.ControllersApi
{   
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Cliente,Cadete")] // Solo estos roles pueden acceder desde la app móvil
    public class PlatosApiController : ControllerBase
    {
        private readonly MiRotiContext _context;

        public PlatosApiController(MiRotiContext context)
        {
            _context = context;
        }

       
        [HttpGet]
        public async Task<IActionResult> GetPlatos()
        {
            var platos = await _context.Platos
                .Where(p => p.Disponible) // Solo platos disponibles
                .Select(p => new
                {
                    p.Id,
                    p.Nombre,
                    p.Descripcion,
                    Precio = p.PrecioVenta, // 🔹 se usa el campo correcto
                    p.ImagenUrl
                })
                .ToListAsync();

            // Para el frontend, una lista vacía es un resultado válido (permite limpiar estado sin tratarlo como error).
            return Ok(platos);
        }

        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlatoById(int id)
        {
            var plato = await _context.Platos
                .Where(p => p.Id == id && p.Disponible)
                .Select(p => new
                {
                    p.Id,
                    p.Nombre,
                    p.Descripcion,
                    Precio = p.PrecioVenta,
                    p.ImagenUrl
                })
                .FirstOrDefaultAsync();

            if (plato == null)
            {
                return NotFound(new { mensaje = "Plato no encontrado." });
            }

            return Ok(plato);
        }
    }
}
