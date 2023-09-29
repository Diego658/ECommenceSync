using AutomatizerInterop.Data.EntityFramewrok;
using AutomatizerInterop.Data.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutomatizerInterop.WebApi.Middleware
{
    public class InteropMiddleware
    {
        private readonly RequestDelegate _next;
        
        public InteropMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IRepository genericRepository, AutomatizerInteropDbContext interopDbContext)
        {
            var configuracionQuery = context.Request.Query["configuracionId"];
            if (!string.IsNullOrWhiteSpace(configuracionQuery))
            {
                
                genericRepository.ConfiguracionID = int.Parse(configuracionQuery);
                interopDbContext.ConfiguracionId = int.Parse(configuracionQuery);
            }
            else
            {
                var configuracionHeader = context.Request.Headers["ConfiguracionId"];
                if (configuracionHeader.Count>0 )
                {
                    genericRepository.ConfiguracionID = int.Parse(configuracionHeader[0]);
                    interopDbContext.ConfiguracionId = int.Parse(configuracionHeader[0]);
                }
            }

            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }
    }
}
