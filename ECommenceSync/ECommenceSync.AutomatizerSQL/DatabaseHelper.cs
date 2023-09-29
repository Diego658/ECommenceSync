using ECommenceSync.Interfaces;
using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;

namespace ECommenceSync.AutomatizerSQL
{
    public class DatabaseHelper : IDatabaseHelper<SqlConnection, int>
    {
        readonly string _connectionString;
        readonly string _codigoEmpresa;
        public int TimeToRetryPosponed { get; }


        public DatabaseHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("AutomatizerSQLDB");
            var automatizerSection = configuration.GetSection("ECommenceSync").GetSection("Erps").GetSection("AutomatizerSQL");
            if (!automatizerSection.Exists()) throw new InvalidOperationException("No se encuentra la seccion de configuracion AutomatizerSQL");
            TimeToRetryPosponed = int.Parse(automatizerSection["TimeToRetryPosponed"]);
            _codigoEmpresa = automatizerSection["CodigoEmpresa"];
        }

        public Task<ConcurrentDictionary<TExternalKey, int>> GetBrandsLinks<TExternalKey>() where TExternalKey : struct
        {
            throw new NotImplementedException();
        }

        public Task<ConcurrentDictionary<TExternalKey, int>> GetCategoryLinks<TExternalKey>() where TExternalKey : struct
        {
            throw new NotImplementedException();
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public Task<ConcurrentDictionary<TExternalKey, int>> GetProductsLinks<TExternalKey>() where TExternalKey : struct
        {
            return Task.FromResult(new ConcurrentDictionary<TExternalKey, int>());
        }

        public Task<ConcurrentDictionary<TExternalKey, int>> GetTagsLinks<TExternalKey>() where TExternalKey : struct
        {
            throw new NotImplementedException();
        }

        private System.Collections.IDictionary _customersLinks;
        private readonly object _customersLock = new();
        public async Task<ConcurrentDictionary<TExternalKey, int>> GetCustomersLinks<TExternalKey>() where TExternalKey : struct
        {
            if (_customersLinks is null)
            {
                await Task.Delay(500);
                lock (_customersLock)
                {
                    if (_customersLinks is { }) return (ConcurrentDictionary<TExternalKey, int>) _customersLinks;
                    using var conex = GetConnection();
                    using var cmd = conex.CreateCommand();
                    cmd.CommandText = "SELECT Id, PrestashopId FROM StoreSync_Customers_Prestashop WHERE(EmpCod = @EmpCod)";
                    cmd.Parameters.AddWithValue("EmpCod", _codigoEmpresa);
                    conex.Open();
                    using var reader = cmd.ExecuteReader();
                    var links = new ConcurrentDictionary<TExternalKey, int>();
                    while (reader.Read())
                    {
                        links.TryAdd(reader.GetFieldValue<TExternalKey>(1), reader.GetInt32(0));

                    }
                    _customersLinks = links;
                }
            }
            
            return (ConcurrentDictionary<TExternalKey, int>)_customersLinks;
        }
    }
}
