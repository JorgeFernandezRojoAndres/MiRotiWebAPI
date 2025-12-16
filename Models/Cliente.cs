using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiRoti.Models
{
    // ❌ Quitamos [Table("Cliente")] para permitir herencia TPH en la tabla Usuario
    public class Cliente : Usuario
    {
        [Required, MaxLength(200)]
        public required string Direccion { get; set; }

        [Required, MaxLength(20)]
        public new required string Telefono { get; set; }

        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}
