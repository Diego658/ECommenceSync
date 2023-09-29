using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Entities.Facturacion
{
    public class InformacionFactura
    {
        public string TransaccionCodigo { get; set; }
        public int Numero { get; set; }
        public string ClienteCodigo { get; set; }
        public string Estado { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
        public string Observaciones { get; set; }
        public byte SucursalClienteCodigo { get; set; }
        public int EmpleadoId { get; set; }
        public string NumeroSerie { get; set; }
        public string NumeroAutorizacion { get; set; }
    }
}
