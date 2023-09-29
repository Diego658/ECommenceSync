using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutomatizerInterop.Data.Entities.Transacciones;
using AutomatizerInterop.Data.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AutomatizerInterop.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransaccionesController : ControllerBase
    {
        private readonly ILogger<TransaccionesController> _logger;
        private readonly ITransaccionesRepository transaccionesRepository;
        
        public TransaccionesController(ILogger<TransaccionesController> logger, ITransaccionesRepository transaccionesRepository)
        {
            _logger = logger;
            this.transaccionesRepository = transaccionesRepository;
        }

        [HttpGet(nameof(GetTransacciones))]
        public Task<List<Transaccion>> GetTransacciones(byte modulo, string categoria)
        {
            return transaccionesRepository.GetTransacciones(modulo, categoria);
        }


    }
}