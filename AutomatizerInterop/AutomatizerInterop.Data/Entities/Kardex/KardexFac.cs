using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Entities.Kardex
{
    public class KardexFac
    {
        public string EmpCod { get; set; }
        public string FacCod { get; set; }
        public int FacNum { get; set; }
        public byte NumPago { get; set; }
        public byte ModIde { get; set; }
        public string IngCod { get; set; }
        public int IngNum { get; set; }
        public DateTime IngFec { get; set; }
        public double IngVal { get; set; }
        public string IngEst { get; set; }
        public string FacEst { get; set; }
        public string DocOrigen { get; set; }
        public string FormaPago { get; set; }
    }
}
