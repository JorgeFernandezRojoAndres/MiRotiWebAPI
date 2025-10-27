using System.Linq;
using MiRoti.Models;

namespace MiRoti.Data
{
    public static class DbInitializer
    {
        public static void Initialize(MiRotiContext context)
        {
            context.Database.EnsureCreated();

            // Si ya hay datos, salir
            if (context.Usuarios.Any())
                return;

            // Usuarios base
            var admin = new Usuario
            {
                Nombre = "Administrador",
                Email = "admin@miroti.com",
                Contrasenia = "admin123",
                Rol = "Administrador"
            };
            context.Usuarios.Add(admin);

            // Clientes de ejemplo
            var cliente1 = new Cliente
            {
                Nombre = "Juan Pérez",
                Email = "juan@mail.com",
                Contrasenia = "1234",
                Rol = "Cliente",
                Direccion = "Av. Libertador 456",
                Telefono = "2664000001"
            };
            var cliente2 = new Cliente
            {
                Nombre = "Ana García",
                Email = "ana@mail.com",
                Contrasenia = "1234",
                Rol = "Cliente",
                Direccion = "San Martín 789",
                Telefono = "2664000002"
            };
            context.Clientes.AddRange(cliente1, cliente2);

            // Cadete
            var cadete = new Cadete
            {
                Nombre = "Pedro López",
                Email = "pedro@mail.com",
                Contrasenia = "1234",
                Rol = "Cadete",
                MedioTransporte = "Moto"
            };
            context.Cadetes.Add(cadete);

            // Ingredientes base
            var ingredientes = new[]
            {
                new Ingrediente { Nombre = "Papa", CostoUnitario = 150 },
                new Ingrediente { Nombre = "Pollo", CostoUnitario = 1200 },
                new Ingrediente { Nombre = "Aceite", CostoUnitario = 900 },
                new Ingrediente { Nombre = "Huevo", CostoUnitario = 80 }
            };
            context.Ingredientes.AddRange(ingredientes);

            // Platos base
            var platos = new[]
            {
                new Plato { Nombre = "Milanesa con Papas", Descripcion = "Clásica", PrecioVenta = 3000, CostoTotal = 2000, Disponible = true },
                new Plato { Nombre = "Pollo al Horno", Descripcion = "Con guarnición", PrecioVenta = 3500, CostoTotal = 2300, Disponible = true }
            };
            context.Platos.AddRange(platos);

            context.SaveChanges();
        }
    }
}
