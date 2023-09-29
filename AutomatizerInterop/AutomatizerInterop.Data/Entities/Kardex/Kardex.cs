using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AutomatizerInterop.Data.Entities.Kardex
{
    public class KarDet
    {
        [Key]
        public int RecNum { get; set; }
        public string EmpCod { get; set; }
        public string SucNro { get; set; }
        public string BodCod { get; set; }
        public string TrnCod { get; set; }
        public int TrnNum { get; set; }
        public string ItmCod { get; set; }
        public byte ISec { get; set; }
        public DateTime TrnFec { get; set; }
        public DateTime Hora { get; set; }
        public float KarCan { get; set; }
        public int UniCod { get; set; }
        public float KarCos { get; set; }
        public float KarTotLin { get; set; }
        public float CosPro { get; set; }
        public float CosUlt { get; set; }
        public float CosEst { get; set; }
        public float KarTotLinPro { get; set; }
        public float KarTotLinUlt { get; set; }
        public float KarTotLinEst { get; set; }
        public string Cat { get; set; }
        public string SecCod { get; set; }
        public short FamCod { get; set; }
        public short UbiCod { get; set; }
        public short IDf1 { get; set; }
        public short IDf2 { get; set; }
        public short IDf3 { get; set; }
        public short IDf4 { get; set; }
        public float Saldo { get; set; }
        public string ItmCodVen { get; set; }
        public string TipPer { get; set; }
        public string Persona { get; set; }
        public string Obs { get; set; }
        public byte ModIde { get; set; }
        public string CliSec { get; set; }
        public byte? SucNum { get; set; }
        public float PVNeto { get; set; }
        public float PesoUnitario { get; set; }
        public float PesoTotal { get; set; }
        public DateTime TrnFecha { get; set; }
        public string TrnCodIng { get; set; }
        public int TrnNumIng { get; set; }

    }
}
