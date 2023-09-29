using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Entities
{
    public class ImagenItem
    {
        public int Id { get; set; }
        public int ItemID { get; set; }
        public string Type { get; set; }
        public int BlobID { get; set; }
        public DateTimeOffset FechaModificacion { get; set; }
        public string Operacion { get; set; }
        public string ImageAlt { get; set; }
    }
}
