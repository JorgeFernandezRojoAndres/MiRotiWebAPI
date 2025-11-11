using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiRoti.Models
{
    [Table("DetallePedido")]
    public class DetallePedido
    {
        [Key]
        public int Id { get; set; }

        // 🔹 Relación con Pedido (uno a muchos)
        [Required]
        public int PedidoId { get; set; }

        [ForeignKey(nameof(PedidoId))]
        public Pedido Pedido { get; set; } = null!;

        // 🔹 Relación con Plato (uno a muchos)
        [Required]
        public int PlatoId { get; set; }

        [ForeignKey(nameof(PlatoId))]
        public Plato Plato { get; set; } = null!;

        // 🔹 Cantidad de platos en el pedido
        [Required]
        public int Cantidad { get; set; }

        // 🔹 Subtotal del plato (Cantidad * Precio del Plato)
        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }
    }
}
