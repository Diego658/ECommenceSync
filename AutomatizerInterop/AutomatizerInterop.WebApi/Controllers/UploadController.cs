using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
//using System.IO.Pipelines;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutomatizerInterop.Data.Entities;
using AutomatizerInterop.Data.Interfaces;
using AutomatizerInterop.WebApi.Data;
using AutomatizerInterop.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PetaPoco;


namespace AutomatizerInterop.WebApi.Controllers
{

    
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly ILogger<UploadController> _logger;
        private readonly AppDbContext _context;
        private readonly IInventarioRepository _inventarioRepository;
        private readonly int _fileSizeLimit;

        public UploadController(ILogger<UploadController> logger, AppDbContext context, IInventarioRepository inventarioRepository, IConfiguration config)
        {
            _logger = logger;
            _context = context;
            _inventarioRepository = inventarioRepository;
            _fileSizeLimit = config.GetValue<int>("FileSizeLimit");

        }

        //[HttpPost("UploadStream"), DisableRequestSizeLimit]
        //public async Task<ActionResult<dynamic>> UploadStream(int itemID)
        //{

        //    try
        //    {

        //        var reader = Request.BodyReader;
        //        using (var stream = new MemoryStream())
        //        {
        //            while (true)
        //            {
        //                ReadResult result = await reader.ReadAsync();
        //                ReadOnlySequence<byte> buffer = result.Buffer;

        //                reader.AdvanceTo(buffer.Start, buffer.End);
        //                var data = buffer.ToArray();
        //                stream.Write(data ,0, Convert.ToInt32( buffer.Length) );
        //                if (result.IsCompleted)
        //                {
        //                    break;
        //                }

        //            }
        //            reader.Complete();



                    
        //            var appFile = new Blob
        //            {
        //                Data = stream.ToArray(),
        //                UploadDT = DateTime.UtcNow
        //            };

        //            var dbBlob = await _context.Blobs.AddAsync(appFile);

        //            await _context.SaveChangesAsync();

        //            return Ok(new { blobId = dbBlob.Entity.Id });

        //        }
                
                

        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Internal server error: {ex}");
        //    }
        //}


        [HttpPost("Upload"), DisableRequestSizeLimit]
        public async Task<ActionResult<dynamic>> Upload(int itemID, string type="normal")
        {
            if (itemID == 0)
            {
                return BadRequest();
            }
            
            try
            {
                List<object> blobIds = new List<object>();
                foreach (var file in Request.Form.Files)
                {
                    if (file.Length > 0 && itemID >0)
                    {
                        var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');


                        using (var stream = new MemoryStream(_fileSizeLimit))
                        {
                            await file.CopyToAsync(stream);

                            var bufferBytes = stream.ToArray();
                            byte[] hashResult;
                            var shaM = new SHA512Managed();
                            hashResult = shaM.ComputeHash(bufferBytes);
                            var stringResult = GetStringHash(hashResult);

                            var blob = _context.Blobs.Where(b => b.HashString == stringResult).Select(b => new { blobId = b.Id }).FirstOrDefault();
                            if (blob != null)
                            {
                                var image = await _inventarioRepository.AddImagenItem(itemID, type, blob.blobId);
                                blobIds.Add(image);
                            }
                            else
                            {
                                var appFile = new Blob
                                {
                                    Data = stream.ToArray(),
                                    UploadDT = DateTime.UtcNow,
                                    Extension = fileName.Substring(fileName.LastIndexOf('.'), fileName.Length - fileName.LastIndexOf('.')),
                                    HashString = stringResult,
                                };
                                var result = await _context.Blobs.AddAsync(appFile);
                                await _context.SaveChangesAsync();
                                var image = await _inventarioRepository.AddImagenItem(itemID, type, result.Entity.Id);
                                blobIds.Add(image);
                            }

                        }
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                return blobIds;

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
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


    }


   

}
