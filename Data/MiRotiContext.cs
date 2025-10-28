using Microsoft.EntityFrameworkCore;
using MiRoti.Models;

namespace MiRoti.Data
{
    public class MiRotiContext : DbContext
    {
        public MiRotiContext(DbContextOptions<MiRotiContext> options)
            : base(options)
        {
        }

        // 🔹 Tablas principales
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Cadete> Cadetes { get; set; }
        public DbSet<Cocinero> Cocineros { get; set; } 
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetallePedido> DetallesPedido { get; set; }
        public DbSet<Plato> Platos { get; set; }
        public DbSet<Ingrediente> Ingredientes { get; set; }
        public DbSet<UnidadMedida> UnidadesMedida { get; set; }
        public DbSet<PlatoIngrediente> PlatosIngredientes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🧩 Herencia (TPH) — todas las subclases en la tabla Usuario
            modelBuilder.Entity<Usuario>()
                .HasDiscriminator<string>("TipoUsuario")
                .HasValue<Usuario>("Usuario")
                .HasValue<Cliente>("Cliente")
                .HasValue<Cadete>("Cadete")
                .HasValue<Cocinero>("Cocinero"); // ✅ agregado

            // 🧩 Pedido ↔ DetallePedido
            modelBuilder.Entity<DetallePedido>()
                .HasOne(d => d.Pedido)
                .WithMany(p => p.Detalles)
                .HasForeignKey(d => d.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🧩 DetallePedido ↔ Plato
            modelBuilder.Entity<DetallePedido>()
                .HasOne(d => d.Plato)
                .WithMany()
                .HasForeignKey(d => d.PlatoId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🧩 Relación N:N Plato ↔ Ingrediente
            modelBuilder.Entity<PlatoIngrediente>()
                .HasKey(pi => new { pi.PlatoId, pi.IngredienteId });

            modelBuilder.Entity<PlatoIngrediente>()
                .HasOne(pi => pi.Plato)
                .WithMany(p => p.PlatoIngredientes)
                .HasForeignKey(pi => pi.PlatoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlatoIngrediente>()
                .HasOne(pi => pi.Ingrediente)
                .WithMany(i => i.PlatoIngredientes)
                .HasForeignKey(pi => pi.IngredienteId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🧩 UnidadMedida ↔ Ingrediente
            modelBuilder.Entity<Ingrediente>()
                .HasOne(i => i.UnidadMedida)
                .WithMany(u => u.Ingredientes)
                .HasForeignKey(i => i.UnidadMedidaId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🧩 Índice único de Email
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
