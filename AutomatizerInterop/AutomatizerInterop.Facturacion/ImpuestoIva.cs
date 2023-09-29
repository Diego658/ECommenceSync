using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Facturacion
{
    public class ImpuestoIva
    {
        public byte CodigoImpuesto { get; set; }
        public string Nombre { get; set; }
        public string CuentaContable { get; set; }
        public string Formula { get; set; }
    }
}
