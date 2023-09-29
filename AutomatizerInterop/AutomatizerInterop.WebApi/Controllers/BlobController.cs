using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutomatizerInterop.WebApi.Data;
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
    public class BlobController : ControllerBase
    {
        private readonly ILogger<BlobController> _logger;
        private readonly AppDbContext _appDbContext;

        public BlobController(ILogger<BlobController> logger, AppDbContext appDbContext)
        {
            _logger = logger;
            _appDbContext = appDbContext;
        }


        [HttpGet("GetBlobData")]
        public async Task<FileStreamResult> GetBlobData(int configuracionId, long blobId)
        {
            var blob = await _appDbContext.Blobs.FindAsync(blobId);
            if (blob == null)
            {
                return null;
            }

            return  new FileStreamResult(new MemoryStream(blob.Data), "application/octet-stream"); 
        }



    }
}