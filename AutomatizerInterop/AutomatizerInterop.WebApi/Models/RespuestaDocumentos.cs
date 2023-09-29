using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutomatizerInterop.WebApi.Models
{
    public class RespuestaDocumentos<T> where T:class
    {
        public int NumeroPendientes { get; set; }
        public List<T> Documentos { get; set; }
    }
}
