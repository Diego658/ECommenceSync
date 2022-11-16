using ECommenceSync;
using ECommenceSync.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ECommenceSyncDI
    {
        public static void AddECommenceSync(this IServiceCollection  services)
        {
            services.AddSingleton(typeof(IStoresCollection), typeof(StoresCollection) );
            services.AddSingleton(typeof(IEcommerceSyncStoresBuilder), typeof(EcommerceSyncStoresBuilder));
            services.AddSingleton(typeof(ISyncWorker), typeof(GenericSyncWorker));
        }


        public static void UseErp()
        {

        }


        public static void UseECommenceStore(this IApplicationBuilder builder, Action<IEcommerceSyncStoresBuilder> configure)
        {
            using (var serviceScope = builder.ApplicationServices.CreateScope())
            {
                var services = serviceScope.ServiceProvider;

                try
                {
                    var serviceContext = services.GetRequiredService<IEcommerceSyncStoresBuilder>();
                    configure(serviceContext);
                    // Use the context here
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<IApplicationBuilder>>();
                    logger.LogError(ex, "An error occurred.");
                }
            }
        }
        public static void RunECommenceSync(this IApplicationBuilder builder)
        {
            using var serviceScope = builder.ApplicationServices.CreateScope();
            var services = serviceScope.ServiceProvider;

            try
            {
                var erp = services.GetRequiredService<IErp>();
                erp.Start();
                var stores = services.GetRequiredService<IStoresCollection>();
                foreach (var store in stores.Stores)
                {
                    store.Start();
                }
                    
                // Use the context here
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<IApplicationBuilder>>();
                logger.LogError(ex, "An error occurred.");
            }
        }
    }
}
