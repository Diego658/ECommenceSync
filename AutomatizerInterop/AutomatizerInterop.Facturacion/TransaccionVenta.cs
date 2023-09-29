using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Facturacion
{
    public class TransaccionVenta
    {
        public string Codigo { get; set; }
        public string IngCod { get; set; }
        public bool IngAut { get; set; }
        public int DiasGracia { get; set; }
        public string CuentaCredito { get; set; }
        public string CuentaCaja { get; set; }
        public int TipCmp { get; set; }
    }
}
