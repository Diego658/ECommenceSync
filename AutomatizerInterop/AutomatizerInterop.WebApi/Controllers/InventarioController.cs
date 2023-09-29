using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using AutomatizerInterop.Data.Entities;
using AutomatizerInterop.Data.Interfaces;
using AutomatizerInterop.WebApi.Data;
using AutomatizerInterop.WebApi.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using AutomatizerInterop.Data.Entities.Inventario.Dtos;

namespace AutomatizerInterop.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InventarioController : ControllerBase
    {
        private readonly ILogger<InventarioController> _logger;
        private readonly IInventarioRepository _inventarioRepository;
        private readonly AppDbContext _appDbContext;
        
        public InventarioController(ILogger<InventarioController> logger, IInventarioRepository inventarioRepository, AppDbContext appDbContext)
        {
            _logger = logger;
            _inventarioRepository = inventarioRepository;
            _appDbContext = appDbContext;

        }

        [HttpPost("MigrarImagenesCategoriasAutomatizerToBlob")]
        public async Task MigrarImagenesAutomatizerToBlob(int configuracionId)
        {
            await Task.Delay(4000);
            var autoConfigRepository = (AutoConfigRepository)_inventarioRepository;
            var imagenes = autoConfigRepository.GetSQLDatabase().Query<dynamic>("SELECT itemID = secuencial, itmlgo from ItmMae where ItmTip = 'G' and LEN(itmlgo)>0");
            foreach (var imagen in imagenes)
            {
                if (System.IO.File.Exists(imagen.itmlgo))
                {
                    
                    using (FileStream stream = System.IO.File.OpenRead(imagen.itmlgo))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await stream.CopyToAsync(memoryStream);
                            var blob = _appDbContext.Blobs.Add(new Models.Blob { Data = memoryStream.ToArray(), UploadDT = DateTimeOffset.Now  });
                            await _appDbContext.SaveChangesAsync();
                            await _inventarioRepository.AddImagenItem(imagen.itemID, "category_default", blob.Entity.Id);
                        }
                        
                    }
                    
                }
                
            }
            await autoConfigRepository.ExecuteAsync(@"UPDATE       ItmMae
            SET itmlgo = ''
            FROM StoreSync_ImagenesItems INNER JOIN
                  ItmMae ON StoreSync_ImagenesItems.ItemID = ItmMae.Secuencial
            WHERE (ItmMae.ItmTip = 'G') AND (StoreSync_ImagenesItems.Operacion <> 'delete')");
        }


        [HttpPost("MigrarImagenesItemsAutomatizerToBlob")]
        public async Task MigrarImagenesItemsAutomatizerToBlob(int configuracionId)
        {
            await Task.Delay(1000);
            var autoConfigRepository = (AutoConfigRepository)_inventarioRepository;
            var imagenes = autoConfigRepository.GetSQLDatabase().Query<dynamic>("SELECT Secuencial, PathImagen FROM PS_Items_ImagenesItemsPs WHERE(Estado = 1) ORDER BY Orden");
            int idAnterior = 0;
            string imageType;
            foreach (var imagen in imagenes)
            {
                if (System.IO.File.Exists(imagen.PathImagen))
                {

                    using (FileStream stream = System.IO.File.OpenRead(imagen.PathImagen))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await stream.CopyToAsync(memoryStream);
                            var blob = _appDbContext.Blobs.Add(new Models.Blob { Data = memoryStream.ToArray(), UploadDT = DateTimeOffset.Now });
                            await _appDbContext.SaveChangesAsync();
                            imageType = idAnterior == imagen.Secuencial? "normal":"cover";
                            await _inventarioRepository.AddImagenItem(imagen.Secuencial, imageType , blob.Entity.Id);
                        }

                    }

                }
                idAnterior = imagen.Secuencial;
            }
           


//            imagenes =  autoConfigRepository.GetSQLDatabase().Query<dynamic>(@"WITH rootCte AS (SELECT        EmpCod AS CodigoEmpresa, ItmCod AS Codigo, ItmDsc AS Nombre, Secuencial AS ItemId, ItmNiv AS Nivel, NULL AS PadreID, ItmTip, Imagen = cast('\\SERVIDOR\helpcomp arbol\' + ItmDsc as varchar(500))
//                                      FROM            dbo.ItmMae
//                                      WHERE        (ItmTip = 'G') AND (ItmNiv = 1)
//                                      UNION ALL
//                                      SELECT        itm.EmpCod AS CodigoEmpresa, itm.ItmCod AS Codigo, itm.ItmDsc AS Nombre, itm.Secuencial AS ItemId, itm.ItmNiv AS Nivel, rootCte.ItemId AS PadreID, itm.ItmTip, 
//									  Imagen = CAST(rootCte.Imagen + '\' + (case itm.ItmTip when 'G' then ItmDsc else (ItmCodVen+ '\' + ItmCodVen) end ) as varchar(500))
//                                      FROM            rootCte INNER JOIN
//                                                               dbo.ItmMae AS itm ON itm.EmpCod = rootCte.CodigoEmpresa AND itm.ItmNiv = rootCte.Nivel + 1
//                                      WHERE        (itm.ItmCod LIKE rootCte.Codigo + '%'))
//,
//paths
//as
//(
//    SELECT        ItemId, PadreID, Imagen 
//     FROM rootCte
//	 INNER JOIN PS_Items_ImagenesItemsPs img
//	 ON rootcte.itemid = img.Secuencial
//	 )
//	 select Secuencial = ItemId, PathImagen = Imagen 
//	 from paths
//");
//            idAnterior = 0;
//            imageType="";
//            var extensiones = new List<string>() { ".jpg", ".jpeg", ".png" };
//            foreach (var imagen in imagenes)
//            {
//                foreach (var extension in extensiones)
//                {
//                    if (System.IO.File.Exists(imagen.PathImagen + extension))
//                    {

//                        using (FileStream stream = System.IO.File.OpenRead(imagen.PathImagen + extension))
//                        {
//                            using (var memoryStream = new MemoryStream())
//                            {
//                                await stream.CopyToAsync(memoryStream);
//                                var blob = _appDbContext.Blobs.Add(new Models.Blob { Data = memoryStream.ToArray(), UploadDT = DateTimeOffset.Now });
//                                await _appDbContext.SaveChangesAsync();
//                                imageType = idAnterior == imagen.Secuencial ? "normal" : "cover";
//                                await _inventarioRepository.AddImagenItem(imagen.Secuencial, imageType, blob.Entity.Id);
//                            }

//                        }

//                    }
//                    idAnterior = imagen.Secuencial;
//                    break;
//                }
                
//            }
            await autoConfigRepository.ExecuteAsync(@"DELETE FROM PS_Items_ImagenesItemsPs
FROM            StoreSync_ImagenesItems INNER JOIN
                         PS_Items_ImagenesItemsPs ON StoreSync_ImagenesItems.ItemID = PS_Items_ImagenesItemsPs.Secuencial
WHERE        (StoreSync_ImagenesItems.Operacion <> 'delete') ");
        }


        [HttpGet(nameof(GetArbolItems))]
        public Task<List<ItemInventario>> GetArbolItems(int configuracionId)
        {
            return _inventarioRepository.GetArbolItems();
        }

        [HttpGet("GetSubitems")]
        public async Task<ActionResult<List<ItemInventario>>> GetSubitems(int configuracionId, string codigoPadre, byte nivel)
        {
            if (string.IsNullOrWhiteSpace(codigoPadre)) codigoPadre = string.Empty;
            var subItems = await _inventarioRepository.GetItemsHijos(codigoPadre, nivel);
            subItems.ForEach(s => s.CodigoPadre = s.Nivel == 1 ? null : s.CodigoPadre);
            return subItems;
        }


        [HttpGet("GetCategoriaInventario")]
        public async Task<ActionResult<dynamic>> GetCategoriaInventario(int configuracionId,int categoriaID)
        {
            var categoria = await _inventarioRepository.GetCategoriaInventario(categoriaID);
            return categoria;
        }

        [HttpGet("GetItemDynamic")]
        public async Task<ActionResult<dynamic>> GetItemDynamic(int configuracionId, int itemID)
        {
            var categoria = await _inventarioRepository.GetItemDynamic(itemID);
            return categoria;
        }


        [HttpPut(nameof(UpdateItem))]
        public async Task<ActionResult<dynamic>> UpdateItem(int configuracionId, int itemId,  [FromBody]ExpandoObject jObject)
        {
            try
            {
                await _inventarioRepository.UpdateItem(jObject, itemId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
                       
        }

        [HttpPost(nameof(GuardarMarca))]
        public async Task<ActionResult<dynamic>> GuardarMarca(int configuracionId, int marcaId, [FromBody]Marca marca)
        {
            try
            {
                await _inventarioRepository.GuardarMarca(marcaId, marca);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }



        [HttpGet("GetImagenesItem")]
        public async Task<ActionResult<List<ImagenItem>>> GetImagenesItem(int configuracionId, int itemID, bool muestraEliminadas = false)
        {
            var imagenes = await _inventarioRepository.GetImagenesItem(itemID);
            if (!muestraEliminadas)
            {
                imagenes = imagenes.Where(i => i.Operacion != "delete").ToList();
            }
            return imagenes;

        }


        [HttpDelete("DeleteImageCategory")]
        public async Task<ActionResult> DeleteImageCategory(int configuracionId, int categoriaId, long blobId)
        {
            //_appDbContext.Blobs.Remove(new Models.Blob { Id = blobId });
            await _inventarioRepository.SetImageItemDeleted(categoriaId, blobId);
            //await _appDbContext.SaveChangesAsync();
            return Ok();
        }


        [HttpPost("AddImagenItem")]
        public async Task<ActionResult> AddImagenItem(int configuracionId, int itemID, string type, long blobId)
        {
            if (configuracionId <= 0) return BadRequest("configuracionId!!!");
            if (itemID <= 0) return BadRequest("ItemID!!!");
            if (string.IsNullOrEmpty(type) ) return BadRequest("ItemID!!!");
            if (blobId <= 0) return BadRequest("blobId!!!");

            await _inventarioRepository.AddImagenItem(itemID, type, blobId);
            return Ok();
        }

        [HttpDelete("DeleteImagenItem")]
        public async Task<ActionResult> DeleteImagenItem(int configuracionId, int itemID, int imageId, int blobId, string type = "ITEM")
        {
            if (configuracionId <= 0) return BadRequest("configuracionId!!!");

            if (imageId <= 0 && type == "ITEM") return BadRequest("imageId!!!");
            if (itemID <= 0) return BadRequest("itemID!!!");
            if (blobId <= 0) return BadRequest("blobId!!!");

            await _inventarioRepository.DeleteImagenItem(type, itemID, imageId, blobId);

            //if (type == "ITEM" )
            //{
            //    await _inventarioRepository.SetImageItemDeleted(blobId);
            //}
            //else if (type == "MARCA")
            //{
            //    _inventarioRepository.DeleteImage(type,  )
            //}

            //_appDbContext.Blobs.Remove(new Models.Blob { Id = blobId });
            
            //await _appDbContext.SaveChangesAsync();
            return Ok();
        }


        [HttpGet("GetUnidades")]
        public async Task<ActionResult<List<Unidad>>> GetUnidades(int configuracionId)
        {
            if (configuracionId <= 0) return BadRequest("configuracionId!!!");
            return await _inventarioRepository.GetUnidades();
        }

        [HttpGet("GetMarcas")]
        public async Task<ActionResult<List<Marca>>> GetMarcas(int configuracionId)
        {
            if (configuracionId <= 0) return BadRequest("configuracionId!!!");
            return await _inventarioRepository.GetMarcas();
        }

        [HttpGet(nameof(GetMarca))]
        public async Task<ActionResult<Marca>> GetMarca(int configuracionId, int marcaId)
        {
            if (configuracionId <= 0) return BadRequest("configuracionId!!!");
            if (marcaId <= 0) return BadRequest("codigo!!!");
            return await _inventarioRepository.GetMarca(marcaId);
        }

        [HttpGet("GetTallas")]
        public async Task<ActionResult<List<Talla>>> GetTallas(int configuracionId)
        {
            if (configuracionId <= 0) return BadRequest("configuracionId!!!");
            return await _inventarioRepository.GetTallas();
        }

        [HttpGet(nameof(GetColores))]
        public async Task<ActionResult<List<Color>>> GetColores(int configuracionId)
        {
            if (configuracionId <= 0) return BadRequest("configuracionId!!!");
            return await _inventarioRepository.GetColores();
        }


        [HttpGet(nameof(GetProcedencias))]
        public async Task<ActionResult<List<Procedencia>>> GetProcedencias(int configuracionId)
        {
            if (configuracionId <= 0) return BadRequest("configuracionId!!!");
            return await _inventarioRepository.GetProcedencias();
        }


        [HttpGet(nameof(GetOrigenes))]
        public ActionResult<List<Procedencia>> GetOrigenes(int configuracionId)
        {
            if (configuracionId <= 0) return BadRequest("configuracionId!!!");
            return new List<Procedencia> { 
                new Procedencia { Codigo =0, Nombre = "No Especificado"  } ,
                new Procedencia { Codigo =1, Nombre = "Reprint"  },
                new Procedencia { Codigo =2, Nombre = "Original"  },
                new Procedencia { Codigo =3, Nombre = "OEM"  },
                new Procedencia { Codigo =4, Nombre = "Assembled"  }


         
            };
        }

        [HttpGet(nameof(Atributos))]
        public  Task<List<AtributoDto>> Atributos(int configuracionId)
        {
            return _inventarioRepository.GetAtributos();
        }





    }
}