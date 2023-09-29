using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Entities
{
    public class FacturacionElectronicaFactura
    {
        public int IDDocumento { get; set; }
        public DateTime Fecha { get; set; }
        public string Transaccion { get; set; }

        public string SerieDocumento { get; set; }
        public int Numero { get; set; }
        public string Cliente { get; set; }
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
        public virtual bool RideGenerado { get; set; }
        public virtual bool XmlGenerado { get; set; }

        public bool EstadoSistema { get; set; }


    }
}
