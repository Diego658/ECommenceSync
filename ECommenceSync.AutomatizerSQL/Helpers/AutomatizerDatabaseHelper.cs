using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ECommenceSync.AutomatizerSQL.Helpers
{

    public interface IAutomatizerDataHelper
    {
        IDbConnection GetConnection();
        SqlConnection GetBlobConnection();
        public string CodigoEmpresa { get; set; }
        public int TipoPrecioPaginaWeb { get; set; }
        string CodigoBodega { get; }
    }

    public class AutomatizerDatabaseHelper : IAutomatizerDataHelper
    {
        private readonly string _connectionString;
        readonly string _blobConnectionString;

        public string CodigoEmpresa { get; set; }
        public int TipoPrecioPaginaWeb { get; set; }

        public string CodigoBodega { get; }

        public AutomatizerDatabaseHelper()
        {
            
        }

        public AutomatizerDatabaseHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("AutomatizerSQLDB");
            _blobConnectionString = configuration.GetConnectionString("AutomatizerSQLBlobDB");
            var automatizerSection = configuration.GetSection("ECommenceSync").GetSection("Erps").GetSection("AutomatizerSQL");
            CodigoEmpresa = automatizerSection["CodigoEmpresa"];
            TipoPrecioPaginaWeb = int.Parse( automatizerSection["TipoClientePaginaWeb"]);
            CodigoBodega = automatizerSection["CodigoBodega"];
        }

        public IDbConnection GetConnection()
        {
            return new SqlConnection(_connectionString );
        }

        public SqlConnection GetBlobConnection()
        {
            return new SqlConnection(_blobConnectionString);
        }
    }
}
