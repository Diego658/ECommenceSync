using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutomatizerInterop.Data.Entities;
using AutomatizerInterop.Data.Entities.Antivirus;
using AutomatizerInterop.Data.Interfaces;
using AutomatizerInterop.Data.Models.Antivirus;
using AutomatizerInterop.WebApi.Models;
using AutomatizerInterop.WebApi.Models.Antivirus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PetaPoco;

namespace AutomatizerInterop.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AntivirusController : ControllerBase
    {
        private readonly ILogger<AntivirusController> _logger;
        private readonly IAntivirusRepository _antivirusRepository;
        private readonly IFacturacionRepository facturacionRepository;
        private readonly IClientesRepository clientesRepository;

        public AntivirusController(ILogger<AntivirusController> logger, IAntivirusRepository antivirusRepository, IFacturacionRepository facturacionRepository, IClientesRepository clientesRepository)
        {
            _logger = logger;
            _antivirusRepository = antivirusRepository;
            this.facturacionRepository = facturacionRepository;
            this.clientesRepository = clientesRepository;
        }



        [HttpGet("GetAntiviruses")]
        public async Task<List<Antivirus>> GetAntiviruses(int idConfiguracion)
        {
            return await _antivirusRepository.GetAntivirusesAsync(idConfiguracion);
        }


        [HttpGet("GetVentasPorRenovar")]
        public async Task<object> GetAntivirusVentas(int idConfiguracion, byte tipo, bool mostrarVencimientosFuturos = false, bool full = false)
        {
            if (full)
            {
                var ventas = await _antivirusRepository.GetVentasPorRenovarFullInfo(idConfiguracion, tipo, mostrarVencimientosFuturos);
                return new
                {
                    ventas,
                    numeroRegistros = ventas.Count,
                    minValue = ventas.Count > 0 ? ventas.Min(venta => venta.FechaVencimiento).Date : DateTime.Now.Date,
                    maxValue = ventas.Count > 0 ? ventas.Max(venta => venta.FechaVencimiento).Date : DateTime.Now.Date.AddDays(1).AddSeconds(-1),
                };
            }
            else
            {
                var ventas = await _antivirusRepository.GetVentasPorRenovar(idConfiguracion, mostrarVencimientosFuturos);
                return ventas;
            }


        }



        [HttpGet("GetEstadoLicenciasAll")]
        public async Task<ActionResult<List<EmpresaResumenEstadoLicenciasAntivirus>>> GetEstadoLicenciasAll()
        {
            var configuraciones = _antivirusRepository.GetConfiguraciones();
            var resultado = new List<EmpresaResumenEstadoLicenciasAntivirus>(5);
            foreach (var configuracion in configuraciones)
            {
                var estado = await _antivirusRepository.GetResumenEstadoLicenciasAntivirus(configuracion.idConfiguracionPrograma);
                resultado.Add(new EmpresaResumenEstadoLicenciasAntivirus
                {
                    ConfiguracionId = configuracion.idConfiguracionPrograma,
                    NombreConfiguracion = configuracion.NombreConfiguracion,
                    Estados = estado
                });
            }
            return resultado;
        }


        [HttpGet("GetEstadoLicencias")]
        public async Task<ActionResult<EmpresaResumenEstadoLicenciasAntivirus>> GetEstadoLicencias(int configuracionId)
        {
            await Task.Delay(500);
            var estado = await _antivirusRepository.GetResumenEstadoLicenciasAntivirus(configuracionId);
            var configuracion = _antivirusRepository.GetConfiguracion(configuracionId);
            var resultado = new EmpresaResumenEstadoLicenciasAntivirus
            {
                ConfiguracionId = configuracionId,
                NombreConfiguracion = configuracion.NombreConfiguracion,
                Estados = estado
            };
            return resultado;
        }


        [HttpGet("GetSaldoLicenciaVsLicenciasPorRenovar")]
        public async Task<ActionResult<List<SaldoLicenciaVsLicenciasPorRenovar>>> GetSaldoLicenciaVsLicenciasPorRenovar(int configuracionId)
        {
            return await _antivirusRepository.GetSaldoLicenciaVsLicenciasPorRenovarsAsync(configuracionId);
        }

        [HttpGet("GetPlantillasNotificacion")]
        public async Task<ActionResult<List<AntivirusNotificacionesPlantilla>>> GetPlantillasNotificacion(int configuracionId)
        {
            return await _antivirusRepository.GetPlantillasNotificacion(configuracionId);
        }


        [HttpPost("AddPlantillaNotificacion")]
        public async Task<ActionResult<AntivirusNotificacionesPlantilla>> AddPlantillaNotificacion(int configuracionId, string nombre)
        {
            return await _antivirusRepository.AddPlantillaNotificacion(configuracionId, nombre);
        }


        [HttpPut("GuardarPlantillaNotificacion")]
        public async Task<ActionResult<bool>> GuardarPlantillaNotificacion([FromQuery] int configuracionId,
            [FromBody] object plantilla)
        {
            if (configuracionId == 0)
            {
                return new BadRequestObjectResult("{\"mensaje:\" \"Configuracion no establecida\"}");
            }

            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(plantilla.ToString());

            //return new OkObjectResult("Ok");
            return await _antivirusRepository.UpdatePlantillaNotificacion(configuracionId, new AntivirusNotificacionesPlantilla
            {
                PlantillaID = obj.Value<byte>("plantillaID"),
                FechaModificacion = DateTime.Now,
                NombrePlantilla = obj.Value<string>("nombrePlantilla"),
                UsuarioModifica = "",
                Contenido = obj.GetValue("contenido").ToString(),
                ContenidoHtml = obj.GetValue("contenidoHtml").ToString()
            });
        }


        [HttpGet(nameof(GetInformacionNotificacion))]
        public async Task<ActionResult<AntivirusHistorialNotificacionFullData>> GetInformacionNotificacion(int notificacionId, bool fullData)
        {
            return await _antivirusRepository.GetInformacionNotificacion(notificacionId, fullData);
        }



        [HttpGet(nameof(GetInformacionVentaAntivirus))]
        public async Task<object> GetInformacionVentaAntivirus(int ventaId, bool fullData)
        {
            if (fullData)
            {
                var ventaAntivirus = await _antivirusRepository.GetFullInformacionVentaAntivirus(ventaId);
                var factura = await facturacionRepository.GetInformacionFactura(ventaAntivirus.TrnCod, ventaAntivirus.TrnNum, false);
                var cliente = await clientesRepository.GetCliente(factura.ClienteCodigo, false);

                return new
                {
                    ventaAntivirus,
                    factura,
                    cliente
                };
            }
            else
            {
                return await _antivirusRepository.GetInformacionVentaAntivirus(ventaId);
            }

        }



        [HttpPost(nameof(RegistrarNotificacion))]
        public async Task<ActionResult<bool>> RegistrarNotificacion([FromQuery] int configuracionId, int ventaId, [FromBody] AntivirusNotificacionModel notificacion)
        {
            try
            {
                await _antivirusRepository.RegistrarNotificacion(ventaId, notificacion);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }


        [HttpPost(nameof(RegistrarNotificacion))]
        public async Task<ActionResult<bool>> OcultarLicencia([FromQuery] int configuracionId, int ventaId, [FromBody] string motivo)
        {
            try
            {
                var user = HttpContext.User;
                await _antivirusRepository.OcultarLicencia(ventaId, 1,motivo );
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }


        [HttpGet(nameof(GetFacturasParaRenovacion))]
        public async Task<object> GetFacturasParaRenovacion(string transaccionCodigo, string clienteCodigo)
        {
            var repository = _antivirusRepository.GetGenericRepository();

            return await repository.GetSQLDatabase().FetchAsync<dynamic>(@"SELECT CONCAT(fac.SerieFact, ' - ',  REPLICATE('0', 9-LEN(av.TrnNum)),  av.TrnNum) As Numero,  av.KardexId As Id 
            FROM AntivirusVentas av
            INNER JOIN FacCab fac 
                ON fac.EmpCod = av.EmpCod AND fac.TrnCod = av.TrnCod AND fac.TrnNum = av.TrnNum
            INNER JOIN KarDet k ON k.RecNum = av.KardexId
            WHERE av.EmpCod = @0 AND av.TrnCod = @1  AND Renovada = 0 AND FechaVencimiento > DATEADD(MONTH, 6, GETDATE()) AND (fac.CliCod = @2) 
            ORDER BY Numero", repository.ConfiguracionProvider.GetConfiguracion(repository.ConfiguracionID).CodigoEmpresa, transaccionCodigo, clienteCodigo);
        }


        [HttpPost(nameof(RegistrarRenovacion))]
        public async Task<ActionResult> RegistrarRenovacion([FromBody] RegistrarRenovacionModel renovacionModel)
        {
            if (renovacionModel == null)
            {
                return new BadRequestResult();
            }
            try
            {
                var venta = await _antivirusRepository.GetInformacionVentaAntivirus(renovacionModel.VentaId.Value);
                if (venta.Renovada)
                {
                    return BadRequest("La venta ya se encuentra renovada.");
                }
                var renovacion =
                    await _antivirusRepository
                    .RegistrarRenovacion(renovacionModel.VentaId.Value, renovacionModel.EmpleadoId.Value, renovacionModel.RenovacionId.Value);
                return Ok(renovacion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }


    }
}