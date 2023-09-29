using AutomatizerInterop.Data.Entities;
using AutomatizerInterop.Data.Entities.Antivirus;
using AutomatizerInterop.Data.Models.Antivirus;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutomatizerInterop.Data.Interfaces
{
    public interface IAntivirusRepository
    {
        Task<List<Antivirus>> GetAntivirusesAsync(int idConfiguracion);
        Task<List<AntivirusVenta>> GetVentasPorRenovar(int idConfiguracion, bool mostrarVencimientosFuturos);
        Task<List<AntivirusVentaFullInfo>> GetVentasPorRenovarFullInfo(int idConfiguracion, byte tipo, bool mostrarVencimientosFuturos);
        Task<List<ResumenEstadoLicenciasAntivirus>> GetResumenEstadoLicenciasAntivirus(int idConfiguracion);
        List<ConfiguracionProgramaFacturacionElectronica> GetConfiguraciones();
        ConfiguracionProgramaFacturacionElectronica GetConfiguracion(int configuracionId);
        Task<List<SaldoLicenciaVsLicenciasPorRenovar>> GetSaldoLicenciaVsLicenciasPorRenovarsAsync(int configuracionId);
        Task<List<AntivirusNotificacionesPlantilla>> GetPlantillasNotificacion(int configuracionId);
        Task<AntivirusNotificacionesPlantilla> GetPlantillaNotificacion(int configuracionId, AntivirusNotificacionesPlantilla plantilla);
        Task<AntivirusNotificacionesPlantilla> AddPlantillaNotificacion(int configuracionId, string nombre);
        Task<bool> UpdatePlantillaNotificacion(int configuracionId, AntivirusNotificacionesPlantilla antivirusNotificacionesPlantilla);
        Task<AntivirusHistorialNotificacionFullData> GetInformacionNotificacion(int notificacionId, bool fullData);
        Task<AntivirusVenta> GetInformacionVentaAntivirus(int ventaId);
        Task<AntivirusVentaFullInfo> GetFullInformacionVentaAntivirus(int ventaId);
        Task RegistrarNotificacion(int ventaId, AntivirusNotificacionModel notificacion);
        IRepository GetGenericRepository();
        Task<dynamic> RegistrarRenovacion(int ventaId, int empleadoId, int renovacionId);
        Task OcultarLicencia(int ventaId, int empleadoId, string motivo);
    }
}