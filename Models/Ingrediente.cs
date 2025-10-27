using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiRoti.Models
{
    [Table("Ingrediente")]
    public class Ingrediente
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal CostoUnitario { get; set; }

        // 🔗 Relación con UnidadMedida
        public int UnidadMedidaId { get; set; }

        [ForeignKey(nameof(UnidadMedidaId))]
        public UnidadMedida UnidadMedida { get; set; } = new UnidadMedida();


        // 🔗 Relación muchos a muchos con Plato
        public ICollection<PlatoIngrediente> PlatoIngredientes { get; set; } = new List<PlatoIngrediente>();
    }
}
