using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AutomatizerInterop.Data.Entities.Inventario
{
    [Table("StoreSync_ImagenesItems")]
    public class ImagenItem
    {
        [Required]
        public int ItemId { get; set; }
        public virtual Item Item { get; set; }
        [Required]
        public string Type { get; set; }
        public long BlobID { get; set; }
        public DateTimeOffset FechaModificacion { get; set; }
        public string Operacion { get; set; }
        public string ImageAlt { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
    }
}
