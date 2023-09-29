using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace AutomatizerInterop.Data.Entities.Inventario
{
    [Table("ItmMae")]
    public class Item
    {
        [MaxLength(2)]
        public string EmpCod { get; set; }
        [MaxLength(24)]
        public string ItmCod { get; set; }
        [MaxLength(1)]
        public string ItmTip { get; set; }
        public string Niv1 { get; set; }
        public string Niv2 { get; set; }
        public string Niv3 { get; set; }
        public string Niv4 { get; set; }
        public string Niv5 { get; set; }
        public string Niv6 { get; set; }
        [MaxLength(100)]
        [Required]
        public string ItmDsc { get; set; }
        public int? ItmUC { get; set; }
        public int? ItmUA { get; set; }
        public int? ItmUD { get; set; }
        public double? ItmCos { get; set; }
        public double? Utilidad { get; set; }
        public byte? ItmNiv { get; set; }
        public string Estado { get; set; }
        public string CtaCon { get; set; }
        public double? ItmCosPro { get; set; }
        public double? ItmCosUlt { get; set; }
        public double? ItmCosEst { get; set; }
        public string Obs { get; set; }
        public string CtaConVen { get; set; }
        public string CtaConCom { get; set; }
        public string CtaConCos { get; set; }
        public bool ItmProd { get; set; }
        public int? RecCod { get; set; }
        [StringLength(24)]
        public string ItmCodVen { get; set; }

        public bool TieLot { get; set; }
        public string ItmLgo { get; set; }
        public string CtaPerdInv { get; set; }
        public string CtaGanInv { get; set; }
        public string EstadoExp { get; set; }
        public bool TieIva { get; set; }
        public bool ImpImg { get; set; }
        public bool VentaCero { get; set; }
        [StringLength(24)]
        public string CtaConImp { get; set; }
        public int Secuencial { get; set; }
        public int? MarCod { get; set; }
        public string CtaConVen2 { get; set; }
        public string CtaConCom2 { get; set; }
        public string CtaConDevVen { get; set; }
        public string CtaConDevVen2 { get; set; }
        public string CtaConDevCom { get; set; }
        public string CtaConDevCom2 { get; set; }
        public byte? TieneSeries { get; set; }
        public byte? DiasMaximo { get; set; }
        public byte? DiasMinimo { get; set; }
        public bool? ExcCalMaxMin { get; set; }
        public string CtaConDescVen { get; set; }
        public string CtaConDescVen2 { get; set; }
        public string CtaConDescCom { get; set; }
        public string CtaConDescCom2 { get; set; }
        public double PVPRep { get; set; }
        public double ItmComision { get; set; }
        public short ItmDias { get; set; }
        public byte NoPueVen { get; set; }
        public byte AfeKarCom { get; set; }
        public short? Contiene { get; set; }
        public byte ProvCod { get; set; }
        public string CtaCP { get; set; }
        public bool ManGar { get; set; }
        public bool Web { get; set; }
        public bool Promo { get; set; }
        public bool AprLisPrec { get; set; }
        public bool ExcLisPrec { get; set; }
        public string Mascara { get; set; }
        public bool TieIce { get; set; }
        public bool ExcProCostos { get; set; }
        public byte? LinCod { get; set; }
        public string CtaGND { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public double TarProd { get; set; }
        public string CtaTransitoria { get; set; }
        public short IdColor { get; set; }
        public double PorcentajeAranceles { get; set; }
        public short ArcCod { get; set; }
        public double PctDescuento { get; set; }
        public string CodigoGenerico { get; set; }
        public string CodigoComercial { get; set; }
        public string BodTraspDefecto { get; set; }
        public bool PermitePerdidas { get; set; }
        public string DireccionWeb { get; set; }
        public short IdTalla { get; set; }
        public byte Condicion { get; set; }
        public bool PrecioRegulado { get; set; }
        public string CtaCodCC { get; set; }
        public string CtaCodPP { get; set; }
        public double PrecioMaximoCompra { get; set; }
        public string Modelo { get; set; }
        public string NroParte { get; set; }
        public string CodigosProveedor { get; set; }
        public double PctDescuentoMaxTC { get; set; }
        public byte Original { get; set; }
        public decimal PctMinUtil { get; set; }
        public bool PermitePedidoNegativo { get; set; }

        [ForeignKey("ItemId")]
        public ICollection<ImagenItem> Imagenes { get; set; }

    }
}
