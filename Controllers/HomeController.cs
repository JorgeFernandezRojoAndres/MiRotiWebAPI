using Microsoft.AspNetCore.Mvc;

namespace MiRoti.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return Content("🍗 Bienvenido a MiRoti API — Sistema de gestión para rotiserías.");
        }
    }
}
