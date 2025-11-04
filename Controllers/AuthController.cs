using Microsoft.AspNetCore.Mvc;
using MiRoti.Data;
using MiRoti.Models;
using MiRoti.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
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

        // ✅ GET: /Auth/Login
        [HttpGet]
        [Route("Auth/Login")]
        public IActionResult Login()
        {
            return View("~/Views/Auth/Login.cshtml");
        }

        // ✅ POST: /Auth/Login (BCrypt + JWT + Cookie)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Auth/Login")]
        public async Task<IActionResult> Login(string email, string contrasenia)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(contrasenia))
            {
                ViewBag.Error = "Debe ingresar un email y una contraseña.";
                return View("~/Views/Auth/Login.cshtml");
            }

            try
            {
                // 🔹 Buscar usuario por email
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
                if (usuario == null)
                {
                    ViewBag.Error = "Email o contraseña incorrectos.";
                    return View("~/Views/Auth/Login.cshtml");
                }

                // 🔹 Verificar contraseña con BCrypt
                bool esValido = BCrypt.Net.BCrypt.Verify(contrasenia, usuario.Contrasenia);
                if (!esValido)
                {
                    ViewBag.Error = "Email o contraseña incorrectos.";
                    return View("~/Views/Auth/Login.cshtml");
                }

                // ✅ Generar token JWT (para app móvil)
                var token = await _authService.AutenticarAsync(email, contrasenia);
                if (token == null)
                {
                    ViewBag.Error = "Error al generar el token.";
                    return View("~/Views/Auth/Login.cshtml");
                }

                // ✅ Guardar datos de sesión
                HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);
                HttpContext.Session.SetString("UsuarioRol", usuario.Rol);
                HttpContext.Session.SetString("TokenJWT", token);

                // 🧩 Crear cookie de autenticación (para MVC)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Nombre ?? usuario.Email),
                    new Claim(ClaimTypes.Role, usuario.Rol ?? "Cliente")
                };

                var identity = new ClaimsIdentity(claims, "Cookies");
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("Cookies", principal, new AuthenticationProperties
                {
                    IsPersistent = true, // mantiene sesión si cierra navegador
                    ExpiresUtc = DateTime.UtcNow.AddHours(8)
                });

                // ✅ Redirigir según el rol
                switch (usuario.Rol)
                {
                    case "Admin":
                        return RedirectToAction("Index", "Analisis");

                    case "Cocinero":
                        return RedirectToAction("Index", "Platos");

                    case "Cadete":
                    case "Cliente":
                        TempData["MensajeApp"] = "📱 Este acceso es solo para el panel web. Ingresá desde la app móvil.";
                        await HttpContext.SignOutAsync("Cookies");
                        HttpContext.Session.Clear();
                        return RedirectToAction("Login", "Auth");

                    default:
                        ViewBag.Error = "Rol no reconocido. Contacte al administrador.";
                        return View("~/Views/Auth/Login.cshtml");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error al iniciar sesión: {ex.Message}";
                return View("~/Views/Auth/Login.cshtml");
            }
        }

        // ✅ GET: /Auth/Register (solo si lo usás)
        [HttpGet]
        [Route("Auth/Register")]
        public IActionResult Register()
        {
            return View("~/Views/Auth/Register.cshtml");
        }

        // ✅ POST: /Auth/Register (con hash)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Auth/Register")]
        public async Task<IActionResult> Register(Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Debe completar todos los campos.";
                return View("~/Views/Auth/Register.cshtml");
            }

            try
            {
                await _authService.RegistrarAsync(usuario);
                TempData["MensajeExito"] = "✅ Usuario registrado correctamente.";
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"❌ Error: {ex.Message}";
                return View("~/Views/Auth/Register.cshtml");
            }
        }

        // ✅ GET: /Auth/Logout
        [HttpGet]
        [Route("Auth/Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies"); // cierra cookie de autenticación
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }
    }
}
