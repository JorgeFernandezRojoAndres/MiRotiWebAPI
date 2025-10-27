using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiRoti.Data;
using MiRoti.Models;

namespace MiRoti.ControllersApi
{
    [ApiController]
    [Route("api/[controller]")]
    public class CadetesApiController : ControllerBase
    {
        private readonly MiRotiContext _context;

        public CadetesApiController(MiRotiContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCadetes()
        {
            var cadetes = await _context.Cadetes.ToListAsync();
            return Ok(cadetes);
        }
    }
}
