using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AutomatizerInterop.Data.Entities.Inventario
{

    public  enum  TiposAtributo : byte
    {
        DropDown,
        RadioButton,
        ColorOrTexture
    }

    [Table("StoreSync_Atributos")]
    public class Atributo
    {
        [Key()]
        public int Id { get; set; }
        [Required]
        [StringLength(2)]
        public string EmpCod { get; set; }
        [Required]
        [StringLength(250)]
        public string Name { get; set; }
        [StringLength(250)]
        public string PublicName { get; set; }
        [Required]
        public TiposAtributo AttributeType { get; set; }

        [Timestamp]
        public byte[] Timestamp { get; set; }

        public DateTimeOffset? FechaModificacion { get; set; }

        public bool Activo { get; set; }
        public virtual List<AtributoValor> Valores {get;set;}

    }
}
