using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiRoti.Data;

namespace MiRoti.ControllersApi
{
    [ApiController]
    [Route("api/ingredientes")]
    public class IngredientesApiController : ControllerBase
    {
        private readonly MiRotiContext _context;

        public IngredientesApiController(MiRotiContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetIngredientes()
        {
            var ingredientes = await _context.Ingredientes
                .Include(i => i.UnidadMedida)
                .Select(i => new { 
                    i.Id, 
                    i.Nombre, 
                    i.CostoUnitario,
                    UnidadMedida = new {
                        i.UnidadMedida.Id,
                        i.UnidadMedida.Nombre,
                        i.UnidadMedida.Abreviatura
                    }
                })
                .ToListAsync();

            return Ok(ingredientes);
        }
    }
}