using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutomatizerInterop.Data.Entities;
using AutomatizerInterop.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AutomatizerInterop.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ConfiguracionesInteropController : ControllerBase
    {
        private readonly ILogger<AntivirusController> logger;
        private readonly IInteropConfiguracionProvider configuracionProvider;

        public ConfiguracionesInteropController(ILogger<AntivirusController> logger, IInteropConfiguracionProvider configuracionProvider)
        {
            this.logger = logger;
            this.configuracionProvider = configuracionProvider;
        }


        [HttpGet("GetAll")]
        public async Task<ActionResult<object>> GetAll()
        {
            //await Task.Delay(2500);
            var configuraciones = await configuracionProvider.GetConfiguracionesAsync();

            return configuraciones.Select(x=> new 
            {
                x.idConfiguracionPrograma,
                x.NombreConfiguracion
            } ).ToList();
        }

        [HttpGet("Get")]
        public async Task<ActionResult<object>> Get(int idConfiguracion)
        {
            //await Task.Delay(2500);
            var configuracion = await configuracionProvider.GetConfiguracionAsync(idConfiguracion);
            return new 
            {
                configuracion.idConfiguracionPrograma,
                configuracion.NombreConfiguracion
            };

        }


    }
}