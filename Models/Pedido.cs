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
        public string Estado { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        // Relaciones
        public int ClienteId { get; set; }
        [ForeignKey(nameof(ClienteId))]
        public Cliente Cliente { get; set; }

        public int? CadeteId { get; set; }
        [ForeignKey(nameof(CadeteId))]
        public Cadete Cadete { get; set; }

        public ICollection<DetallePedido> Detalles { get; set; }
    }
}
