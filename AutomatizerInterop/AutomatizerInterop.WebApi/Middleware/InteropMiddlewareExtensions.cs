using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutomatizerInterop.WebApi.Middleware
{
    public static class InteropMiddlewareExtensions
    {
        public static IApplicationBuilder UseAutomatizerInterop(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<InteropMiddleware>();
        }
    }
}
