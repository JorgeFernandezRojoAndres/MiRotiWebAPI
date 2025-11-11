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
            // Verificación de campos vacíos
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(contrasenia))
            {
                TempData["Error"] = "Debe ingresar un email y una contraseña.";
                return View("~/Views/Auth/Login.cshtml");
            }

            try
            {
                // 🔹 Obtener usuario por email
                var usuario = await GetUserByEmailAsync(email);
                if (usuario == null || !BCrypt.Net.BCrypt.Verify(contrasenia, usuario.Contrasenia))
                {
                    TempData["Error"] = "Email o contraseña incorrectos.";
                    return View("~/Views/Auth/Login.cshtml");
                }

                // ✅ Generar token JWT
                var token = await _authService.AutenticarAsync(email, contrasenia);
                if (token == null)
                {
                    TempData["Error"] = "Error al generar el token.";
                    return View("~/Views/Auth/Login.cshtml");
                }

                // ✅ Guardar datos de sesión
                HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);
                HttpContext.Session.SetString("UsuarioRol", usuario.Rol);
                HttpContext.Session.SetString("TokenJWT", token);

                // 🧩 Crear cookie de autenticación para MVC
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Nombre ?? usuario.Email),
                    new Claim(ClaimTypes.Role, usuario.Rol ?? "Cliente")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
                {
                    IsPersistent = true, // Mantiene sesión si cierra navegador
                    ExpiresUtc = DateTime.UtcNow.AddHours(8)
                });

                // ✅ Redirigir según el rol
                return usuario.Rol switch
                {
                    "Admin" => RedirectToAction("Index", "Analisis"),
                    "Cocinero" => RedirectToAction("Index", "Platos"),
                    "Cadete" or "Cliente" => RedirectToAction("Login", "Auth", new { errorMessage = "📱 Este acceso es solo para el panel web. Ingresá desde la app móvil." }),
                    _ => RedirectToAction("Login", "Auth", new { errorMessage = "Rol no reconocido. Contacte al administrador." })
                };
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al iniciar sesión: {ex.Message}";
                return View("~/Views/Auth/Login.cshtml");
            }
        }

        // ✅ GET: /Auth/Register (solo si lo usas)
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
                TempData["Error"] = "Debe completar todos los campos.";
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
                TempData["Error"] = $"❌ Error: {ex.Message}";
                return View("~/Views/Auth/Register.cshtml");
            }
        }

        // ✅ GET: /Auth/Logout
        [HttpGet]
        [Route("Auth/Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); // Cierra cookie de autenticación
            HttpContext.Session.Clear(); // Limpia la sesión
            return RedirectToAction("Login", "Auth");
        }

        // 🔹 Método para obtener usuario por email
        private async Task<Usuario?> GetUserByEmailAsync(string email)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
