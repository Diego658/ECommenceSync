using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutomatizerInterop.WebApi.Models
{
    public class AntivirusPlantillaNotificacionEditModel
    {
        public byte PlantillaID { get; set; }
        public string NombrePlantilla { get; set; }
        public string Contenido { get; set; }
        public string ContenidoHtml { get; set; }
    }
}
