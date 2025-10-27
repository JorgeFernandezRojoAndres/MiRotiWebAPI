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

        // Tablas
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Cadete> Cadetes { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetallePedido> DetallesPedido { get; set; }
        public DbSet<Plato> Platos { get; set; }
        public DbSet<Ingrediente> Ingredientes { get; set; }
        public DbSet<UnidadMedida> UnidadesMedida { get; set; }          // 🆕
        public DbSet<PlatoIngrediente> PlatosIngredientes { get; set; }  // 🆕

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de herencia
            modelBuilder.Entity<Usuario>()
                .HasDiscriminator<string>("TipoUsuario")
                .HasValue<Usuario>("Usuario")
                .HasValue<Cliente>("Cliente")
                .HasValue<Cadete>("Cadete");

            // Relaciones Pedido ↔ DetallePedido
            modelBuilder.Entity<DetallePedido>()
                .HasOne(d => d.Pedido)
                .WithMany(p => p.Detalles)
                .HasForeignKey(d => d.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relaciones DetallePedido ↔ Plato
            modelBuilder.Entity<DetallePedido>()
                .HasOne(d => d.Plato)
                .WithMany()
                .HasForeignKey(d => d.PlatoId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ Relación N:N entre Plato e Ingrediente (tabla intermedia)
            modelBuilder.Entity<PlatoIngrediente>()
                .HasKey(pi => new { pi.PlatoId, pi.IngredienteId }); // clave compuesta

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

            // ✅ Relación UnidadMedida ↔ Ingrediente
            modelBuilder.Entity<Ingrediente>()
                .HasOne(i => i.UnidadMedida)
                .WithMany(u => u.Ingredientes)
                .HasForeignKey(i => i.UnidadMedidaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices y restricciones
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
