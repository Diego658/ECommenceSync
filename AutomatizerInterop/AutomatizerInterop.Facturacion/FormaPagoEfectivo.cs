using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Facturacion
{
    public class FormaPagoEfectivo:FormaPago
    {
        public override FormasPago Forma { get;  }

        public FormaPagoEfectivo()
        {
            Forma = FormasPago.Contado;
        }

        
    }
}
