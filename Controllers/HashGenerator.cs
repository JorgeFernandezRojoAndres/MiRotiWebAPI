using System;
using BCrypt.Net;

namespace MiRoti.Controllers
{
    // ⚙️ Clase temporal para generar hashes BCrypt
    // Podés ejecutarla una vez para obtener los hashes reales de tus contraseñas
    public class HashGenerator
    {
        public static void Ejecutar()

        {
            Console.WriteLine("=== Generador de Hashes BCrypt ===");
            Console.WriteLine("Usá estos hashes para actualizar tus contraseñas en la tabla Usuario.\n");

            // 🔐 Contraseñas base que querés encriptar
            string[] contrasenias = { "admin123", "chef123", "cliente123", "cadete123" };

            foreach (var pass in contrasenias)
            {
                string hash = BCrypt.Net.BCrypt.HashPassword(pass);
                Console.WriteLine($"{pass} → {hash}");
            }

            Console.WriteLine("\n💡 Copiá los hashes generados y usalos en tu UPDATE SQL.");
            Console.WriteLine("Ejemplo:\nUPDATE Usuario SET Contrasenia = '<hash>' WHERE Email = 'admin@miroti.com';\n");
            Console.WriteLine("Presioná Enter para salir...");
            Console.ReadLine();
        }
    }
}
