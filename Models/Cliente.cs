using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiRoti.Models
{
    [Table("Cliente")]
    public class Cliente : Usuario
    {
        [Required, MaxLength(200)]
        public string? Direccion { get; set; }

        [Required, MaxLength(20)]
        public string? Telefono { get; set; }

        public ICollection<Pedido>? Pedidos { get; set; }
    }
}
