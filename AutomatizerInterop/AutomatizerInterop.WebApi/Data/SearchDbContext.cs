using AutomatizerInterop.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutomatizerInterop.WebApi.Data
{
    public class SearchDbContext : DbContext
    {
        private readonly IInteropConfiguracionProvider configuracionProvider;

        public SearchDbContext(IInteropConfiguracionProvider configuracionProvider)
        {
            this.configuracionProvider = configuracionProvider;
        }

        
        public DbSet<Models.Treeview.Item> ArbolInventario { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuracion = configuracionProvider.GetConfiguracion(1);
            optionsBuilder.UseSqlServer($"Data Source={configuracion.ServidorSQLServer};Initial Catalog={configuracion.CatalogoSqlServer};User ID=Automatizer;Password=jwptadgt158;Encrypt=False");
        }
    }
}
