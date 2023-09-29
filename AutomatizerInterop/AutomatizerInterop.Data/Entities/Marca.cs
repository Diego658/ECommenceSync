using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Entities
{
    public class Marca
    {
        public int Codigo{ get; set; }
        public string Nombre { get; set; }
        public string DescriptionShort { get; set; }
        public string Description { get; set; }
        public long LogoBlobId { get; set; }
        public int ID { get; set; }
    }
}