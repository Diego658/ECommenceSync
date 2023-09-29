using AutomatizerInterop.Data.Entities.Cliente;
using AutomatizerInterop.Data.Entities.Facturacion;
using AutomatizerInterop.Data.Entities.GuiasTransporte;
using AutomatizerInterop.Data.Entities.Inventario;
using AutomatizerInterop.Data.Entities.Kardex;
using AutomatizerInterop.Data.Entities.Stores.Prestashop;
using AutomatizerInterop.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AutomatizerInterop.Data.EntityFramewrok
{


    public class AutomatizerInteropDbContext : DbContext
    {
        private readonly IInteropConfiguracionProvider _configuracionProvider;

        public int ConfiguracionId { get; set; }

        public DbSet<Atributo> Atributos { get; set; }
        public DbSet<AtributoValor> AtributoValores { get; set; }
        public DbSet<ImagenItem> ImagenesItems { get; set; }
        public DbSet<Item> Items { get; set; }

        public DbSet<FacCab> FacCab { get; set; }
        public DbSet<FacDet> FacDet { get; set; }
        public DbSet<FacDetSer> FacDetSer { get; set; }

        public DbSet<FacDetSerComp> FacDetSerComp { get; set; }
        public DbSet<FacRubDet> FacRubDet { get; set; }
        public DbSet<FacFue> FacFue { get; set; }

        public DbSet<KardexFac> KardexFac { get; set; }
        public DbSet<KardexCli> KardexCli { get; set; }

        public DbSet<KarDet> KarDet { get; set; }

        public DbSet<CambioPendiente> PrestashopCambiosPendientes { get; set; }

        public DbSet<GuiaTransporte> GuiasTransporte { get; set; }

        public DbSet<CompaniasTransporte> CompaniasTransportes { get; set; }

        public DbSet<GuiasTransporteDetalle> GuiasTransporteDetalles { get; set; }

        public DbSet<Climae> Clientes { get; set; }

        public DbSet<PedidoPrestashop> PedidosPrestashop { get; set; }

        public AutomatizerInteropDbContext(IInteropConfiguracionProvider configuracionProvider)
        {
            _configuracionProvider = configuracionProvider;
        }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            if (ConfiguracionId == 0)
            {
                ConfiguracionId = 1;
                //throw new InvalidOperationException("ConfiguracionId ");
            }


            var configuracion = _configuracionProvider.GetConfiguracion(ConfiguracionId);
            optionsBuilder.UseSqlServer($"Data Source={configuracion.ServidorSQLServer};Initial Catalog={configuracion.CatalogoSqlServer};User ID=Automatizer;Password=jwptadgt158");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Item>()
                .HasKey(table => new { table.Secuencial });

            modelBuilder.Entity<Item>()
                .HasIndex(table => new
                {
                    table.EmpCod,
                    table.ItmCod,
                    table.ItmTip
                });

            modelBuilder.Entity<FacCab>().HasKey(table => new
            {
                table.EmpCod,
                table.SucCod,
                table.TrnCod,
                table.TrnNum
            });

            modelBuilder.Entity<FacDet>().HasKey(table => new
            {
                table.EmpCod,
                table.SucCod,
                table.TrnCod,
                table.TrnNum,
                table.ItmCod,
                table.ISec
            });


            modelBuilder.Entity<FacDetSer>().HasKey(table => new
            {
                table.EmpCod,
                table.SucCod,
                table.TrnCod,
                table.TrnNum,
                table.SerNum,
                table.SSec
            });

            modelBuilder.Entity<FacDetSerComp>().HasKey(table => new
            {
                table.EmpCod,
                table.SucCod,
                table.TrnCod,
                table.TrnNum,
                table.SerNum,
                table.SSec,
                table.ItmCod,
                table.BodCod
            });

            modelBuilder.Entity<FacFue>().HasKey(table => new
            {
                table.EmpCod,
                table.SucCod,
                table.TrnCod,
                table.TrnNum,
                table.TipoF,
                table.FCod,
                table.FNum

            });


            modelBuilder.Entity<FacRubDet>().HasKey(table => new
            {
                table.EmpCod,
                table.SucCod,
                table.TrnCod,
                table.TrnNum,
                table.ItmCod,
                table.ISec,
                table.RubNum
            });

            modelBuilder.Entity<KardexFac>().HasKey(table => new
            {
                table.EmpCod,
                table.FacCod,
                table.FacNum,
                table.NumPago,
                table.ModIde,
                table.IngCod,
                table.IngNum
            });



            modelBuilder.Entity<CambioPendiente>().HasKey(table => new
            {
                table.Destino,
                table.ClaveExterna
            });

            modelBuilder.Entity<CompaniasTransporte>().ToTable<CompaniasTransporte>("Guias_CompaniasTransporte");
            modelBuilder.Entity<CompaniasTransporte>().HasKey(table => new
            {
                table.CompaniaID
            });

            modelBuilder.Entity<Climae>().HasKey(table => new
            {
                table.CliSec
            });

            modelBuilder.Entity<GuiaTransporte>().ToTable("Guias_GuiasTransporte")
                .HasKey(table => new
                {
                    table.Id
                });

            modelBuilder.Entity<GuiasTransporteDetalle>().ToTable("Guias_GuiasTransporteDetalle")
                .HasKey(table => new
                {
                    table.EmpCod,
                    table.Id,
                    table.TrnCod,
                    table.TrnNum
                });



        }


    }
}
