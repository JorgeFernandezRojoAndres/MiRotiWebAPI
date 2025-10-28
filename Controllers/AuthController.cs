using Microsoft.AspNetCore.Mvc;
using MiRoti.Data;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace MiRoti.Controllers
{
    public class AuthController : Controller
    {
        private readonly MiRotiContext _context;

        public AuthController(MiRotiContext context)
        {
            _context = context;
        }

        // ✅ GET: /Auth/Login
        [HttpGet]
        [Route("Auth/Login")]
        public IActionResult Login()
        {
            return View("~/Views/Auth/Login.cshtml");
        }

        // ✅ POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Auth/Login")]
        public IActionResult Login(string email, string contrasenia)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(contrasenia))
            {
                ViewBag.Error = "Debe ingresar un email y una contraseña.";
                return View("~/Views/Auth/Login.cshtml");
            }

            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.Email == email && u.Contrasenia == contrasenia);

            if (usuario == null)
            {
                ViewBag.Error = "Email o contraseña incorrectos.";
                return View("~/Views/Auth/Login.cshtml");
            }

            // ✅ Guardar datos de sesión
            HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);
            HttpContext.Session.SetString("UsuarioRol", usuario.Rol);

            // ✅ Redirigir según el rol
            switch (usuario.Rol)
            {
                case "Admin":
                    return RedirectToAction("Index", "Analisis");

                case "Cocinero":
                    return RedirectToAction("Index", "Pedidos");

                case "Cadete":
                case "Cliente":
                    TempData["MensajeApp"] = "📱 Este acceso es solo para el panel web. Ingresá desde la app móvil.";
                    HttpContext.Session.Clear();
                    return RedirectToAction("Login", "Auth");

                default:
                    ViewBag.Error = "Rol no reconocido. Contacte al administrador.";
                    return View("~/Views/Auth/Login.cshtml");
            }
        }

        // ✅ GET: /Auth/Logout
        [HttpGet]
        [Route("Auth/Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }
    }
}
