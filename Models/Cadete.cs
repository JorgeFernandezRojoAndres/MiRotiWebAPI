using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MiRoti.Models
{
    // ❌ Quitamos [Table("Cadete")] para mantener herencia TPH (tabla Usuario)
    public class Cadete : Usuario
    {
        [MaxLength(50)]
        public string? MedioTransporte { get; set; }

        [MaxLength(200)]
        public string? Direccion { get; set; }

        [MaxLength(20)]
        public new string? Telefono { get; set; }

        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}
