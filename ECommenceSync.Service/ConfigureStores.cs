using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ConfigureStores
    {
        public static void AddStores(this IServiceCollection services, IConfiguration configuration)
        {
            var section = configuration.GetSection("ECommenceSync:Stores");
            if (section.Exists())
            {
                foreach (var storeSection in section.GetChildren())
                {
                    var enabled = storeSection.GetValue<bool>("enabled",false);
                    if (!enabled) continue;
                    switch (storeSection.Key)
                    {
                        case "Prestashop":
                            services.AddPrestashop();
                            break;
                        case "WooCommerce":
                            services.AddWooCommerce();
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
