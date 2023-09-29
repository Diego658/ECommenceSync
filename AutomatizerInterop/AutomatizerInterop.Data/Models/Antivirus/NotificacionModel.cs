using AutomatizerInterop.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutomatizerInterop.Data.Models.Antivirus
{
  
    public class AntivirusNotificacionModel
    {
        public CanalesNotificacion CanalNotificacion { get; set; }
        public DateTimeOffset FechaNotificacion { get; set; }
        public string Direccion { get; set; }
        public string Persona { get; set; }
        public string Observaciones { get; set; }
        public bool Localizado { get; set; }
        public bool Confirmado { get; set; }
        public string Usuario { get; set; }
    }
}
