using AutomatizerInterop.Data.Entities;
using AutomatizerInterop.Data.Helper;
using AutomatizerInterop.Data.Interfaces;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatizerInterop.Data.Repositories
{
    public class FacturacionElectronicaRepository : IFacturacionElectronicaRepository
    {
        private readonly IDatabase _mysqlDatabase;
        private readonly IInteropConfiguracionProvider configuracionProvider;

        //private readonly IDatabase _sqlDatabase;

        public FacturacionElectronicaRepository(IInteropConfiguracionProvider configuracionProvider, IDatabase mysqlDatabase)
        {
            this.configuracionProvider = configuracionProvider;
            _mysqlDatabase = mysqlDatabase;
            //_sqlDatabase = sqlDataabse;
        }

        public async Task<IEnumerable<ConfiguracionProgramaFacturacionElectronica>> GetConfiguracionesAsync()
        {
            return await configuracionProvider.GetConfiguracionesAsync();
            //await Task.Delay(250);
            //var configuraciones = new List<ConfiguracionProgramaFacturacionElectronica>();
            //var reader = await _mysqlDatabase.QueryAsync<ConfiguracionProgramaFacturacionElectronica>("SELECT * FROM sistema.configuracionprograma");
            //while (await reader.ReadAsync())
            //{
            //    configuraciones.Add(reader.Poco);
            //}
            //reader.Dispose();
            //return configuraciones;
        }

        public async Task<ConfiguracionProgramaFacturacionElectronica> GetConfiguracionAsync(int idConfiguracion)
        {
            return await configuracionProvider.GetConfiguracionAsync(idConfiguracion);
            //return await _mysqlDatabase.FirstOrDefaultAsync<ConfiguracionProgramaFacturacionElectronica>
            //    ("SELECT * FROM sistema.configuracionprograma where idConfiguracionPrograma = @0", idConfiguracion);
        }


        public async Task<IEnumerable<FacturacionElectronicaFactura>> GetFacturasAsync(int idConfiguracion, DateTime fechaInicio, DateTime fechaFin)
        {
            var configuracion = await GetConfiguracionAsync(idConfiguracion);
            var sqlDatabase = DatabaseHelper.GetSqlDatabase(configuracion);
            var infoAdicionalFacturas = new List<dynamic>();
            var reader = await sqlDatabase.QueryAsync<dynamic>(@"SELECT   CASE FacCab.EstCod WHEN 'R' THEN CAST(1 as bit) ELSE CAST(0 as bit) END As Estado,    FAC_ELE_Documentos.ID, climae.CliApl + ' ' + climae.CliNom AS Cliente, Trn.TrnDes AS Transaccion, FAC_ELE_Documentos.IDDocumento, '' as NumeroFacturaAfectada 
                FROM FAC_ELE_Documentos INNER JOIN
                         FAC_ELE_Transacciones ON FAC_ELE_Documentos.EmpCod = FAC_ELE_Transacciones.EmpCod AND FAC_ELE_Documentos.ModIde = FAC_ELE_Transacciones.ModIde AND 
                         FAC_ELE_Documentos.TrnCod = FAC_ELE_Transacciones.TrnCod INNER JOIN
                         FacCab ON FAC_ELE_Documentos.EmpCod = FacCab.EmpCod AND FAC_ELE_Documentos.TrnCod = FacCab.TrnCod AND FAC_ELE_Documentos.TrnNum = FacCab.TrnNum INNER JOIN
                         climae ON FacCab.EmpCod = climae.EmpCod AND FacCab.CliCod = climae.Clisec INNER JOIN
                         Trn ON FAC_ELE_Documentos.EmpCod = Trn.EmpCod AND FAC_ELE_Documentos.ModIde = Trn.ModIde AND FAC_ELE_Documentos.TrnCod = Trn.TrnCod
                WHERE (FAC_ELE_Transacciones.EmpCod = @0) AND (FAC_ELE_Documentos.ModIde = 8) AND(Trn.Dev = 0) AND FechaEmision >= @1 AND FechaEmision <= @2", configuracion.CodigoEmpresa, fechaInicio.Date, fechaFin.Date.AddHours(23).AddMinutes(59).AddSeconds(59));

            while (await reader.ReadAsync())
            {
                infoAdicionalFacturas.Add(reader.Poco);
            }

            reader.Dispose();


            var facturas = _mysqlDatabase.Query<FacturacionElectronicaFactura>(@"SELECT ID As IDDocumento, SerieDocumento, FechaEmision As Fecha, '' As Transaccion, TrnNum As Numero, '' As Cliente, EstadoSri, RideGenerado, XmlGenerado
            FROM sistema.fedocumentos where ConfiguracionId = @0 AND TipoDocumentoSri = 1 AND FechaEmision >= @1 AND FechaEmision <= @2;", configuracion.Id, fechaInicio, fechaFin).ToList();


            foreach (var infoAdicional in infoAdicionalFacturas)
            {
                var factura = facturas.FirstOrDefault(f => f.IDDocumento == infoAdicional.IDDocumento);
                if (factura != null)
                {
                    factura.EstadoSistema = infoAdicional.Estado;
                    factura.Cliente = infoAdicional.Cliente;
                    factura.Transaccion = infoAdicional.Transaccion;
                }
            }

            return facturas;


        }

        public async Task<IEnumerable<FacturacionElectronicaNotaCredito>> GetNotasCreditoAsync(int idConfiguracion, DateTime fechaInicio, DateTime fechaFin)
        {
            var configuracion = await GetConfiguracionAsync(idConfiguracion);
            var sqlDatabase = DatabaseHelper.GetSqlDatabase(configuracion);
            var infoAdicionalNorasCredito = new List<dynamic>();
            var reader = await sqlDatabase.QueryAsync<dynamic>(@"SELECT   CASE FacCab.EstCod WHEN 'R' THEN CAST(1 as bit) ELSE CAST(0 as bit) END As Estado,     FAC_ELE_Documentos.ID, climae.CliApl + ' ' + climae.CliNom AS Cliente, Trn.TrnDes AS Transaccion, FAC_ELE_Documentos.IDDocumento, FacFue.FCod + ' ' + CAST(FacFue.FNum AS varchar(10)) 
                         AS NumeroFacturaAfectada
FROM            FAC_ELE_Documentos INNER JOIN
                         FAC_ELE_Transacciones ON FAC_ELE_Documentos.EmpCod = FAC_ELE_Transacciones.EmpCod AND FAC_ELE_Documentos.ModIde = FAC_ELE_Transacciones.ModIde AND 
                         FAC_ELE_Documentos.TrnCod = FAC_ELE_Transacciones.TrnCod INNER JOIN
                         FacCab ON FAC_ELE_Documentos.EmpCod = FacCab.EmpCod AND FAC_ELE_Documentos.TrnCod = FacCab.TrnCod AND FAC_ELE_Documentos.TrnNum = FacCab.TrnNum INNER JOIN
                         climae ON FacCab.EmpCod = climae.EmpCod AND FacCab.CliCod = climae.Clisec INNER JOIN
                         Trn ON FAC_ELE_Documentos.EmpCod = Trn.EmpCod AND FAC_ELE_Documentos.ModIde = Trn.ModIde AND FAC_ELE_Documentos.TrnCod = Trn.TrnCod INNER JOIN
                         FacFue ON FacCab.EmpCod = FacFue.EmpCod AND FacCab.SucCod = FacFue.SucCod AND FacCab.TrnCod = FacFue.TrnCod AND FacCab.TrnNum = FacFue.TrnNum
WHERE     (FAC_ELE_Transacciones.EmpCod = @0) AND   (FAC_ELE_Documentos.ModIde = 8) AND (Trn.Dev = 1) AND (FAC_ELE_Documentos.FechaEmision >= @1) AND (FAC_ELE_Documentos.FechaEmision <= @2)", configuracion.CodigoEmpresa, fechaInicio.Date, fechaFin.Date.AddHours(23).AddMinutes(59).AddSeconds(59));


            while (await reader.ReadAsync())
            {
                infoAdicionalNorasCredito.Add(reader.Poco);
            }

            reader.Dispose();

            var notasCredito = _mysqlDatabase.Query<FacturacionElectronicaNotaCredito>(@"SELECT ID As IDDocumento, FechaEmision As Fecha, '' As Transaccion, TrnNum As Numero, '' As Cliente, EstadoSri, RideGenerado, XmlGenerado, '' as NumeroFacturaAfectada 
            FROM sistema.fedocumentos where ConfiguracionId = @0 AND TipoDocumentoSri = 4 AND FechaEmision >= @1 AND FechaEmision <= @2;", configuracion.Id, fechaInicio.Date, fechaFin.Date.AddHours(23).AddMinutes(59).AddSeconds(59)).ToList();


            foreach (var infoAdicional in infoAdicionalNorasCredito)
            {
                var notaCredito = notasCredito.FirstOrDefault(f => f.IDDocumento == infoAdicional.IDDocumento);
                if (notaCredito != null)
                {
                    notaCredito.EstadoSistema = infoAdicional.Estado;
                    notaCredito.Cliente = infoAdicional.Cliente;
                    notaCredito.Transaccion = infoAdicional.Transaccion;
                    notaCredito.NumeroFacturaAfectada = infoAdicional.NumeroFacturaAfectada;
                }
            }

            return notasCredito;
        }

        public async Task<IEnumerable<FacturacionElectronicaRetencion>> GetRetencionesAsync(int idConfiguracion, DateTime fechaInicio, DateTime fechaFin)
        {
            var configuracion = await GetConfiguracionAsync(idConfiguracion);
            var sqlDatabase = DatabaseHelper.GetSqlDatabase(configuracion);

            var retencionesPendientesAgregar = sqlDatabase.Query<FacturacionElectronicaRetencion>(@"SELECT   CASE FacCabC.EstCod WHEN 'R' THEN CAST(1 as bit) ELSE CAST(0 as bit) END As EstadoSistema,     FacCabC.FacEnt AS Fecha, FacCabC.SerieRet AS Serie, FacCabC.NumRet AS Numero, CliMaeC.CliApl + ' ' + CliMaeC.CliNom AS Proveedor, 255 AS EstadoSri, FacCabC.TrnCod AS CodigoTransaccionCompra, 
                         FacCabC.TrnNum AS NumeroCompra, SUBSTRING(NroSer,1,3) + SUBSTRING(NroSer,4,3) + '-' + REPLICATE('0',9-LEN(GuiRem)) +  GuiRem As NumeroFacturaCompra,Trn.TrnDes AS TransaccionCompra, CAST(0 AS BIT) AS RideGenerado, CAST(0 AS BIT) AS XmlGenerado, CAST(0 AS BIT) AS NotificadoXMail, '' AS DireccionNotificacion
            FROM FacCabC INNER JOIN
                         FAC_ELE_Transacciones ON FacCabC.EmpCod = FAC_ELE_Transacciones.EmpCod AND 3 = FAC_ELE_Transacciones.ModIde AND FacCabC.TrnCod = FAC_ELE_Transacciones.TrnCod INNER JOIN
                         FAC_ELE_SeriesRet ON FacCabC.EmpCod = FAC_ELE_SeriesRet.EmpCod AND FacCabC.SerieRet = FAC_ELE_SeriesRet.SerieRet INNER JOIN
                         CliMaeC ON FacCabC.EmpCod = CliMaeC.EmpCod AND FacCabC.CliCod = CliMaeC.Clisec INNER JOIN
                         Trn ON FacCabC.EmpCod = Trn.EmpCod AND 3 = Trn.ModIde AND FacCabC.TrnCod = Trn.TrnCod LEFT OUTER JOIN
                         FAC_ELE_Documentos ON FacCabC.EmpCod = FAC_ELE_Documentos.EmpCod AND 3 = FAC_ELE_Documentos.ModIde AND FacCabC.TrnCod = FAC_ELE_Documentos.TrnCod AND 
                         FacCabC.TrnNum = FAC_ELE_Documentos.TrnNum
            WHERE (FacCabC.EmpCod = @EmpCod) AND (FacCabC.EstCod = 'R') AND (FacCabC.FacEnt >= @FechaInicio) AND (FacCabC.FacEnt <= @FechaFin) AND (FAC_ELE_Documentos.EmpCod IS NULL)",
            new
            {
                EmpCod = configuracion.CodigoEmpresa,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            }).ToList();

            int idDocumento = int.MaxValue;
            retencionesPendientesAgregar.ForEach(r => r.IDDocumento = idDocumento--);

            var retencionesAgregadas = sqlDatabase.Query<FacturacionElectronicaRetencion>(@"SELECT CASE FacCabC.EstCod WHEN 'R' THEN CAST(1 as bit) ELSE CAST(0 as bit) END As EstadoSistema, ID, FacCabC.FacEnt AS Fecha, FacCabC.SerieRet AS Serie, FacCabC.NumRet AS Numero, CliMaeC.CliApl + ' ' + CliMaeC.CliNom AS Proveedor, 255 AS EstadoSri, FacCabC.TrnCod AS CodigoTransaccionCompra, 
                         FacCabC.TrnNum AS NumeroCompra, SUBSTRING(NroSer,1,3) + SUBSTRING(NroSer,4,3) + '-' + REPLICATE('0',9-LEN(GuiRem)) +  GuiRem As NumeroFacturaCompra, Trn.TrnDes AS TransaccionCompra, CAST(0 AS BIT) AS RideGenerado, CAST(0 AS BIT) AS XmlGenerado, CAST(0 AS BIT) AS NotificadoXMail, '' AS DireccionNotificacion, 
                         FAC_ELE_Documentos.IDDocumento
            FROM  FacCabC INNER JOIN
                         FAC_ELE_Transacciones ON FacCabC.EmpCod = FAC_ELE_Transacciones.EmpCod AND 3 = FAC_ELE_Transacciones.ModIde AND FacCabC.TrnCod = FAC_ELE_Transacciones.TrnCod INNER JOIN
                         FAC_ELE_SeriesRet ON FacCabC.EmpCod = FAC_ELE_SeriesRet.EmpCod AND FacCabC.SerieRet = FAC_ELE_SeriesRet.SerieRet INNER JOIN
                         CliMaeC ON FacCabC.EmpCod = CliMaeC.EmpCod AND FacCabC.CliCod = CliMaeC.Clisec INNER JOIN
                         Trn ON FacCabC.EmpCod = Trn.EmpCod AND 3 = Trn.ModIde AND FacCabC.TrnCod = Trn.TrnCod INNER JOIN
                         FAC_ELE_Documentos ON FacCabC.EmpCod = FAC_ELE_Documentos.EmpCod AND 3 = FAC_ELE_Documentos.ModIde AND FacCabC.TrnCod = FAC_ELE_Documentos.TrnCod AND 
                         FacCabC.TrnNum = FAC_ELE_Documentos.TrnNum
            WHERE (FacCabC.EmpCod = @EmpCod) AND (FacCabC.EstCod = 'R') AND (FacCabC.FacEnt >= @FechaInicio) AND (FacCabC.FacEnt <= @FechaFin)",
            new
            {
                EmpCod = configuracion.CodigoEmpresa,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            }
            ).ToList();

            retencionesAgregadas.Where(r => r.IDDocumento == 0).ToList().ForEach(r => r.IDDocumento = idDocumento--);

            var estadoFacturacion = _mysqlDatabase.Query<dynamic>(@"SELECT ID, TrnCod, TrnNum, FechaEmision, ClaveAcceso, RideGenerado, XmlGenerado, NotificadoXMail, ErrorAlNotificar, EstadoSri 
            FROM sistema.fedocumentos where ConfiguracionId = @ConfiguracionId AND  EmpCod = @EmpCod AND TipoDocumentoSri=7 AND FechaEmision >= @FechaInicio AND FechaEmision<= @FechaFin;",
            new
            {
                EmpCod = configuracion.CodigoEmpresa,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                ConfiguracionId = configuracion.Id
            }).ToList();


            estadoFacturacion.ForEach(x =>
            {
                var retencion = retencionesAgregadas.FirstOrDefault(ret => ret.IDDocumento == x.ID);
                if (retencion != null)
                {
                    retencion.RideGenerado = x.RideGenerado;
                    retencion.XmlGenerado = x.RideGenerado;
                    retencion.ClaveAcceso = x.ClaveAcceso;
                    retencion.NotificadoXMail = x.NotificadoXMail;
                    retencion.EstadoSri = Convert.ToByte(x.EstadoSri);
                }
            });

            var result = new List<FacturacionElectronicaRetencion>(retencionesPendientesAgregar);

            result.AddRange(retencionesAgregadas);


            return result;
        }


        public async Task<FacturacionElectronicaRetencion> GetRetencionAsync(int idConfiguracion, int id)
        {
            var configuracion = await GetConfiguracionAsync(idConfiguracion);
            var sqlDatabase = DatabaseHelper.GetSqlDatabase(configuracion);


            var retencion = await sqlDatabase.FirstOrDefaultAsync<FacturacionElectronicaRetencion>(@"SELECT CASE FacCabC.EstCod WHEN 'R' THEN CAST(1 as bit) ELSE CAST(0 as bit) END As EstadoSistema, ID, FacCabC.FacEnt AS Fecha, FacCabC.SerieRet AS Serie, FacCabC.NumRet AS Numero, CliMaeC.CliApl + ' ' + CliMaeC.CliNom AS Proveedor, 255 AS EstadoSri, FacCabC.TrnCod AS CodigoTransaccionCompra, 
                         FacCabC.TrnNum AS NumeroCompra, SUBSTRING(NroSer,1,3) + SUBSTRING(NroSer,4,3) + '-' + REPLICATE('0',9-LEN(GuiRem)) +  GuiRem As NumeroFacturaCompra, Trn.TrnDes AS TransaccionCompra, CAST(0 AS BIT) AS RideGenerado, CAST(0 AS BIT) AS XmlGenerado, CAST(0 AS BIT) AS NotificadoXMail, '' AS DireccionNotificacion, 
                         FAC_ELE_Documentos.IDDocumento
            FROM  FacCabC INNER JOIN
                         FAC_ELE_Transacciones ON FacCabC.EmpCod = FAC_ELE_Transacciones.EmpCod AND 3 = FAC_ELE_Transacciones.ModIde AND FacCabC.TrnCod = FAC_ELE_Transacciones.TrnCod INNER JOIN
                         FAC_ELE_SeriesRet ON FacCabC.EmpCod = FAC_ELE_SeriesRet.EmpCod AND FacCabC.SerieRet = FAC_ELE_SeriesRet.SerieRet INNER JOIN
                         CliMaeC ON FacCabC.EmpCod = CliMaeC.EmpCod AND FacCabC.CliCod = CliMaeC.Clisec INNER JOIN
                         Trn ON FacCabC.EmpCod = Trn.EmpCod AND 3 = Trn.ModIde AND FacCabC.TrnCod = Trn.TrnCod INNER JOIN
                         FAC_ELE_Documentos ON FacCabC.EmpCod = FAC_ELE_Documentos.EmpCod AND 3 = FAC_ELE_Documentos.ModIde AND FacCabC.TrnCod = FAC_ELE_Documentos.TrnCod AND 
                         FacCabC.TrnNum = FAC_ELE_Documentos.TrnNum
            WHERE (FAC_ELE_Documentos.ID = @0)", id);

            if (retencion == null)
            {
                return null;
            }

            var estadoFacturacion = await _mysqlDatabase.FirstOrDefaultAsync<dynamic>(@"SELECT ID, TrnCod, TrnNum, FechaEmision, ClaveAcceso, RideGenerado, XmlGenerado, NotificadoXMail, ErrorAlNotificar, EstadoSri 
            FROM sistema.fedocumentos where ConfiguracionId = @ConfiguracionId AND EmpCod = @EmpCod  AND TrnCod = @TrnCod AND TrnNum = @TrnNum  AND TipoDocumentoSri=7;",
            new
            {
                EmpCod = configuracion.CodigoEmpresa,
                TrnCod = retencion.CodigoTransaccionCompra,
                TrnNum = retencion.Numero,
                ConfiguracionId = configuracion.Id
            });

            if (estadoFacturacion != null)
            {
                retencion.RideGenerado = estadoFacturacion.RideGenerado;
                retencion.XmlGenerado = estadoFacturacion.RideGenerado;
                retencion.ClaveAcceso = estadoFacturacion.ClaveAcceso;
                retencion.NotificadoXMail = estadoFacturacion.NotificadoXMail;
                retencion.EstadoSri = Convert.ToByte(estadoFacturacion.EstadoSri);
                retencion.IDDocumento = estadoFacturacion.ID;
            }
            return retencion;
        }

        public async Task<Stream> GetPdfAsync(int idDocumento)
        {
            var documento = await _mysqlDatabase.FirstOrDefaultAsync<dynamic>("SELECT * FROM sistema.fedocumentos WHERE sistema.fedocumentos.ID = @0", idDocumento);
            if (documento == null)
            {
                return null;
            }
            var configuracion = await GetConfiguracionAsync(documento.ConfiguracionId);

            var pathPdf = $"{configuracion.PathPDF}{documento.ClaveAcceso}.pdf";

            //Buffer 512kib
            var memStream = new MemoryStream(512 * 1024);

            using (var fileStream = System.IO.File.OpenRead(pathPdf))
            {
                await fileStream.CopyToAsync(memStream);
            }
            return memStream;
        }

        public async Task<Stream> GetPdfAsync(byte tipoDocumento, string codigo, int numero)
        {
            var documento = await _mysqlDatabase.FirstOrDefaultAsync<dynamic>("SELECT * FROM sistema.fedocumentos WHERE sistema.fedocumentos.TipoDocumentoSri = @0 AND sistema.fedocumentos.TrnCod = @1 AND sistema.fedocumentos.TrnNum = @2", tipoDocumento, codigo, numero);
            if (documento == null)
            {
                return null;
            }
            var configuracion = await GetConfiguracionAsync(documento.ConfiguracionId);

            var pathPdf = $"{configuracion.PathPDF}{documento.ClaveAcceso}.pdf";

            //Buffer 512kib
            var memStream = new MemoryStream(512 * 1024);

            using (var fileStream = System.IO.File.OpenRead(pathPdf))
            {
                await fileStream.CopyToAsync(memStream);
            }
            return memStream;
        }

        public async Task<Stream> GetXmlAsync(int idDocumento)
        {
            var documento = await _mysqlDatabase.FirstOrDefaultAsync<dynamic>("SELECT * FROM sistema.fedocumentos WHERE sistema.fedocumentos.ID = @0", idDocumento);
            if (documento == null)
            {
                return null;
            }
            var configuracion = await GetConfiguracionAsync(documento.ConfiguracionId);

            var pathXml = $"{configuracion.PathXmlAutorizados}{documento.ClaveAcceso}.xml";

            //Buffer 512kib
            var memStream = new MemoryStream(512 * 1024);

            using (var fileStream = System.IO.File.OpenRead(pathXml))
            {
                await fileStream.CopyToAsync(memStream);
            }
            return memStream;
        }

        public async Task<List<RegistroOperacionDocumento>> GetLogDocumentoAsync(int idDocumento)
        {
            var reader = await _mysqlDatabase.QueryAsync<RegistroOperacionDocumento>(@"SELECT FechaInicioOperacion, FechaFinOperacion, Registro FROM sistema.feregistrooperaciones where IdDocumento = @0", idDocumento);
            var list = new List<RegistroOperacionDocumento>(12);
            while (await reader.ReadAsync())
            {

                list.Add(reader.Poco);
            }
            return list;
        }

        public async Task<FacturacionElectronicaRetencion> IniciarProcesoRetencionAsync(int idConfiguracion, FacturacionElectronicaRetencion retencion)
        {
            var configuracion = await GetConfiguracionAsync(idConfiguracion);
            var sqlDatabase = DatabaseHelper.GetSqlDatabase(configuracion);
            var feDocumento = new FEDocumento
            {
                EmpCod = configuracion.CodigoEmpresa,
                ModIde = 3,
                TrnCod = retencion.CodigoTransaccionCompra,
                TrnNum = retencion.NumeroCompra,
                TipoSri = 7,
                Migrado = false,
                FechaEmision = retencion.Fecha,
                SerieDocumento = retencion.Serie,
                IDDocumento = 0,
                NumeroAuxiliar = retencion.Numero
            };
            var id = await sqlDatabase.InsertAsync(feDocumento);

            //esperamos autorizacion
            await Task.Delay(5000);

            retencion = await GetRetencionAsync(idConfiguracion, Convert.ToInt32(id));

            return retencion;


        }

        public async Task<int> GetNumeroPendientesAsync(int idConfiguracion, byte tipoDocumento)
        {
            System.Diagnostics.Debug.WriteLine($"GetNumeroPendientesAsync");
            if (tipoDocumento != 7)
            {
                return await _mysqlDatabase.ExecuteScalarAsync<int>("SELECT count(*) FROM sistema.fedocumentos where ConfiguracionId = @1 AND EstadoSri NOT IN (3,5) AND TipoDocumentoSri = @0;", tipoDocumento, idConfiguracion);
            }
            else//caso especial retenciones
            {
                //select * from FAC_ELE_Documentos where TipoSri = 7 and Migrado = 0
                var configuracion = await GetConfiguracionAsync(idConfiguracion);
                var sqlDatabase = DatabaseHelper.GetSqlDatabase(configuracion);
                var pendientes = await _mysqlDatabase.ExecuteScalarAsync<int>("SELECT count(*) FROM sistema.fedocumentos where ConfiguracionId = @1 AND EstadoSri NOT IN (3,5) AND TipoDocumentoSri = @0;", tipoDocumento, idConfiguracion);
                var noAgregadas = await sqlDatabase.ExecuteScalarAsync<int>(@"SELECT count(*) as Pendientes 
            FROM FacCabC INNER JOIN
                         FAC_ELE_Transacciones ON FacCabC.EmpCod = FAC_ELE_Transacciones.EmpCod AND 3 = FAC_ELE_Transacciones.ModIde AND FacCabC.TrnCod = FAC_ELE_Transacciones.TrnCod INNER JOIN
                         FAC_ELE_SeriesRet ON FacCabC.EmpCod = FAC_ELE_SeriesRet.EmpCod AND FacCabC.SerieRet = FAC_ELE_SeriesRet.SerieRet INNER JOIN
                         CliMaeC ON FacCabC.EmpCod = CliMaeC.EmpCod AND FacCabC.CliCod = CliMaeC.Clisec INNER JOIN
                         Trn ON FacCabC.EmpCod = Trn.EmpCod AND 3 = Trn.ModIde AND FacCabC.TrnCod = Trn.TrnCod LEFT OUTER JOIN
                         FAC_ELE_Documentos ON FacCabC.EmpCod = FAC_ELE_Documentos.EmpCod AND 3 = FAC_ELE_Documentos.ModIde AND FacCabC.TrnCod = FAC_ELE_Documentos.TrnCod AND 
                         FacCabC.TrnNum = FAC_ELE_Documentos.TrnNum
            WHERE (FacCabC.EmpCod = @0) AND (FacCabC.EstCod = 'R')  AND (FAC_ELE_Documentos.EmpCod IS NULL)", configuracion.CodigoEmpresa);
                //var noMigradas = await sqlDatabase.ExecuteScalarAsync<int>(@"select count(id) from FAC_ELE_Documentos where EmpCod = @0 AND TipoSri = 7 and Migrado = 0", configuracion.CodigoEmpresa);
                return pendientes + noAgregadas;
            }
        }


        public async Task<Tuple<bool, string>> ReintentarDocumentoAsync(int idDocumento)
        {
            try
            {
                var count = await _mysqlDatabase.ExecuteAsync("update sistema.fedocumentos set ridegenerado=0, xmlgenerado=0, puedecontinuar=1, operacionactual=1, isworking=0 where id = @0", idDocumento);
                if (count == 0)
                {
                    throw new InvalidOperationException($"No se pudo actualizar el documento con id {idDocumento}, documento no encontrado!!!");
                }
                return Tuple.Create(true, "Ok");
            }
            catch (Exception ex)
            {
                return Tuple.Create(false, ex.Message);
            }
        }


        public async Task<Tuple<bool, string>> ReenviarEmailDocumento(
          int idDocumento,
          string emails)
        {
            try
            {
                if (emails == null)
                    emails = "";
                if (await ((IExecuteAsync)this._mysqlDatabase).ExecuteAsync("update sistema.fedocumentos set notificadoxmail = 0, erroralnotificar = 0, \r\n                direccionnotificacion = @1, `puedecontinuar`='1', `operacionactual`='7', isworking = 0 where id = @0", new object[2]
                {
          (object) idDocumento,
          (object) emails
                }) == 0)
                    throw new InvalidOperationException(string.Format("No se pudo actualizar el documento con id {0}, documento no encontrado!!!", (object)idDocumento));
                return Tuple.Create<bool, string>(true, "Ok");
            }
            catch (Exception ex)
            {
                return Tuple.Create<bool, string>(false, ex.Message);
            }
        }

    }
}
