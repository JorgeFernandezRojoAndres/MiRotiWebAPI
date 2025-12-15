using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiRoti.Data;
using MiRoti.Models;

namespace MiRoti.Controllers.ControllersApi
{
    [ApiController]
    [Route("api/usuarios")]
    [Authorize]
    public class UsuariosApiController : ControllerBase
    {
        private readonly MiRotiContext _context;

        public UsuariosApiController(MiRotiContext context)
        {
            _context = context;
        }

        [HttpGet("perfil")]
        public async Task<IActionResult> ObtenerPerfil()
        {
            var usuarioIdClaim = User.FindFirstValue("id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(usuarioIdClaim) || !int.TryParse(usuarioIdClaim, out var usuarioId))
                return Unauthorized(new { mensaje = "Token inválido" });

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            string? direccion = null;
            string? telefono = null;

            if (usuario is Cliente cliente)
            {
                direccion = cliente.Direccion;
                telefono = cliente.Telefono;
            }
            else if (usuario is Cadete cadete)
            {
                direccion = cadete.Direccion;
                telefono = cadete.Telefono;
            }

            return Ok(new
            {
                id = usuario.Id,
                nombre = usuario.Nombre,
                email = usuario.Email,
                direccion,
                telefono,
                rol = usuario.Rol
            });
        }
    }
}
