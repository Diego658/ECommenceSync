using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace AutomatizerInterop.Facturacion
{
    public class FacturacionHelper : IFacturacionHelper
    {
        string _connectionString;
        public FacturacionHelper(IConfiguration configuracion)
        {
            _connectionString = configuracion.GetConnectionString("AutomatizerSQLDB");
        }

        public DbConnection GetDbConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
