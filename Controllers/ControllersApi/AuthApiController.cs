using Microsoft.AspNetCore.Mvc;
using MiRoti.Models;
using MiRoti.Services;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using MiRoti.Data;

namespace MiRoti.ControllersApi
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthApiController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly MiRotiContext _context;

        public AuthApiController(AuthService authService, MiRotiContext context)
        {
            _authService = authService;
            _context = context;
        }

        // ✅ DTO para recibir solo las credenciales
        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Contrasenia { get; set; } = string.Empty;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Contrasenia))
                return BadRequest(new { mensaje = "Faltan credenciales" });

            // ✅ Buscar el usuario en la base de datos
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (usuario == null)
                return Unauthorized(new { mensaje = "Usuario no encontrado" });

            // ✅ Verificar la contraseña
            bool passwordValida = BCrypt.Net.BCrypt.Verify(request.Contrasenia, usuario.Contrasenia);
            if (!passwordValida)
                return Unauthorized(new { mensaje = "Contraseña incorrecta" });

            // ✅ Generar el token JWT
            var token = _authService.GenerarToken(usuario);

            // 🔹 Devolver datos completos que el móvil necesita
            return Ok(new
            {
                token,
                id = usuario.Id,
                email = usuario.Email,
                rol = usuario.Rol
            });
        }
    }
}
