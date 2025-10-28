using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiRoti.Models
{
    [Table("DetallePedido")]
    public class DetallePedido
    {
        [Key]
        public int Id { get; set; }

        public int PedidoId { get; set; }
        [ForeignKey(nameof(PedidoId))]
        public required Pedido Pedido { get; set; }

        public int PlatoId { get; set; }
        [ForeignKey(nameof(PlatoId))]
        public required Plato Plato { get; set; }

        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }
    }
}
