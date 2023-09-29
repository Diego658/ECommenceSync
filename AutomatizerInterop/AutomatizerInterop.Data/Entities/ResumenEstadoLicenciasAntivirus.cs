using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Entities
{
    public class ResumenEstadoLicenciasAntivirus
    {
        public string Grupo { get; set; }
        public int Cantidad { get; set; }
    }


    public class EmpresaResumenEstadoLicenciasAntivirus
    {
        public int ConfiguracionId { get; set; }
        public string NombreConfiguracion { get; set; }
        public List<ResumenEstadoLicenciasAntivirus> Estados { get; set; }
    }



}
