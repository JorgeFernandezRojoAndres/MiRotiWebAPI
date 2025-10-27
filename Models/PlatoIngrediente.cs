using System.ComponentModel.DataAnnotations.Schema;

namespace MiRoti.Models
{
    [Table("PlatoIngrediente")]
    public class PlatoIngrediente
    {
        public int PlatoId { get; set; }
        public Plato Plato { get; set; } = new Plato();

        public int IngredienteId { get; set; }
        public Ingrediente Ingrediente { get; set; } = new Ingrediente();

        public double Cantidad { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }
    }
}
