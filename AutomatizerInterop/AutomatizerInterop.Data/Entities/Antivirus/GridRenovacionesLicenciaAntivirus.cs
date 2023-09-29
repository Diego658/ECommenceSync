using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Entities.Antivirus
{
    public class GridRenovacionesLicenciaAntivirus
    {
        public string CodigoFactura { get; set; }
        public int NumeroFactura { get; set; }
        public string ReferenciaAntivirus { get; set; }
        public string NombreAntivirus { get; set; }
        public string NombreCliente { get; set; }
        public DateTime FechaVencimiento { get; set; }
    }
}
