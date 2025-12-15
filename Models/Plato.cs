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

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MinLength(1, ErrorMessage = "El nombre no puede estar vacío.")]
        [MaxLength(100)]
        public string Nombre { get; set; } = "";

        public string Descripcion { get; set; } = string.Empty;

        [Range(1, double.MaxValue, ErrorMessage = "El precio de venta debe ser mayor a 0.")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioVenta { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "El costo total debe ser mayor a 0.")]
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
