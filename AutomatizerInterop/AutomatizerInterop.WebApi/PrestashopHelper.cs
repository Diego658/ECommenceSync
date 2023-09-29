using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutomatizerInterop.WebApi
{
    public class PrestashopHelper : IPrestashopHelper
    {
        public string ApiUrl  {get;}

        public string ApiKey { get; }

        public long EstadoPagoAceptado { get; }

        public long EstadoEnPreparacion { get; }

        public long EstadoEnviado { get; }

        public long EstadoEntregado { get; }

        public long EstadoCancelado { get; }

        public string BodCodOrdenesPrestashop { get; }

        public string ServicioTransporte { get; }

        public string CodigoEgresoTemporal { get; }

        public PrestashopHelper(IConfiguration configuration)
        {
            var prestashopSetion = configuration.GetSection("Prestashop");
            if (!prestashopSetion.Exists())
            {
                throw new InvalidOperationException("No se encuentra la configuracion de prestashop");
            }
            ApiUrl = prestashopSetion["ApiUrl"];
            ApiKey = prestashopSetion["ApiSecret"];
            EstadoPagoAceptado = long.Parse( prestashopSetion[nameof(EstadoPagoAceptado)]);
            EstadoEnPreparacion = long.Parse(prestashopSetion[nameof(EstadoEnPreparacion)]);
            this.EstadoEnviado = long.Parse(prestashopSetion[nameof(EstadoEnviado)]);
            this.EstadoEntregado = long.Parse(prestashopSetion[nameof(EstadoEntregado)]);
            this.EstadoCancelado = long.Parse(prestashopSetion[nameof(EstadoCancelado)]);
            this.BodCodOrdenesPrestashop = prestashopSetion[nameof(BodCodOrdenesPrestashop)];
            this.ServicioTransporte = prestashopSetion[nameof(ServicioTransporte)];
            this.CodigoEgresoTemporal = prestashopSetion[nameof(CodigoEgresoTemporal)];
        }

    }
}
