using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiRoti.Data;
using MiRoti.Models;
using Microsoft.AspNetCore.Authorization;

namespace MiRoti.Controllers
{
    // 🔐 Controlador accesible por Admin y Cocinero
    [Authorize(Roles = "Admin,Cocinero")]
    public class PedidosController : Controller
    {
        private readonly MiRotiContext _context;

        public PedidosController(MiRotiContext context)
        {
            _context = context;
        }

        // 🔹 Listado de pedidos con cliente y cadete relacionados
        public async Task<IActionResult> Index()
        {
            // Traer todos los pedidos, sin filtrar por estado
            var pedidos = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Cadete)
                .OrderByDescending(p => p.FechaHora) // Ordenar por fecha de manera descendente
                .ToListAsync();

            // Mostrar los pedidos en la consola del servidor para depuración
            Console.WriteLine("Pedidos recibidos desde la base de datos:");
            foreach (var pedido in pedidos)
            {
                Console.WriteLine($"Pedido ID: {pedido.Id}, Estado: {pedido.Estado}");
            }

            // 🧩 Métricas para el dashboard (con tolerancia a diferentes variantes de texto)
            var totalPedidos = pedidos.Count;

            var entregados = pedidos.Count(p =>
                p.Estado.Equals("Entregado", StringComparison.OrdinalIgnoreCase) ||
                p.Estado.Equals("Completado", StringComparison.OrdinalIgnoreCase));

            var enCamino = pedidos.Count(p =>
                p.Estado.Equals("En camino", StringComparison.OrdinalIgnoreCase) ||
                p.Estado.Equals("EnCamino", StringComparison.OrdinalIgnoreCase));

            var nuevos = pedidos.Count(p =>
                p.Estado.Equals("Nuevo", StringComparison.OrdinalIgnoreCase) ||
                p.Estado.Equals("Preparando", StringComparison.OrdinalIgnoreCase) ||
                p.Estado.Equals("En preparación", StringComparison.OrdinalIgnoreCase));

            var totalGanancia = pedidos.Sum(p => p.Total);

            // 📊 Pasar métricas a la vista
            ViewData["TotalPedidos"] = totalPedidos;
            ViewData["Entregados"] = entregados;
            ViewData["EnCamino"] = enCamino;
            ViewData["Nuevos"] = nuevos;
            ViewData["TotalGanancia"] = totalGanancia;

            // Mostrar mensaje de éxito si existe
            ViewBag.SuccessMessage = TempData["SuccessMessage"];

            return View(pedidos); // Pasar todos los pedidos a la vista
        }

        // 🔹 Acción para editar el estado de un pedido (GET)
        [HttpGet]
        public async Task<IActionResult> EditarEstado(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Cadete)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
            {
                return NotFound(); // Si no se encuentra el pedido
            }

            // Pasamos el pedido a la vista
            return View(pedido);
        }

        // 🔹 Acción para actualizar el estado del pedido (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarEstado(int id, string nuevoEstado)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
            {
                return NotFound(); // Si no se encuentra el pedido
            }

            // Validación: Prohibir transición de 'Nuevo' a 'Completado'
            if (pedido.Estado == "Nuevo" && nuevoEstado == "Completado")
            {
                ModelState.AddModelError("Estado", "No puedes cambiar el estado directamente de 'Nuevo' a 'Completado'.");
                return View(pedido);
            }

            // Validación para flujo de estados
            if (string.IsNullOrWhiteSpace(nuevoEstado))
            {
                ModelState.AddModelError("Estado", "El estado no puede estar vacío.");
                return View(pedido);
            }

            // Actualizamos el estado del pedido
            pedido.Estado = nuevoEstado;

            // Guardamos los cambios
            await _context.SaveChangesAsync();

            // Mensaje de éxito
            TempData["SuccessMessage"] = "Estado actualizado correctamente.";

            // Redirigimos a la lista de pedidos
            return RedirectToAction(nameof(Index));
        }
    }
}
