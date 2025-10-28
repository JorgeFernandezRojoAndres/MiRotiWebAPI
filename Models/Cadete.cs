using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiRoti.Models
{
    // ❌ Quitamos [Table("Cadete")] para mantener herencia TPH (tabla Usuario)
    public class Cadete : Usuario
    {
        [MaxLength(50)]
        public string? MedioTransporte { get; set; }

        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}
