using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutomatizerInterop.WebApi.Models
{
    public class DocumentosPendientes
    {
        public int ConfiguracionId { get; set; }
        public string ConfiguracionName { get; set; }
        public int Factura { get; set; }
        public int NotaCredito { get; set; }
        public int Retencion { get; set; }
    }
}
