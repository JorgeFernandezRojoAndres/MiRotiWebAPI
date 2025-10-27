using System;
using System.Collections.Generic;

namespace MiRoti.DTOs
{
    public class PedidoDTO
    {
        public int Id { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
        public string Estado { get; set; } = string.Empty;
        public decimal Total { get; set; }

        public List<DetallePedidoDTO> Detalles { get; set; } = new();
    }

    public class DetallePedidoDTO
    {
        public string Plato { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
    }
}
