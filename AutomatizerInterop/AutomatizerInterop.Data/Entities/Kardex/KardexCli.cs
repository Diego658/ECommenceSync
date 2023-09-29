using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AutomatizerInterop.Data.Entities.Kardex
{
    public class KardexCli
    {
        [Key]
        public int RecNum { get; set; }
        public string EmpCod { get; set; }
        public string SucCod { get; set; }
        public string TrnCod { get; set; }
        public int TrnNum { get; set; }
        public string CliSec { get; set; }
        public float TrnCnt { get; set; }
        public DateTime TrnFec { get; set; }
        public float CliSalAct { get; set; }
        public string Wrk { get; set; }
        public string TrnCat { get; set; }
        public string Obs { get; set; }
        public string NroFacCom { get; set; }
        public byte SucNum { get; set; }
        public int? PrySec { get; set; }
        public string SgrUsr { get; set; }
        public int NroRecibo { get; set; }
        public string ComprobanteCnt { get; set; }

    }
}
