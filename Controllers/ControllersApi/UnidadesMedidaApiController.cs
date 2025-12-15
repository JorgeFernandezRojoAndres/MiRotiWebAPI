using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiRoti.Data;

namespace MiRoti.ControllersApi
{
    [ApiController]
    [Route("api/unidades-medida")]
    public class UnidadesMedidaApiController : ControllerBase
    {
        private readonly MiRotiContext _context;

        public UnidadesMedidaApiController(MiRotiContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUnidadesMedida()
        {
            var unidades = await _context.UnidadesMedida
                .Select(u => new { u.Id, u.Nombre, u.Abreviatura })
                .ToListAsync();

            return Ok(unidades);
        }
    }
}