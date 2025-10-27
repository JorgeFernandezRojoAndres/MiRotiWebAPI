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
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioVenta { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal CostoTotal { get; set; }

        public bool Disponible { get; set; } = true;
    }
}
