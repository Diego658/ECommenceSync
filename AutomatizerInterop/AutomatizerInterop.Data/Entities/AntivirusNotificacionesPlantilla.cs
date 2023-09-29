using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Entities
{
    public class AntivirusNotificacionesPlantilla
    {
        public string EmpCod { get; set; }
        public byte PlantillaID { get; set; }
        public string NombrePlantilla { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string UsuarioCrea { get; set; }
        public DateTime FechaModificacion { get; set; }
        public string UsuarioModifica { get; set; }
        public string Contenido { get; set; }
        public string ContenidoHtml { get; set; }

    }
}
