using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MiRoti.Data;
using MiRoti.Models;
using Microsoft.EntityFrameworkCore;

namespace MiRoti.Services
{
    public class AuthService
    {
        private readonly MiRotiContext _context;
        private readonly IConfiguration _config;

        public AuthService(MiRotiContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // 🔐 LOGIN real (antes era simulado)
        public async Task<string?> AutenticarAsync(string email, string contrasenia)
        {
            // Buscar usuario por email
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return null;

            // Validar contraseña con BCrypt
            bool esValido = BCrypt.Net.BCrypt.Verify(contrasenia, user.Contrasenia);
            if (!esValido)
                return null;

            // Generar token JWT
            var token = GenerarToken(user);
            return token;
        }

        // 🧩 Registrar nuevo usuario (opcional)
        public async Task<Usuario> RegistrarAsync(Usuario usuario)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email))
                throw new Exception("El correo ya está registrado.");

            // Hashear contraseña
            usuario.Contrasenia = BCrypt.Net.BCrypt.HashPassword(usuario.Contrasenia);
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return usuario;
        }

        // 🔧 Generador de token JWT con claims
        private string GenerarToken(Usuario user)
        {
            var jwtKey = _config["Jwt:Key"]!;
            var jwtIssuer = _config["Jwt:Issuer"]!;
            var jwtAudience = _config["Jwt:Audience"]!;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim("id", user.Id.ToString()),
                new Claim("rol", user.Rol),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
