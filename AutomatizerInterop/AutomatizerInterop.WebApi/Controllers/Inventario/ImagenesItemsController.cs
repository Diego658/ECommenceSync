using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutomatizerInterop.Data.Entities.Inventario;
using AutomatizerInterop.Data.EntityFramewrok;
using Microsoft.AspNetCore.Authorization;
using AutomatizerInterop.Data.Interfaces;
using AutomatizerInterop.Data.Entities;
using System.Net.Http.Headers;
using System.IO;
using System.Security.Cryptography;
using AutomatizerInterop.WebApi.Data;
using System.Text;

namespace AutomatizerInterop.WebApi.Controllers.Inventario
{
    [Authorize]
    [Route("api/Automatizer/Inventario/[controller]")]
    [ApiController]
    public class ImagenesItemsController : ControllerBase
    {
        private readonly AutomatizerInteropDbContext _context;
        private readonly AppDbContext _blobContext;
        private readonly ConfiguracionProgramaFacturacionElectronica _configuracion;

        public ImagenesItemsController(AutomatizerInteropDbContext context, AppDbContext blobContext, IInteropConfiguracionProvider configuracionProvider)
        {
            _context = context;
            _blobContext = blobContext;
            _configuracion = configuracionProvider.GetConfiguracion(context.ConfiguracionId);
        }

        // GET: api/ImagenesItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AutomatizerInterop.Data.Entities.Inventario.ImagenItem>>> GetImagenesItems(int? itemId)
        {
            var imagenes = _context.ImagenesItems.Where(i => i.Operacion != "delete" && i.Item.EmpCod == _configuracion.CodigoEmpresa);
            if (itemId.HasValue)
            {
                imagenes = imagenes.Where(i => i.ItemId == itemId);
            }
            return await imagenes.ToListAsync();
        }

        // GET: api/ImagenesItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AutomatizerInterop.Data.Entities.Inventario.ImagenItem>> GetImagenItem(int id)
        {
            var imagenItem = await _context.ImagenesItems.FindAsync(id);

            if (imagenItem == null)
            {
                return NotFound();
            }

            return imagenItem;
        }

        // PUT: api/ImagenesItems/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutImagenItem(int id, AutomatizerInterop.Data.Entities.Inventario.ImagenItem imagenItem)
        {
            if (id != imagenItem.Id)
            {
                return BadRequest();
            }

            

            if (imagenItem.Type == "cover")
            {
                var imagenesItem = _context.ImagenesItems.Where(i => i.ItemId == imagenItem.ItemId && i.Id != id && i.Type == "cover").AsTracking();
                foreach (var imagen in imagenesItem)
                {
                    imagen.Type = "normal";
                    imagen.FechaModificacion = DateTimeOffset.Now;
                }
            }

            var imageDb = _context.ImagenesItems.Find(id);
            imageDb.FechaModificacion = DateTimeOffset.Now;
            imageDb.Type = imagenItem.Type ?? imageDb.Type;
            imageDb.ImageAlt = imagenItem.ImageAlt ?? imageDb.ImageAlt;
            imageDb.Operacion = imagenItem.Operacion ?? imageDb.Operacion;

            _context.Entry(imageDb).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ImagenItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ImagenesItems
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<AutomatizerInterop.Data.Entities.Inventario.ImagenItem>> PostImagenItem(AutomatizerInterop.Data.Entities.Inventario.ImagenItem imagenItem)
        {
            _context.ImagenesItems.Add(imagenItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetImagenItem", new { id = imagenItem.Id }, imagenItem);
        }


        [HttpPost("Upload"), DisableRequestSizeLimit]
        public async Task<ActionResult<List<AutomatizerInterop.Data.Entities.Inventario.ImagenItem>>> Upload(int? itemId)
        {
            if (!itemId.HasValue)
            {
                return BadRequest();
            }
            try
            {
                var imagenesNuevas = new List<AutomatizerInterop.Data.Entities.Inventario.ImagenItem>(5);
                foreach (var file in Request.Form.Files)
                {
                    if (file.Length <= 0) continue;
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    using (var stream = new MemoryStream())
                    {
                        await file.CopyToAsync(stream);
                        var bufferBytes = stream.ToArray();
                        byte[] hashResult;
                        var shaM = new SHA512Managed();
                        hashResult = shaM.ComputeHash(bufferBytes);
                        var stringResult = GetStringHash(hashResult);
                        var blob = _blobContext
                            .Blobs
                            .Where(b => b.HashString == stringResult)
                            .Select(b => new { blobId = b.Id }).FirstOrDefault();
                        if(blob is null)
                        {
                            var appFile = new Models.Blob
                            {
                                Data = stream.ToArray(),
                                UploadDT = DateTime.UtcNow,
                                Extension = fileName.Substring(fileName.LastIndexOf('.'), fileName.Length - fileName.LastIndexOf('.')),
                                HashString = stringResult,
                            };
                            var result = await _blobContext.Blobs.AddAsync(appFile);
                            await _blobContext.SaveChangesAsync();
                            blob = new { blobId = result.Entity.Id };
                        }
                        var tieneImagenes = await _context.ImagenesItems.Where(i => i.ItemId == itemId && i.Operacion != "delete" ).AnyAsync();
                        var image = new AutomatizerInterop.Data.Entities.Inventario.ImagenItem
                        {
                            BlobID = blob.blobId,
                            FechaModificacion = DateTimeOffset.Now,
                            ImageAlt = "",
                            ItemId = itemId.Value,
                            Operacion = "Add",
                            Type = tieneImagenes ? "normal" : "cover"
                        };
                        _context.ImagenesItems.Add(image);
                        await _context.SaveChangesAsync();
                        imagenesNuevas.Add(image);
                    }
                }
                return Ok(imagenesNuevas);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private static string GetStringHash(byte[] hash)
        {

            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < hash.Length; i++)
            {
                sBuilder.Append(hash[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        // DELETE: api/ImagenesItems/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<AutomatizerInterop.Data.Entities.Inventario.ImagenItem>> DeleteImagenItem(int id)
        {
            var imagenItem = await _context.ImagenesItems.FindAsync(id);
            if (imagenItem == null)
            {
                return NotFound();
            }

            imagenItem.Operacion = "delete";
            

            if (imagenItem.Type == "cover")
            {
                var newCover = _context.ImagenesItems.Where(i => i.ItemId == imagenItem.ItemId && i.Id != id).AsTracking().FirstOrDefault();
                if (!(newCover  is null) )
                {
                    newCover.Type = "cover";
                    newCover.FechaModificacion = DateTimeOffset.Now;
                }
            }

            imagenItem.FechaModificacion = DateTimeOffset.Now;
            //_context.ImagenesItems.Remove(imagenItem);
            await _context.SaveChangesAsync();

            return imagenItem;
        }

        private bool ImagenItemExists(int id)
        {
            return _context.ImagenesItems.Any(e => e.Id == id);
        }
    }
}
