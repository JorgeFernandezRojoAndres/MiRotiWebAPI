using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiRoti.Models
{
    [Table("Pedido")]
    public class Pedido
    {
        [Key]
        public int Id { get; set; }

        public DateTime FechaHora { get; set; }

        [Required, MaxLength(50)]
        public required string Estado { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        // Relaciones
        public int ClienteId { get; set; }
        [ForeignKey(nameof(ClienteId))]
        public required Cliente Cliente { get; set; }

        public int? CadeteId { get; set; }
        [ForeignKey(nameof(CadeteId))]
        public Cadete? Cadete { get; set; }  // puede ser null si aún no se asignó

        public List<DetallePedido> Detalles { get; set; } = new();
    }
}
