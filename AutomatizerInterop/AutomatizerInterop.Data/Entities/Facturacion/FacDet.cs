using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AutomatizerInterop.Data.Entities.Facturacion
{
    public class FacDet
    {
        public string EmpCod { get; set; }
        public string SucCod { get; set; }
        public string TrnCod { get; set; }
        public int TrnNum { get; set; }
        public string ItmCod{ get; set; }
        public byte ISec { get; set; }
        public string ItmTip { get; set; }
        public double ItmCan { get; set; }
        public int ItmUni { get; set; }
        public double ItmPre { get; set; }
        public double ItmImp { get; set; }
        public double ItmDes { get; set; }
        public string BodCod { get; set; }
        public double CanItmPen { get; set; }
        [MaxLength(500)]
        public string DetObs { get; set; }
        public double? CostoReal { get; set; }
        public int? Orden { get; set; }
        public string CodigoBod { get; set; }
        public double ItmIce { get; set; }
        public double ItmRec { get; set; }
        public double? PesoUnitario { get; set; }
        public double? PesoTotal { get; set; }
        public DateTime? FacFec { get; set; }
        public short RecCod { get; set; }
        public byte TipUltTrn { get; set; }
        public double PctUltTrn { get; set; }
        public bool TieneIvaParaRep { get; set; }
        public short RepMarSec { get; set; }
        public short RepModSec { get; set; }
        public double CostoRef { get; set; }
        public short Vendedor { get; set; }
        public double TipoPrecio { get; set; }
        public double CantidadItmParaNC { get; set; }
        public decimal CantReferencia { get; set; }

    }
}
