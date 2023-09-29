using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutomatizerInterop.Data.Entities;
using AutomatizerInterop.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AutomatizerInterop.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StoreSyncController : ControllerBase
    {

        public class CategoriasSincronizar
        {
            public string idsCategorias { get; set; }
            public string idsCategoriasOmitir { get; set; }
        }

        private readonly ILogger<StoreSyncController> _logger;
        private readonly IStoreSyncRepository _storeSyncRepository;

        public StoreSyncController(ILogger<StoreSyncController> logger, IStoreSyncRepository storeSyncRepository)
        {
            _logger = logger;
            _storeSyncRepository = storeSyncRepository;

        }


        [HttpPost("SetCategoriasSincronizar")]
        public async Task<IActionResult> SetCategoriasSincronizar([FromBody]CategoriasSincronizar categorias, int configuracionId)
        {
            if (categorias == null)
            {
                return BadRequest(nameof(categorias));
            }

            if (categorias.idsCategorias == "")
            {
                categorias.idsCategorias = "0";
            }

            if (categorias.idsCategoriasOmitir == "")
            {
                categorias.idsCategoriasOmitir = "0";
            }
                
            try
            {
                await _storeSyncRepository.UpdateCategoriesToSync(categorias.idsCategorias, categorias.idsCategoriasOmitir ?? "0");
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }


        [HttpGet(nameof(GetErroresSincronizacionProductos))]
        public async Task<List<ErrorSincronizacionProducto>> GetErroresSincronizacionProductos(int idConfiguracion)
        {
            return await _storeSyncRepository.GetErroresSincronizacionProductos(idConfiguracion);
        }


        [HttpGet(nameof(GetProductosSinImagen))]
        public async Task<List<ProductoSinImagen>> GetProductosSinImagen(int idConfiguracion)
        {
            return await _storeSyncRepository.GetProductosSinImagen(idConfiguracion);
        }

    }




}