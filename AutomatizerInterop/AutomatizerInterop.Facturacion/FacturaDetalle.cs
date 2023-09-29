using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Facturacion
{
    public abstract class FacturaDetalle
    {
        public int Secuencial { get; set; }
        public string Codigo { get; set; }
        public TipoDetalle Tipo { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Descuento { get; set; }
        public decimal TotalDescuento { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }
        public string CodigoBodega { get; set; }
        public string Observaciones { get; set; }
        public bool TieneIva { get; set; }

        public string CuentaVenta { get; set; }
        public string CuentaVentaIva { get; set; }
        public string CuentaDescuentoVenta { get; set; }
        public string CuentaDescuentoVentaIva { get; set; }

    }

}
