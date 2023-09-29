using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AutomatizerInterop.Data.Entities.Facturacion
{
    public class FacCab
    {
        public string EmpCod { get; set; }
        public string SucCod { get; set; }
        public string TrnCod { get; set; }
        public int TrnNum { get; set; }
        public string CliCod { get; set; }
        public string EstCod { get; set; }
        public DateTime? FacFec { get; set; }
        public double? FacSub { get; set; }
        public double? FacImp { get; set; }
        public double? FacDes { get; set; }
        public double? FacRec { get; set; }
        public double? FacTot { get; set; }
        public string Tipo { get; set; }
        public DateTime? FacEnt { get; set; }
        [MaxLength(800)]
        public string FacObs { get; set; }
        public byte? SucNum { get; set; }
        public double? FacCon { get; set; }
        public double? FacCre { get; set; }
        public int? EplCod { get; set; }
        public string Wrk { get; set; }
        public string GuiRem { get; set; }
        public string BodCod { get; set; }
        public string CajCod { get; set; }
        [MaxLength(500)]
        public string ObsExt { get; set; }
        public string TarCod { get; set; }
        public string TarNumBou { get; set; }
        public string TarNumCuo { get; set; }
        public string TarEst { get; set; }
        public double? TarVal { get; set; }
        public string TarObs { get; set; }
        public string TrnNumTar { get; set; }
        public bool? Dev { get; set; }
        public int? TrnNumChe { get; set; }
        public double? TotCheques { get; set; }
        public double? CliPorDesc { get; set; }
        public int? NumPagos { get; set; }
        public int? NumDias { get; set; }
        public int? EmpCodLab { get; set; }
        public double? BieTarIva { get; set; }
        public double? BieTarCero { get; set; }
        public double? SrvTarIva { get; set; }
        public double? SrvTarCero { get; set; }
        public double? ActTarIva { get; set; }
        public double? ActTarCero { get; set; }
        public double? GastosTarIva { get; set; }
        public double? GastosTarCero { get; set; }
        public byte? DevCod { get; set; }
        public string SerieFact { get; set; }
        public string AutFact { get; set; }
        public double? TotIce { get; set; }
        public int? PrySec { get; set; }
        public double? FacAnt { get; set; }
        public byte? NumImp { get; set; }
        public DateTime? Vence { get; set; }
        public string SgrUsrV { get; set; }
        public string CliEvent { get; set; }
        public bool FacturaConvenio { get; set; }
        public short EplCodNomina { get; set; }
        public short PtosFactura { get; set; }
        public short PtosCanjeados { get; set; }
        public short PtosSaldo { get; set; }
        public double Recibido { get; set; }
        public double Cambio { get; set; }
        public string DetallePago { get; set; }
        public byte PlanPag { get; set; }
        public byte ListaPrecios { get; set; }
        public DateTime? FecIniAut { get; set; }

        public double ValorDescCabManual { get; set; }
        public string CtaAutoconsumo { get; set; }
        public string Correlativo { get; set; }
        public string Verificador { get; set; }
        public string NroDocTransp { get; set; }
        public string distAduanero { get; set; }
        public string ClaveAcceso { get; set; }
        public string TrnCodFactND { get; set; }
        public int TrnNumFactND { get; set; }
        public decimal MontoInteresAplicadoConND { get; set; }
        public double BaseNetaGravada { get; set; }
        public double BaseNetaCero { get; set; }
        public bool EsExportacion { get; set; }
        public DateTime? FecIniTraslado { get; set; }
        public DateTime? FecFinTraslado { get; set; }
        public string DocAduaneroUnico { get; set; }
        public byte CodMotivoTraslado { get; set; }
        public byte CodPuntoPartida { get; set; }
        public string CliSecTransportista { get; set; }
        public byte SecPlaca { get; set; }
        public decimal PorcentajeComision { get; set; }
        public string TrnCodSC { get; set; }
        public int TrnNumSC { get; set; }
        public string CodigoIESS { get; set; }
        public int SecSoan { get; set; }
        public decimal FPC_Efectivo { get; set; }
        public decimal FPC_Cheques { get; set; }
        public decimal FPC_Tarjeta { get; set; }
        public decimal FPC_Transferencia { get; set; }
        public int ZonCod { get; set; }
        public short CliTipHistorico { get; set; }
        public short CliPaiGR { get; set; }
        public short CliPrvGR { get; set; }
        public short CliCiuGR { get; set; }
        public string UsuarioAutorizaDescuento { get; set; }

    }
}
