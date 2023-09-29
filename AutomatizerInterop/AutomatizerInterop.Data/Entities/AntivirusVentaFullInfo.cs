using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Entities
{
    public class AntivirusVentaFullInfo:AntivirusVenta
    {
        public string GrupoVencimiento { get; set; }

        public short GrupoVencimientoNumero { get; set; }

        public string TransaccionVenta { get; set; }

        public string IdentificacionCliente { get; set; }

        public string CodigoCliente { get; set; }

        public string Cliente { get; set; }

        public string EmailCliente { get; set; }

        public string Producto { get; set; }

        public string CodigoVenta { get; set; }

        public int Cantidad { get; set; }

        public Decimal Precio { get; set; }

        public string ObservacionesNotificacion { get; set; }

        public string Series { get; set; }

        public int AntivirusId { get; set; }
    }
}
