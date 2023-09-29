using AutomatizerInterop.Data.EntityFramewrok;
using AutomatizerInterop.Data.Entities;
using AutomatizerInterop.Data.Entities.Antivirus;
using AutomatizerInterop.Data.Helper;
using AutomatizerInterop.Data.Interfaces;
using AutomatizerInterop.Data.Models.Antivirus;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatizerInterop.Data.Repositories
{
    public class AntivirusRepository : AutoConfigRepository, IAntivirusRepository
    {
        private const string SqlAllAv = @"SELECT itm.ItmCodVen, itm.ItmCod, itm.Secuencial, itm.ItmDsc 
                FROM Antivirus  av
                inner join ItmMae itm on  av.IdItem = itm.Secuencial
                where itm.EmpCod = @0";

        private const string SqlFullInfoVentaAntivirus = @"SELECT av.*, TransaccionVenta = trn.TrnDes, Cliente = cli.NombreCompleto, EmailCliente = suc.CliEma, Producto = itm.ItmDsc, CodigoVenta = itm.ItmCodVen, 
	ObservacionesNotificacion = avn.Observaciones, Cantidad = CAST(k.KarCan as int) , IdentificacionCliente = cli.Clicedruc, FechaFactura = fac.FacFec, 
	CodigoCliente = cli.clisec, dbo.StoreSync_GetSeriesUsadasMovimientoKardex(k.RecNum) As Series
FROM dbo.AntivirusVentas av
                INNER JOIN Trn ON Trn.EmpCod = av.EmpCod AND trn.ModIde = 8 AND Trn.TrnCat = 'FA' AND Trn.TrnCod = av.TrnCod
                INNER JOIN KarDet k ON k.RecNum = av.KardexId
                INNER JOIN ItmMae itm ON itm.EmpCod = k.EmpCod AND itm.ItmCod = k.ItmCod
                INNER JOIN FacCab fac on fac.EmpCod = k.EmpCod AND fac.TrnCod = k.TrnCod AND fac.TrnNum = k.TrnNum
                INNER JOIN climae cli ON cli.EmpCod = fac.EmpCod AND cli.Clisec = fac.CliCod
                INNER JOIN CliSuc suc ON suc.EmpCod = fac.EmpCod AND suc.CliSec = fac.CliCod AND suc.SucNum = fac.SucNum
                LEFT JOIN AntivirusHistorialNotificaciones avn ON avn.Id = av.UltimaNotificacionID
                WHERE av.EmpCod = @0 ";



        private readonly IInteropConfiguracionProvider configuracionProvider;

        public AntivirusRepository(IInteropConfiguracionProvider configuracionProvider, IRepository genericRepository, AutomatizerInteropDbContext dbContext) 
            : base(configuracionProvider, genericRepository, dbContext)
        {
            this.configuracionProvider = configuracionProvider;
        }

        public async Task<List<Antivirus>> GetAntivirusesAsync(int idConfiguracion)
        {
            ConfiguracionProgramaFacturacionElectronica configuracion = await configuracionProvider.GetConfiguracionAsync(idConfiguracion);
            using (var database = DatabaseHelper.GetSqlDatabase(configuracion))
            {

                var result = new List<Antivirus>();
                using (var reader = await database.QueryAsync<Antivirus>(SqlAllAv, configuracion.CodigoEmpresa))
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(reader.Poco);
                    }

                }
                return result;
            }
        }



        public async Task<List<AntivirusVenta>> GetVentasPorRenovar(int idConfiguracion, bool mostrarVencimientosFuturos)
        {
            ConfiguracionProgramaFacturacionElectronica configuracion = await configuracionProvider.GetConfiguracionAsync(idConfiguracion);
            using (var database = DatabaseHelper.GetSqlDatabase(configuracion))
            {
                var result = new List<AntivirusVenta>();
                using (var reader = await database.QueryAsync<AntivirusVenta>(@"SELECT *
                FROM dbo.AntivirusVentas av
                WHERE av.EmpCod = @0 AND Renovada = 0  " + (mostrarVencimientosFuturos ? "" : " AND FechaVencimiento < DATEADD(DAY,15, GETDATE())"), configuracion.CodigoEmpresa))
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(reader.Poco);
                    }

                }
                return result;
            }
        }

        public async Task<List<AntivirusVentaFullInfo>> GetVentasPorRenovarFullInfo(int idConfiguracion, byte tipo, bool mostrarVencimientosFuturos)
        {
            ConfiguracionProgramaFacturacionElectronica configuracion = await configuracionProvider.GetConfiguracionAsync(idConfiguracion);
            using (var database = DatabaseHelper.GetSqlDatabase(configuracion))
            {
                var result = new List<AntivirusVentaFullInfo>();
                var filtroTipo = "";
                if (tipo == 0) filtroTipo = " AND (av.UltimaNotificacionID IS NULL OR av.Confirmado = 0) AND (av.Oculta = 0) "; //Pendientes
                if (tipo == 1) filtroTipo = " AND av.Confirmado = 1 AND (av.Oculta = 0) "; //Confirmados
                if (tipo == 2) filtroTipo = " AND avn.Localizado = 0 AND (av.Oculta = 0) "; //NoLocalizados

                using (var reader = await database.QueryAsync<AntivirusVentaFullInfo>(SqlFullInfoVentaAntivirus + " AND av.Renovada = 0   " + filtroTipo + (mostrarVencimientosFuturos ? "" : " AND av.FechaVencimiento < DATEADD(DAY,15, GETDATE())") + " ORDER BY av.FechaVencimiento", configuracion.CodigoEmpresa))
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(reader.Poco);
                    }

                }
                return result;
            }
        }


        public async Task<List<ResumenEstadoLicenciasAntivirus>> GetResumenEstadoLicenciasAntivirus(int idConfiguracion)
        {
            try
            {
                var configuracion = await configuracionProvider.GetConfiguracionAsync(idConfiguracion);
                var result = new List<ResumenEstadoLicenciasAntivirus>(8);
                using (var database = DatabaseHelper.GetSqlDatabase(configuracion))
                {
                    using (var reader = await database.QueryAsync<ResumenEstadoLicenciasAntivirus>(@"select Grupo = dbo.Antivirus_GetGruposCaducidadVenta(FechaVencimiento), Cantidad = COUNT(KardexId) 
                from AntivirusVentas
                where EmpCod = @0
                group by dbo.Antivirus_GetGruposCaducidadVenta(FechaVencimiento)", configuracion.CodigoEmpresa))
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(reader.Poco);
                        }
                    }
                }
                return result;
            }
            catch (Exception)
            {
                return new List<ResumenEstadoLicenciasAntivirus>();
            }

        }


        public async Task<List<SaldoLicenciaVsLicenciasPorRenovar>> GetSaldoLicenciaVsLicenciasPorRenovarsAsync(int configuracionId)
        {
            var configuracion = await configuracionProvider.GetConfiguracionAsync(configuracionId);
            var result = new List<SaldoLicenciaVsLicenciasPorRenovar>(8);
            using (var database = DatabaseHelper.GetSqlDatabase(configuracion))
            {
                using (var reader = await database.QueryProcAsync<SaldoLicenciaVsLicenciasPorRenovar>("SP_GetSaldoLicenciaVsLicenciasPorRenovar",
                    new { EmpCod = configuracion.CodigoEmpresa, BodCod = "B1" }))
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(reader.Poco);
                    }
                }
            }
            return result;
        }


        public List<ConfiguracionProgramaFacturacionElectronica> GetConfiguraciones()
        {
            return configuracionProvider.GetConfiguraciones();
        }

        public ConfiguracionProgramaFacturacionElectronica GetConfiguracion(int configuracionId)
        {
            return configuracionProvider.GetConfiguracion(configuracionId);
        }

        public async Task<List<AntivirusNotificacionesPlantilla>> GetPlantillasNotificacion(int configuracionId)
        {
            var configuracion = await configuracionProvider.GetConfiguracionAsync(configuracionId);
            var result = new List<AntivirusNotificacionesPlantilla>(8);
            using (var database = DatabaseHelper.GetSqlDatabase(configuracion))
            {
                using (var reader = await database.QueryAsync<AntivirusNotificacionesPlantilla>(@"SELECT        EmpCod, PlantillaID, NombrePlantilla, FechaCreacion, UsuarioCrea, FechaModificacion, UsuarioModifica, Contenido, ContenidoHtml
                FROM AntivirusNotificacionesPlantillas
                WHERE (EmpCod = @0)",
                    configuracion.CodigoEmpresa))
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(reader.Poco);
                    }
                }
            }
            return result;
        }

        public Task<AntivirusNotificacionesPlantilla> GetPlantillaNotificacion(int configuracionId, AntivirusNotificacionesPlantilla plantilla)
        {
            throw new NotImplementedException();
        }

        public Task<AntivirusNotificacionesPlantilla> AddPlantillaNotificacion(int configuracionId, string nombre)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdatePlantillaNotificacion(int configuracionId, AntivirusNotificacionesPlantilla antivirusNotificacionesPlantilla)
        {
            var configuracion = await configuracionProvider.GetConfiguracionAsync(configuracionId);
            using (var database = DatabaseHelper.GetSqlDatabase(configuracion))
            {
                var resultado = await database.ExecuteAsync("UPDATE AntivirusNotificacionesPlantillas SET Contenido = @2, ContenidoHtml = @3, FechaModificacion = GETDATE() WHERE EmpCod = @0 AND PlantillaID = @1",
                    configuracion.CodigoEmpresa, antivirusNotificacionesPlantilla.PlantillaID, antivirusNotificacionesPlantilla.Contenido, antivirusNotificacionesPlantilla.ContenidoHtml);
                return resultado == 1;
            }
        }

        public Task<AntivirusHistorialNotificacionFullData> GetInformacionNotificacion(int notificacionId, bool fullData)
        {
            if (fullData)
            {
                return FirstAsync<AntivirusHistorialNotificacionFullData>(@"SELECT  Id, KardexId, FechaNotificacion, CanalNotificacion, Automatica, EmpleadoNotifica, DireccionNotificacion, PersonaNotificada, Observaciones, Localizado, Confirmado, NombreEmpleado= EplNom + ' ' + EplApl
                FROM AntivirusHistorialNotificaciones
                INNER JOIN EplMae ON EplMae.EmpCod = @0 AND EplMae.EplSec = AntivirusHistorialNotificaciones.EmpleadoNotifica WHERE AntivirusHistorialNotificaciones.Id = @1", notificacionId);
            }
            else
            {
                return FirstAsync<AntivirusHistorialNotificacionFullData>(@"SELECT  Id, KardexId, FechaNotificacion, CanalNotificacion, Automatica, EmpleadoNotifica, DireccionNotificacion, PersonaNotificada, Observaciones, Localizado, Confirmado, NombreEmpleado= EplNom + ' ' + EplApl
                FROM AntivirusHistorialNotificaciones WHERE AntivirusHistorialNotificaciones.Id = @1", notificacionId);
            }
        }

        public async Task<AntivirusVenta> GetInformacionVentaAntivirus(int ventaId)
        {
            return await FirstAsync<AntivirusVenta>("SELECT * FROM AntivirusVentas WHERE KardexId = @1", ventaId); ;
        }

        public async Task<AntivirusVentaFullInfo> GetFullInformacionVentaAntivirus(int ventaId)
        {
            return await FirstAsync<AntivirusVentaFullInfo>(SqlFullInfoVentaAntivirus + " AND av.KardexId = @1", ventaId);
        }

        public async Task RegistrarNotificacion(int ventaId, AntivirusNotificacionModel notificacion)
        {
            await ExecuteAsync(@"INSERT INTO AntivirusHistorialNotificaciones
            (KardexId, FechaNotificacion, CanalNotificacion, Automatica, EmpleadoNotifica, DireccionNotificacion, PersonaNotificada, Observaciones, Localizado, Confirmado)
            VALUES
            (@1, @2, @3, @4, dbo.StoreSync_GetEmpleadoIdFromUser(@0, @5), @6, @7, @8, @9, @10)",
            ventaId, notificacion.FechaNotificacion, notificacion.CanalNotificacion, false, notificacion.Usuario, notificacion.Direccion, notificacion.Persona, notificacion.Observaciones, notificacion.Localizado, notificacion.Confirmado);
        }

        public IRepository GetGenericRepository()
        {
            return GenericRepository;
        }

        public async Task<dynamic> RegistrarRenovacion(int ventaId, int empleadoId, int renovacionId)
        {
            using (var db = GetSQLDatabase())
            {
                try
                {
                    await db.BeginTransactionAsync();
                    await db.ExecuteAsync("UPDATE AntivirusVentas SET Renovada = 1, RenovacionID = @1 WHERE KardexId = @0", ventaId, renovacionId);
                    await db.ExecuteAsync(@"INSERT INTO AntivirusVentasRenovaciones
                         (VentaId, EmpleadoId, RenovacionId)
                    VALUES (@0, @1, @2)", ventaId, empleadoId, renovacionId);
                    var renovacion = await db.FirstAsync<dynamic>("SELECT * FROM AntivirusVentasRenovaciones WHERE VentaId = @0", ventaId);
                    db.CompleteTransaction();
                    return renovacion;
                }
                catch (Exception)
                {
                    db.AbortTransaction();
                    throw;
                }
            }

        }

        public async Task OcultarLicencia(int ventaId, int empleadoId, string motivo)
        {
            await ExecuteAsync(@"INSERT INTO AntivirusHistorialNotificaciones
            (KardexId, FechaNotificacion, CanalNotificacion, Automatica, EmpleadoNotifica, DireccionNotificacion, PersonaNotificada, Observaciones, Localizado, Confirmado, Ocultar)
            VALUES
            (@1, @2, @3, @4,  @5, @6, @7, @8, @9, @10, @11)",
            ventaId, DateTime.Now, 1, false, empleadoId, "", "", motivo, true, false, true);
        }
    }
}
