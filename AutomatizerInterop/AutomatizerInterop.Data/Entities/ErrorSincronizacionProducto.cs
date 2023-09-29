using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Entities
{
    public class ErrorSincronizacionProducto
    {
        public int ItemID { get; set; }
        public string StoreName { get; set; }
        public string Tipo { get; set; }
        public string Nombre { get; set; }
        public string Referencia { get; set; }
        public string Error { get; set; }
    }
}
