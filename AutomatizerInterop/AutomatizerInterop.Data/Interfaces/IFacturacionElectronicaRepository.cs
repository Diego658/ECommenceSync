using AutomatizerInterop.Data.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AutomatizerInterop.Data.Interfaces
{
    public interface IFacturacionElectronicaRepository
    {
        public Task<IEnumerable<ConfiguracionProgramaFacturacionElectronica>> GetConfiguracionesAsync();
        public Task<IEnumerable<FacturacionElectronicaFactura>> GetFacturasAsync(int idConfiguracion, DateTime fechaInicio, DateTime fechaFin);
        public Task<IEnumerable<FacturacionElectronicaNotaCredito>> GetNotasCreditoAsync(int idConfiguracion, DateTime fechaInicio, DateTime fechaFin);
        public Task<ConfiguracionProgramaFacturacionElectronica> GetConfiguracionAsync(int idConfiguracion);
        public Task<IEnumerable<FacturacionElectronicaRetencion>> GetRetencionesAsync(int idConfiguracion, DateTime fechaInicio, DateTime fechaFin);
        Task<Stream> GetPdfAsync(int idDocumento);
        Task<Stream> GetPdfAsync(byte tipoDocumento, string codigo, int numero);
        Task<Stream> GetXmlAsync(int idDocumento);
        Task<List<RegistroOperacionDocumento>> GetLogDocumentoAsync(int idDocumento);
        Task<FacturacionElectronicaRetencion> IniciarProcesoRetencionAsync(int idConfiguracion, FacturacionElectronicaRetencion retencion);
        Task<int> GetNumeroPendientesAsync(int idConfiguracion, byte tipoDocumento);
        Task<Tuple<bool, string>> ReintentarDocumentoAsync(int idDocumento);
        Task<Tuple<bool, string>> ReenviarEmailDocumento(int idDocumento, string emails);
    }
}
