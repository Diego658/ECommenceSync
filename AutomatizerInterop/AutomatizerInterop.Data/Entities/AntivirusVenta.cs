using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Entities
{


    public enum LeyendasTiepoCaducidad
    {
        EsteMes,
        MesPasado,

    }

    public  class AntivirusVenta
    {
        public string EmpCod { get; set; }
        public int KardexId { get; set; }
        public string TrnCod { get; set; }
        public int TrnNum { get; set; }
        public DateTime FechaFactura { get; set; }
        public int PeriodoLicencia { get; set; }
        public bool Renovada { get; set; }
        public int NumeroNotificaciones { get; set; }
        public DateTime? FechaUltimaNotificacion { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public bool OmitirNotificacion { get; set; }
        public int? UltimaNotificacionID { get; set; }
        public int? RenovacionID { get; set; }
        public bool Confirmado { get; set; }
        public bool Oculta { get; set; }

    }
}
