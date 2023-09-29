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
    public class ClientesController : ControllerBase
    {
        private readonly ILogger<ClientesController> logger;
        private readonly IClientesRepository clientesRepository;

        public ClientesController(ILogger<ClientesController> logger, IClientesRepository clientesRepository)
        {
            this.logger = logger;
            this.clientesRepository = clientesRepository;
        }


        //[HttpGet("Get")]
        //public async Task<ActionResult<Cliente>> GetCliente(string codigoCliente, bool fullData)
        //{
        //    //try
        //    //{
        //    //    return await clientesRepository.GetCliente(codigoCliente, fullData);
        //    //}
        //    //catch (InvalidOperationException ioe)
        //    //{
        //    //    return NotFound();
        //    //}
        //    //catch
        //    //{
        //    //    return BadRequest();
        //    //}

        //}




        //[HttpGet("GetClientes")]
        //public async Task<ActionResult<List<Cliente>>> GetClientes([FromBody]List<QueryFilter> queryFilters)
        //{
        //    //try
        //    //{
        //    //    return await clientesRepository.GetClientes(queryFilters);
        //    //}
        //    //catch
        //    //{
        //    //    return BadRequest();
        //    //}

        //}



        [HttpGet("GetContactosCliente")]
        public async Task<ActionResult<List<ContactoCliente>>> GetContactosCliente(string clienteCodigo)
        {
            try
            {
                var contactos = await clientesRepository.GetContactosCliente(clienteCodigo);
                return contactos;
            }
            catch
            {
                return BadRequest();
            }
        }


        [HttpPost(nameof(UpdateEmail))]
        public async Task<ActionResult> UpdateEmail(string clienteCodigo, string email)
        {
            try
            {
                await clientesRepository.UpdateEmail(clienteCodigo, 1, email);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}