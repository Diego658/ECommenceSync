using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Facturacion
{
    public class FormaPagoCredito : FormaPago
    {
        public override FormasPago Forma { get; }
        public int NumeroPagos { get; set; }
        public int NumeroDias { get; set; }
        public FormaPagoCredito()
        {
            Forma = FormasPago.Credito;
        }

    }
}
