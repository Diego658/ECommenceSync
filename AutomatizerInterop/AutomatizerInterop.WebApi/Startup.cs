using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PetaPoco;
using Microsoft.Extensions.Options;
using PetaPoco.Providers;
using AutomatizerInterop.Data.Interfaces;
using AutomatizerInterop.Data.Repositories;
using AutomatizerInterop.WebApi.Middleware;
using AutomatizerInterop.WebApi.Models;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using AutomatizerInterop.WebApi.Services;
using AutomatizerInterop.WebApi.Data;
using Microsoft.EntityFrameworkCore;
using AutomatizerInterop.Data.EntityFramewrok;
using AutoMapper;
using AutomatizerInterop.Data.Entities.Inventario;

namespace AutomatizerInterop.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddCors(c =>
            {
                c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });


            var assembly = System.Reflection.Assembly.GetAssembly(typeof(Atributo));
            services.AddAutoMapper( assembly);

            services.AddControllers();


            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });


            services.AddScoped<IUserService, UserService>();


            var mysqlConStr = Configuration.GetConnectionString("facturacionElectronicaMysql");
            services.AddScoped<IDatabase>(provider => { return new Database(mysqlConStr, new MySqlDatabaseProvider()); });
            services.AddSingleton(typeof(IInteropConfiguracionProvider), new InteropConfiguracionProvider(new Database(mysqlConStr, new MySqlDatabaseProvider())));
            services.AddScoped(typeof(IUsuariosRepository), typeof(UsaPrimeraConfiguracionUsuariosRepository));


            services.AddScoped(typeof(IRepository), provider => new GenericRepository());

            services.AddScoped(typeof(IFacturacionElectronicaRepository), typeof(FacturacionElectronicaRepository));
            services.AddScoped(typeof(IAntivirusRepository), typeof(AntivirusRepository));
            services.AddScoped(typeof(IClientesRepository), typeof(ClientesRepository));
            services.AddScoped(typeof(IInventarioRepository), typeof(InventarioRepository));
            services.AddScoped(typeof(IStoreSyncRepository), typeof(StoreSyncRepository));
            services.AddScoped(typeof(IBodegaRepository), typeof(BodegaRepository));
            services.AddScoped(typeof(IFacturacionRepository), typeof(FacturacionRepository));
            services.AddScoped(typeof(ITransaccionesRepository), typeof(TransaccionesRepository));
            services.AddSingleton(typeof(IPrestashopHelper), typeof( PrestashopHelper));
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString( "blobsDb")));
            services.AddDbContext<SearchDbContext>();
            services.AddDbContext<AutomatizerInteropDbContext>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
#pragma warning disable CA1822 // Mark members as static
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
#pragma warning restore CA1822 // Mark members as static
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAutomatizerInterop();

            app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());


            //app.UseHttpsRedirection();

            app.UseCors("AllowOrigin");

            app.UseAuthentication();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                
            });

        }
    }
}
