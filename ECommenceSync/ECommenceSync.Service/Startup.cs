using ECommenceSync.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ECommenceSync.Service
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
            services.AddECommenceSync();
            services.AddAutomatizerSQL();
            //services.AddPrestashop();
            services.AddStores(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<GreeterService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });

            app.UseECommenceStore(storeBuilder =>
            {

                using (var serviceScope = app.ApplicationServices.CreateScope())
                {
                    var services = serviceScope.ServiceProvider;
                    try
                    {
                        var section = Configuration.GetSection("ECommenceSync:Stores");
                        if (section.Exists())
                        {
                            var erp = services.GetRequiredService<IErp>();
                            IEnumerable<IOperation> manualOps;
                            foreach (var storeSection in section.GetChildren())
                            {
                                var enabled = storeSection.GetValue<bool>("enabled", false);
                                if (!enabled) continue;
                                switch (storeSection.Key)
                                {
                                    case "Prestashop":
                                        var prestashopStore = services.GetRequiredService<Prestashop.Prestashop>();
                                        prestashopStore.ConfigureOperations<int>();

                                        erp.ConfigureOperations<long>(prestashopStore);

                                        //Correr operaciones manuales

                                        manualOps = prestashopStore
                                            .Operations
                                            .Where(op => op.Direction == OperationDirections.StoreToStore && op.Mode == OperationModes.Manual);
                                        foreach (var mo in manualOps)
                                        {
                                            mo.Work().Wait();
                                        }
                                        break;
                                    case "WooCommerce":
                                        var wooStore = services.GetRequiredService<WooCommerce.WooCommerce>();
                                        wooStore.ConfigureOperations<int>();
                                        erp.ConfigureOperations<long>(wooStore);
                                        manualOps = wooStore
                                             .Operations
                                             .Where(op => op.Direction == OperationDirections.StoreToStore && op.Mode == OperationModes.Manual);
                                        foreach (var mo in manualOps)
                                        {
                                            mo.Work().Wait();
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }



                    }
                    catch (Exception ex)
                    {
                        var logger = services.GetRequiredService<ILogger<Startup>>();
                        logger.LogError(ex, "An error occurred.");
                    }
                }
            });
            app.RunECommenceSync();
        }
    }
}
