using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AutomatizerInterop.Data.Entities.Inventario
{
    [Table("StoreSync_AtributosValores")]
    public class AtributoValor
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [ForeignKey("Atributo")]
        public int AtributoId { get; set; }
        public virtual Atributo Atributo { get; set; }
        [Required]
        [StringLength(150)]
        public string Valor { get; set; }

        [Required]
        public int Orden { get; set; }

        public string Color { get; set; }

        public bool Activo { get; set; }

        [Timestamp]
        public byte[] Timestamp { get; set; }

        public DateTimeOffset? FechaModificacion { get; set; }
    }
}
