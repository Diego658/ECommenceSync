using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AutomatizerInterop.Data.Entities.Facturacion
{
    public class FacRubDet
    {
        public string EmpCod { get; set; }
        public string SucCod { get; set; }
        public string TrnCod { get; set; }
        public int TrnNum { get; set; }
        public string ItmCod { get; set; }
        public byte ISec { get; set; }
        public byte RubNum { get; set; }
        [MaxLength(50)]
        public string RubDsc { get; set; }
        [MaxLength(255)]
        public string Formula { get; set; }
        public double RubValor { get; set; }
        public string RubCat { get; set; }
        public byte Codigo { get; set; }
        public double Pct { get; set; }
        public double BaseCalculo { get; set; }
        public string FormulaBaseImp { get; set; }
    }
}
