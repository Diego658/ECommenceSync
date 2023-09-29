using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Entities
{
    public class FacturacionElectronicaRetencion
    {
        public int IDDocumento { get; set; }
        public DateTime Fecha { get; set; }
        public string Serie { get; set; }
        public int Numero { get; set; }
        public string Proveedor { get; set; }
        public string Estado
        {
            get
            {
                switch (EstadoSri)
                {
                    case 0:
                        return "ProcesoNoIniciado";
                    case 1:
                        return "Enviado";
                    case 2:
                        return "Rechazado";
                    case 3:
                        return "Autorizado";
                    case 4:
                        return "NoAutorizado";
                    case 5:
                        return "Anulado";
                    case 70:
                        return "EnProcesamiento";
                    case 255:
                        return "NoMigrado";
                    default:
                        return "ERROR";
                }
            }
        }
        public byte EstadoSri { get; set; }

        public string EstadoSistema { get; set; }
        public string CodigoTransaccionCompra { get; set; }
        public int NumeroCompra { get; set; }
        public string NumeroFacturaCompra { get; set; }
        public string TransaccionCompra { get; set; }
        public bool RideGenerado { get; set; }
        public bool XmlGenerado { get; set; }
        public bool NotificadoXMail { get; set; }
        public string DireccionNotificacion { get; set; }
        public string ClaveAcceso { get; set; }
    }
}
