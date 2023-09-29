using AutomatizerInterop.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutomatizerInterop.Data.Interfaces
{
    public interface IInteropConfiguracionProvider
    {
        Task<List<ConfiguracionProgramaFacturacionElectronica>> GetConfiguracionesAsync();
        Task<ConfiguracionProgramaFacturacionElectronica> GetConfiguracionAsync(int idConfiguracion);
        List<ConfiguracionProgramaFacturacionElectronica> GetConfiguraciones();
        ConfiguracionProgramaFacturacionElectronica GetConfiguracion(int idConfiguracion);
    }
}
