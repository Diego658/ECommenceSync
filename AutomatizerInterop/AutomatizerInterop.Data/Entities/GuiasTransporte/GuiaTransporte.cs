using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Entities.GuiasTransporte
{
    public class GuiaTransporte
    {
        public string EmpCod { get; set; }

        public int Id { get; set; }

        public int CompaniaId { get; set; }

        public string ClienteId { get; set; }

        public DateTime? Fecha { get; set; }

        public string NumeroGuia { get; set; }

        public short NumeroPiezas { get; set; }

        public string Direccion { get; set; }

        public string TrnCodFactura { get; set; }

        public int TrnNumFactura { get; set; }

        public Decimal CostoEnvio { get; set; }

        public bool EsEnvioPorCobrar { get; set; }

        public bool Cobrado { get; set; }

        public bool Entregado { get; set; }

        public bool CorreoEnviado { get; set; }

        public string DireccionCorreo { get; set; }

        public string CreadoPor { get; set; }

        public DateTime FechaCreacion { get; set; }

        public string DetalleMercaderia { get; set; }

        public bool GuiaCargadaAFactura { get; set; }

        public long IDBlob { get; set; }
    }
}
