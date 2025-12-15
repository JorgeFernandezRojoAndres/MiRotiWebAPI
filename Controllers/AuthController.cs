using System;
using Microsoft.AspNetCore.Mvc;
using MiRoti.Data;
using MiRoti.Models;
using MiRoti.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace MiRoti.Controllers
{
    public class AuthController : Controller
    {
        private readonly MiRotiContext _context;
        private readonly AuthService _authService;

        public AuthController(MiRotiContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpGet]
        [Route("Auth/Login")]
        public IActionResult Login()
        {
            return View("~/Views/Auth/Login.cshtml");
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        [Route("Auth/Login")]
        public async Task<IActionResult> Login(string email, string contrasenia)
        {
            Console.WriteLine($"Intentando iniciar sesion con email: {email}");

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(contrasenia))
            {
                ViewBag.Error = "Debe ingresar un email y una contrasenia.";
                Console.WriteLine("Campos vacios detectados.");
                return View("~/Views/Auth/Login.cshtml");
            }

            try
            {
                var usuario = await GetUserByEmailAsync(email);
                if (usuario == null)
                {
                    ViewBag.Error = "Usuario no encontrado.";
                    Console.WriteLine($"Usuario no encontrado para email: {email}");
                    return View("~/Views/Auth/Login.cshtml");
                }

                bool passwordValida = BCrypt.Net.BCrypt.Verify(contrasenia, usuario.Contrasenia);
                Console.WriteLine($"Verificacion de contrasenia: {(passwordValida ? "OK" : "INCORRECTA")}");

                if (!passwordValida)
                {
                    ViewBag.Error = "Email o contrasenia incorrectos.";
                    return View("~/Views/Auth/Login.cshtml");
                }

                var token = await _authService.AutenticarAsync(email, contrasenia);
                if (token == null)
                {
                    ViewBag.Error = "Error al generar el token.";
                    Console.WriteLine("Error al generar token JWT.");
                    return View("~/Views/Auth/Login.cshtml");
                }

                var rolNormalizado = NormalizarRol(usuario.Rol);

                HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre ?? "");
                HttpContext.Session.SetString("UsuarioRol", rolNormalizado);
                HttpContext.Session.SetString("TokenJWT", token);
                Console.WriteLine($"Sesion creada para {usuario.Nombre} ({rolNormalizado})");

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Nombre ?? usuario.Email),
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Role, rolNormalizado)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddHours(8)
                });

                Console.WriteLine($"Login exitoso. Redirigiendo segun rol: {rolNormalizado}");

                return rolNormalizado switch
                {
                    "Admin" => RedirectToAction("Index", "Analisis"),
                    "Cocinero" => RedirectToAction("Index", "Platos"),
                    "Cadete" or "Cliente" => RedirectToAction("Login", "Auth", new { errorMessage = "Este acceso es solo para el panel web. Ingrese desde la app movil." }),
                    _ => RedirectToAction("Login", "Auth", new { errorMessage = "Rol no reconocido. Contacte al administrador." })
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepcion en Login: {ex}");
                ViewBag.Error = "Ocurrio un problema al iniciar sesion. Intente nuevamente.";
                return View("~/Views/Auth/Login.cshtml");
            }
        }

        [HttpGet]
        [Route("Auth/Register")]
        public IActionResult Register()
        {
            return View("~/Views/Auth/Register.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Auth/Register")]
        public async Task<IActionResult> Register(Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Debe completar todos los campos.";
                return View("~/Views/Auth/Register.cshtml");
            }

            usuario.Rol = NormalizarRol(usuario.Rol);

            try
            {
                await _authService.RegistrarAsync(usuario);
                TempData["MensajeExito"] = "Usuario registrado correctamente.";
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return View("~/Views/Auth/Register.cshtml");
            }
        }

        [HttpGet]
        [Route("Auth/Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }

        private async Task<Usuario?> GetUserByEmailAsync(string email)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
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
