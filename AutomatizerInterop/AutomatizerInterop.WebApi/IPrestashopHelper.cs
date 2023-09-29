using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutomatizerInterop.WebApi
{
    public interface IPrestashopHelper
    {
        string ApiUrl { get; }
        string ApiKey { get; }
        long EstadoPagoAceptado { get; }
        long EstadoEnPreparacion { get; }
        long EstadoEnviado { get; }

        long EstadoEntregado { get; }

        long EstadoCancelado { get; }

        string BodCodOrdenesPrestashop { get; }

        string ServicioTransporte { get; }

        string CodigoEgresoTemporal { get; }
    }
}
