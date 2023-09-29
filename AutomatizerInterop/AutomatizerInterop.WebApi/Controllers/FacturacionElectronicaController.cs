using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomatizerInterop.Data.Entities;
using AutomatizerInterop.Data.Interfaces;
using AutomatizerInterop.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AutomatizerInterop.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FacturacionElectronicaController : ControllerBase
    {
        private readonly ILogger<AntivirusController> _logger;
        private readonly IFacturacionElectronicaRepository _repository;

        public FacturacionElectronicaController(ILogger<AntivirusController> logger, IFacturacionElectronicaRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }


        [HttpGet("Configuraciones")]
        public async Task<IEnumerable<ConfiguracionProgramaFacturacionElectronica>> GetConfiguracionesAsync()
        {
            return await _repository.GetConfiguracionesAsync();
        }

        [HttpGet("Facturas")]
        public async Task<ActionResult<RespuestaDocumentos<FacturacionElectronicaFactura>>> GetFacturasAsync(int idConfiguracion, DateTime fechaInicio, DateTime fechaFin)
        {
            if (idConfiguracion == 0)
            {
                return BadRequest();
            }
            var facturas = await _repository.GetFacturasAsync(idConfiguracion, fechaInicio, fechaFin);
            var pendientes = await _repository.GetNumeroPendientesAsync(idConfiguracion, 1);
            var respuesta = new RespuestaDocumentos<FacturacionElectronicaFactura>()
            {
                Documentos = facturas.ToList(),
                NumeroPendientes = pendientes
            };
            return respuesta;
        }

        //[HttpGet("Facturas")]
        //public async Task<ActionResult<IEnumerable<FacturacionElectronicaFactura>>> GetFacturasAsync([FromBody] DataSourceLoadOptions loadOptions, int idConfiguracion, DateTime fechaInicio, DateTime fechaFin)
        //{
        //    return _repository.GetFacturas(idConfiguracion, fechaInicio, fechaFin).ToList();
        //}


        [HttpGet("NotasCredito")]
        public async Task<ActionResult<RespuestaDocumentos<FacturacionElectronicaNotaCredito>>> GetNotasCreditoAsync(int idConfiguracion, DateTime fechaInicio, DateTime fechaFin)
        {
            var notasCredito = await _repository.GetNotasCreditoAsync(idConfiguracion, fechaInicio, fechaFin);
            var pendientes = await _repository.GetNumeroPendientesAsync(idConfiguracion, 4);
            var respuesta = new RespuestaDocumentos<FacturacionElectronicaNotaCredito>()
            {
                Documentos = notasCredito.ToList(),
                NumeroPendientes = pendientes
            };
            return respuesta;
        }



        [HttpGet("Retenciones")]
        public async Task<ActionResult<RespuestaDocumentos<FacturacionElectronicaRetencion>>> GetRetencionesAsync(int idConfiguracion, DateTime fechaInicio, DateTime fechaFin)
        {
            var retenciones = await _repository.GetRetencionesAsync(idConfiguracion, fechaInicio, fechaFin);
            var pendientes = await _repository.GetNumeroPendientesAsync(idConfiguracion, 7);
            var respuesta = new RespuestaDocumentos<FacturacionElectronicaRetencion>()
            {
                Documentos = retenciones.ToList(),
                NumeroPendientes = pendientes
            };
            return respuesta;
        }


        [HttpGet("GetPdf")]
        public async Task<FileStreamResult> GetPdfAsync(int idDocumento)
        {
            //var path = "<Get the file path using the ID>";
            var stream = await _repository.GetPdfAsync(idDocumento); //File.OpenRead(path);
            stream.Seek(0, System.IO.SeekOrigin.Begin);

            System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "Factura.pdf",
                Inline = true  // false = prompt the user for downloading;  true = browser to try to show the file inline
            };
            Response.Headers.Add("Content-Disposition", cd.ToString());
            Response.Headers.Add("X-Content-Type-Options", "nosniff");

            return new FileStreamResult(stream, "application/octet-stream");
        }


        [HttpGet("GetPdf2")]
        public async Task<FileStreamResult> GetPdfAsync(byte tipoDocumento, string codigo, int numero)
        {
            //var path = "<Get the file path using the ID>";
            //var stream = System.IO.File.OpenRead(@"E:\PathPdf\0102202001140079984500120011010000051181594837217.pdf"); 
            var stream = await _repository.GetPdfAsync(tipoDocumento, codigo, numero); //File.OpenRead(path);
            stream.Seek(0, System.IO.SeekOrigin.Begin);

            System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "Factura.pdf",
                Inline = true  // false = prompt the user for downloading;  true = browser to try to show the file inline
            };
            Response.Headers.Add("Content-Disposition", cd.ToString());
            Response.Headers.Add("X-Content-Type-Options", "nosniff");

            return new FileStreamResult(stream, "application/octet-stream");
        }

        [HttpGet("GetXml")]
        public async Task<FileStreamResult> GetXmlAsync(int idDocumento)
        {
            //var path = "<Get the file path using the ID>";
            var stream = await _repository.GetXmlAsync(idDocumento);
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "Factura.xml",
                Inline = true  // false = prompt the user for downloading;  true = browser to try to show the file inline
            };
            Response.Headers.Add("Content-Disposition", cd.ToString());
            Response.Headers.Add("X-Content-Type-Options", "nosniff");
            return new FileStreamResult(stream, "application/octet-stream");
        }

        [HttpGet("GetLogDocumento")]
        public async Task<List<RegistroOperacionDocumento>> GetLogDocumento(int idDocumento)
        {
            var logs = await _repository.GetLogDocumentoAsync(idDocumento);

            return logs;

        }


        [HttpPost("IniciarProcesoRetencion")]
        public async Task<object> IniciarProcesoRetencionAsync([FromQuery]int idConfiguracion, [FromBody]FacturacionElectronicaRetencion retencion)
        {
            retencion = await _repository.IniciarProcesoRetencionAsync(idConfiguracion, retencion);
            return retencion;
        }


        [HttpPost("ReintentarDocumento")]
        public async Task<Tuple<bool, string>> ReintentarDocumentoAsync([FromQuery]int idDocumento)
        {
            var r = await _repository.ReintentarDocumentoAsync(idDocumento);
            return r;
        }


        [HttpGet("GetNumeroPendientes")]
        public async Task<DocumentosPendientes> GetNumeroPendientes(int configuracionId)
        {
            await Task.Delay(1000);
            var pendientes = new DocumentosPendientes();
            pendientes.Factura = await _repository.GetNumeroPendientesAsync(configuracionId, 1);
            pendientes.NotaCredito = await _repository.GetNumeroPendientesAsync(configuracionId, 4);
            pendientes.Retencion = await _repository.GetNumeroPendientesAsync(configuracionId, 7);
            return pendientes;
        }


        [HttpGet("GetNumeroPendientesAll")]
        public async Task<List<DocumentosPendientes>> GetNumeroPendientes()
        {
            var configuraciones = await _repository.GetConfiguracionesAsync();
            var resultado = new List<DocumentosPendientes>(configuraciones.Count() + 1);
            System.Diagnostics.Debug.WriteLine($"Numero configuraciones {configuraciones.Count()}");
            foreach (var configuracion in configuraciones.ToList())
            {
                var pendientes = new DocumentosPendientes();
                pendientes.ConfiguracionId = configuracion.idConfiguracionPrograma;
                pendientes.ConfiguracionName = configuracion.NombreConfiguracion;
                pendientes.Factura = await _repository.GetNumeroPendientesAsync(configuracion.idConfiguracionPrograma, 1);
                pendientes.NotaCredito = await _repository.GetNumeroPendientesAsync(configuracion.idConfiguracionPrograma, 4);
                pendientes.Retencion = await _repository.GetNumeroPendientesAsync(configuracion.idConfiguracionPrograma, 7);
                resultado.Add(pendientes);
            }
            return resultado;
        }

    }
}