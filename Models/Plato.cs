using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiRoti.Models
{
    [Table("Plato")]
    public class Plato
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioVenta { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal CostoTotal { get; set; }

        public bool Disponible { get; set; } = true;

        // 🔹 Nueva propiedad para almacenar la URL o ruta de la imagen
        [MaxLength(255)]
        public string? ImagenUrl { get; set; }

        
        // 🔹 Relación muchos a muchos con ingredientes
        public ICollection<PlatoIngrediente> PlatoIngredientes { get; set; } = new List<PlatoIngrediente>();

        // 🔹 Relación uno a muchos con DetallePedido (no directamente con Pedido)
        public ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();

    }
}
