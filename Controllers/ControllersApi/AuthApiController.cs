using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiRoti.Data;
using MiRoti.Models;
using MiRoti.Services;

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

        // DTO para registro de clientes
        public class RegisterRequest
        {
            public string Nombre { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Contrasenia { get; set; } = string.Empty;
            public string Direccion { get; set; } = string.Empty;
            public string Telefono { get; set; } = string.Empty;
        }

        public class ForgotPasswordRequest
        {
            public string Email { get; set; } = string.Empty;
        }

        public class ResetPasswordRequest
        {
            public string Email { get; set; } = string.Empty;
            public string NuevaContrasenia { get; set; } = string.Empty;
            public string RepetirContrasenia { get; set; } = string.Empty;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Contrasenia))
                return BadRequest(new { mensaje = "Faltan credenciales" });

            // ✅ Buscar el usuario en la base de datos
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u =>
                u.Email == request.Email
            );
            if (usuario == null)
                return Unauthorized(new { mensaje = "Usuario no encontrado" });

            // 🧩 Depuración: mostrar valores que se comparan
            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine($"🧠 Comparando contraseña recibida: '{request.Contrasenia}'");
            Console.WriteLine($"🧩 Hash guardado en BD: '{usuario.Contrasenia}'");
            Console.WriteLine("------------------------------------------------------");

            // ✅ Verificar la contraseña
            bool passwordValida = BCrypt.Net.BCrypt.Verify(
                request.Contrasenia,
                usuario.Contrasenia
            );

            if (!passwordValida)
            {
                Console.WriteLine("❌ Contraseña incorrecta (no coincide el hash)");
                return Unauthorized(new { mensaje = "Contraseña incorrecta" });
            }

            Console.WriteLine("✅ Contraseña verificada correctamente");

            // ✅ Generar el token JWT
            var token = _authService.GenerarToken(usuario);

            // 🔹 Devolver datos completos que el móvil necesita
            return Ok(
                new
                {
                    token,
                    id = usuario.Id,
                    email = usuario.Email,
                    rol = usuario.Rol,
                }
            );
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Contrasenia) || string.IsNullOrWhiteSpace(request.Nombre))
                return BadRequest(new { mensaje = "Nombre, email y contrasenia son requeridos" });

            var existe = await _context.Usuarios.AnyAsync(u => u.Email == request.Email);
            if (existe)
                return Conflict(new { mensaje = "El email ya existe" });

            var cliente = new Cliente
            {
                Nombre = request.Nombre,
                Email = request.Email,
                Contrasenia = BCrypt.Net.BCrypt.HashPassword(request.Contrasenia),
                Rol = "Cliente",
                Direccion = request.Direccion,
                Telefono = request.Telefono
            };

            _context.Usuarios.Add(cliente);
            await _context.SaveChangesAsync();

            var token = _authService.GenerarToken(cliente);

            return CreatedAtAction(nameof(Login), new
            {
                token,
                id = cliente.Id,
                email = cliente.Email,
                rol = cliente.Rol
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { mensaje = "El correo electrónico es requerido" });

            if (string.IsNullOrWhiteSpace(request.NuevaContrasenia) || string.IsNullOrWhiteSpace(request.RepetirContrasenia))
                return BadRequest(new { mensaje = "La contraseña es requerida" });

            if (!string.Equals(request.NuevaContrasenia, request.RepetirContrasenia, StringComparison.Ordinal))
                return BadRequest(new { mensaje = "Las contraseñas no coinciden" });

            // Regla mínima: 8 caracteres
            if (request.NuevaContrasenia.Trim().Length < 8)
                return BadRequest(new { mensaje = "La contraseña es inválida o demasiado corta" });

            var emailLower = request.Email.Trim().ToLowerInvariant();
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email.ToLower() == emailLower);
            if (usuario == null)
                return NotFound(new { mensaje = "Correo no encontrado" });

            usuario.Contrasenia = BCrypt.Net.BCrypt.HashPassword(request.NuevaContrasenia);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Contraseña actualizada correctamente" });
        }
    }
}
