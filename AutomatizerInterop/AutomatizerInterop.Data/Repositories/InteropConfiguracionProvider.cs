using AutomatizerInterop.Data.Entities;
using AutomatizerInterop.Data.Interfaces;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatizerInterop.Data.Repositories
{
    public sealed class InteropConfiguracionProvider : IInteropConfiguracionProvider
    {
        private readonly IDatabase database;

        private static List<ConfiguracionProgramaFacturacionElectronica> configuraciones;

        public InteropConfiguracionProvider(IDatabase database)
        {
            this.database = database;
        }

        public async Task<ConfiguracionProgramaFacturacionElectronica> GetConfiguracionAsync(int idConfiguracion)
        {
            if (configuraciones ==null)
            {
                //await Task.Delay(250);
                if (configuraciones == null)
                {
                    await GetConfiguracionesAsync();
                }
            }
            return configuraciones.First(c => c.idConfiguracionPrograma == idConfiguracion);
            //return await _mysqlDatabase.FirstOrDefaultAsync<ConfiguracionProgramaFacturacionElectronica>
            //    ("SELECT * FROM sistema.configuracionprograma where idConfiguracionPrograma = @0", idConfiguracion);
        }

        public async Task< List<ConfiguracionProgramaFacturacionElectronica>> GetConfiguracionesAsync()
        {
            
            if (configuraciones == null)
            {
                System.Diagnostics.Debug.WriteLine($"GetConfiguracionesAsync");
                lock (this)
                {
                    Task.Delay(500).Wait();
                }
                if (configuraciones == null)
                {
                    var tmp = new List<ConfiguracionProgramaFacturacionElectronica>();
                    using (var reader = await database.QueryAsync<ConfiguracionProgramaFacturacionElectronica>("SELECT * FROM sistema.configuracionprograma"))
                    {
                        while (await reader.ReadAsync())
                        {
                            tmp.Add(reader.Poco);
                        }
                    }
                    configuraciones = tmp;
                }
            }
            return configuraciones;
        }

        public List<ConfiguracionProgramaFacturacionElectronica> GetConfiguraciones()
        {
            if (configuraciones == null)
            {
                if (configuraciones == null)
                {
                    GetConfiguracionesAsync().Wait();
                }
            }
            return configuraciones;
        }

        public ConfiguracionProgramaFacturacionElectronica GetConfiguracion(int idConfiguracion)
        {
            if (configuraciones == null)
            {
                //await Task.Delay(250);
                if (configuraciones == null)
                {
                    GetConfiguracionesAsync().Wait();
                }
            }
            return configuraciones.First(c => c.idConfiguracionPrograma == idConfiguracion);
        }
    }
}
