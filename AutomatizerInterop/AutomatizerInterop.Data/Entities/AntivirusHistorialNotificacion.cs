using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Entities
{

    public enum CanalesNotificacion
    {
        Telefono=1,
        Celular=2,
        Email=3,

    }

    public class AntivirusHistorialNotificacion
    {
        public int Id { get; set; }
        public int KardexId { get; set; }
        public DateTime FechaNotificacion { get; set; }
        public CanalesNotificacion CanalNotificacion { get; set; }
        public bool Automatica { get; set; }
        public int EmpleadoNotifica{ get; set; }
        public string DireccionNotificacion{ get; set; }
        public string PersonaNotificada { get; set; }
        public string Observaciones { get; set; }
        public bool Localizado { get; set; }
        public bool Confirmado { get; set; }


    }



    public class AntivirusHistorialNotificacionFullData:AntivirusHistorialNotificacion
    {
        public string NombreEmpleado { get; set; }

    }


}
