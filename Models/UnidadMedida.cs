using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiRoti.Models
{
    [Table("UnidadMedida")]
    public class UnidadMedida
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string Abreviatura { get; set; } = string.Empty;

        // Relación 1-N con Ingrediente
        public ICollection<Ingrediente> Ingredientes { get; set; } = new List<Ingrediente>();
    }
}
