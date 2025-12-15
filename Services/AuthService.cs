using System;
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

        public async Task<string?> AutenticarAsync(string email, string contrasenia)
        {
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return null;

            var hashGuardado = user.Contrasenia;
            bool esValido = BCrypt.Net.BCrypt.Verify(contrasenia, hashGuardado);

            // Si la contraseña se guardó en texto plano (semilla vieja), permitirla y rehashear
            if (!esValido && string.Equals(hashGuardado, contrasenia, StringComparison.Ordinal))
            {
                esValido = true;
                user.Contrasenia = BCrypt.Net.BCrypt.HashPassword(contrasenia);
                await _context.SaveChangesAsync();
            }

            if (!esValido)
                return null;

            var token = GenerarToken(user);
            return token;
        }

        public async Task<Usuario> RegistrarAsync(Usuario usuario)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email))
                throw new Exception("El correo ya esta registrado.");

            usuario.Contrasenia = BCrypt.Net.BCrypt.HashPassword(usuario.Contrasenia);
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return usuario;
        }

        public string GenerarToken(Usuario user)
        {
            var jwtKey = _config["Jwt:Key"]!;
            // Mantener consistente con la validación configurada en Program.cs (defaults incluidos).
            var jwtIssuer = _config["Jwt:Issuer"] ?? "MiRotiAPI";
            var jwtAudience = _config["Jwt:Audience"] ?? "MiRotiMobile";

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var rolNormalizado = NormalizarRol(user.Rol);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("id", user.Id.ToString()),
                new Claim(ClaimTypes.Role, rolNormalizado),
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

        private static string NormalizarRol(string? rol)
        {
            if (string.IsNullOrWhiteSpace(rol))
                return "Cliente";

            var limpio = rol.Trim();
            return limpio.Equals("Administrador", StringComparison.OrdinalIgnoreCase) ? "Admin" : limpio;
        }
    }
}
