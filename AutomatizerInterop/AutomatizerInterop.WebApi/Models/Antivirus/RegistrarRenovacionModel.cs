using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AutomatizerInterop.WebApi.Models.Antivirus
{
    public class RegistrarRenovacionModel
    {
        [Required]
        public short? EmpleadoId { get; set; }
        [Required]
        public int? VentaId { get; set; }
        [Required]
        public int? RenovacionId { get; set; }
        [Required]
        public string Observaciones { get; set; }
    }
}
