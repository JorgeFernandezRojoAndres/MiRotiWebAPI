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

            // 🔹 Unidades de medida
            var unidades = new[]
            {
                new UnidadMedida { Nombre = "Kilogramo", Abreviatura = "kg" },
                new UnidadMedida { Nombre = "Gramo", Abreviatura = "g" },
                new UnidadMedida { Nombre = "Unidad", Abreviatura = "u" },
                new UnidadMedida { Nombre = "Litro", Abreviatura = "L" }
            };
            context.UnidadesMedida.AddRange(unidades);
            context.SaveChanges();

            // 🔹 Usuarios base
            var admin = new Usuario
            {
                Nombre = "Administrador",
                Email = "admin@miroti.com",
                Contrasenia = "admin123",
                Rol = "Administrador"
            };
            context.Usuarios.Add(admin);

            // 🔹 Cocinero base
            var cocinero = new Cocinero
            {
                Nombre = "Chef Admin",
                Email = "chef@miroti.com",
                Contrasenia = "123456",
                Rol = "Cocinero",
                Especialidad = "Comidas Caseras"
            };
            context.Cocineros.Add(cocinero);

            // 🔹 Clientes de ejemplo
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

            // 🔹 Cadete base
            var cadete = new Cadete
            {
                Nombre = "Pedro López",
                Email = "pedro@mail.com",
                Contrasenia = "1234",
                Rol = "Cadete",
                MedioTransporte = "Moto"
            };
            context.Cadetes.Add(cadete);

            context.SaveChanges();

            // 🔹 Ingredientes base (vinculados a unidades)
            var ingredientes = new[]
            {
                new Ingrediente { Nombre = "Papa", CostoUnitario = 150, UnidadMedidaId = unidades[1].Id },
                new Ingrediente { Nombre = "Pollo", CostoUnitario = 1200, UnidadMedidaId = unidades[0].Id },
                new Ingrediente { Nombre = "Aceite", CostoUnitario = 900, UnidadMedidaId = unidades[3].Id },
                new Ingrediente { Nombre = "Huevo", CostoUnitario = 80, UnidadMedidaId = unidades[2].Id }
            };
            context.Ingredientes.AddRange(ingredientes);

            // 🔹 Platos base
            var platos = new[]
            {
                new Plato { Nombre = "Milanesa con Papas", Descripcion = "Clásica", PrecioVenta = 3000, CostoTotal = 2000, Disponible = true },
                new Plato { Nombre = "Pollo al Horno", Descripcion = "Con guarnición", PrecioVenta = 3500, CostoTotal = 2300, Disponible = true }
            };
            context.Platos.AddRange(platos);
            context.SaveChanges();

            // 🔹 Relaciones Plato ↔ Ingredientes (tabla intermedia)
            var relaciones = new[]
            {
                new PlatoIngrediente { PlatoId = platos[0].Id, IngredienteId = ingredientes[0].Id, Cantidad = 0.3, Subtotal = 45 },
                new PlatoIngrediente { PlatoId = platos[0].Id, IngredienteId = ingredientes[3].Id, Cantidad = 1, Subtotal = 80 },
                new PlatoIngrediente { PlatoId = platos[1].Id, IngredienteId = ingredientes[1].Id, Cantidad = 0.5, Subtotal = 600 },
                new PlatoIngrediente { PlatoId = platos[1].Id, IngredienteId = ingredientes[0].Id, Cantidad = 0.2, Subtotal = 30 }
            };
            context.PlatosIngredientes.AddRange(relaciones);

            context.SaveChanges();
        }
    }
}
