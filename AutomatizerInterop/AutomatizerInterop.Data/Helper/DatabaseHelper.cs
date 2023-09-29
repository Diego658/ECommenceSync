using AutomatizerInterop.Data.Entities;
using PetaPoco;
using PetaPoco.Providers;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Helper
{
    public static class DatabaseHelper
    {
        public static IDatabase GetSqlDatabase(ConfiguracionProgramaFacturacionElectronica  configuracion)
        {
            var database = new Database(
                $"Data Source={configuracion.ServidorSQLServer};Initial Catalog={configuracion.CatalogoSqlServer};User ID=Automatizer;Password=jwptadgt158", new SqlServerDatabaseProvider());
            return database;
        }

        public static string GetSqlConnectionString(ConfiguracionProgramaFacturacionElectronica configuracion)
        {
            return $"Data Source={configuracion.ServidorSQLServer};Initial Catalog={configuracion.CatalogoSqlServer};User ID=Automatizer;Password=jwptadgt158";
        }
    }
}
