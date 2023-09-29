using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Facturacion
{
    public abstract class FormaPago
    {
        public abstract FormasPago Forma { get; }
        public decimal Valor { get; set; }

    }
}
