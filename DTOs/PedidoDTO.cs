using System.Collections.Generic;

namespace MiRoti.DTOs
{
    public class PlatoDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal PrecioVenta { get; set; }
        public decimal CostoTotal { get; set; }
        public bool Disponible { get; set; }

        // Ingredientes simplificados
        public List<IngredienteDTO> Ingredientes { get; set; } = new();
    }

    public class IngredienteDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public double Cantidad { get; set; }
        public decimal Subtotal { get; set; }
        public string Unidad { get; set; } = string.Empty;
    }
}
