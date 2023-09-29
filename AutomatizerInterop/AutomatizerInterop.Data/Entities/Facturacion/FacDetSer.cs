using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AutomatizerInterop.Data.Entities.Facturacion
{
    public class FacDetSer
    {
        public string EmpCod { get; set; }
        public string SucCod { get; set; }
        public string TrnCod { get; set; }
        public int TrnNum { get; set; }
        public string SerNum { get; set; }
        public byte SSec { get; set; }
        [Required]
        [MaxLength(1000)]
        public string SerDsc { get; set; }
        public double SerPre { get; set; }
        public double SerImp { get; set; }
        public double SerDes { get; set; }
        public string CtaCod { get; set; }
        public double SerCan { get; set; }
        public double CanSerPen { get; set; }
        [MaxLength(100)]
        public string SerObs { get; set; }
        public int? Orden { get; set; }
        public double SerIce { get; set; }
        public double SerRec { get; set; }
        public string EgresoBod { get; set; }
        public bool CompPers { get; set; }
        public string BodCod { get; set; }
        public bool TieneIvaParaRep { get; set; }
        public short Vendedor { get; set; }
        public double CantidadItmParaNC { get; set; }
        public bool Llevar { get; set; }
        public decimal CantReferencia { get; set; }

    }
}
