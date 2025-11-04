using System;
using System.Collections.Generic;

namespace MiRoti.DTOs
{
    // ===========================================================
    // 🥘 DTO para Plato  (Tu código original, sin cambios)
    // ===========================================================
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

    // ===========================================================
    // 🧂 DTO para Ingrediente  (Tu código original, sin cambios)
    // ===========================================================
    public class IngredienteDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public double Cantidad { get; set; }
        public decimal Subtotal { get; set; }
        public string Unidad { get; set; } = string.Empty;
    }

    // (Removed duplicate PedidoDTO class definition)

    public class DetallePedidoInfoDTO
    {
        public string Plato { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
    }
}
