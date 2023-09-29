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
    public class BodegaController : ControllerBase
    {
        private readonly ILogger<BodegaController> logger;
        private readonly IBodegaRepository bodegaRepository;

        public BodegaController(ILogger<BodegaController> logger, IBodegaRepository bodegaRepository)
        {
            this.logger = logger;
            this.bodegaRepository = bodegaRepository;
        }


        [HttpGet(nameof(GetItemDetalleBodegas))]
        public async Task<ActionResult<List<BodegaItemDetallado>>> GetItemDetalleBodegas(int configuracionId, int itemId)
        {
            if (configuracionId ==0 || itemId ==0)
            {
                return BadRequest("ConfiguracionId, itemId");
            }
            var detalle = await bodegaRepository.GetBodegasItemDetallado(itemId);
            return detalle;
        }



    }
}