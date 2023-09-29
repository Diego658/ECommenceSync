using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Entities.Facturacion
{
    public class FacDetSerComp
    {
        public string EmpCod { get; set; }
        public string SucCod { get; set; }
        public string TrnCod { get; set; }
        public int TrnNum { get; set; }
        public string SerNum { get; set; }
        public byte SSec { get; set; }
        public string ItmCod { get; set; }
        public string BodCod { get; set; }
        public double ItmCnt { get; set; }
    }
}
