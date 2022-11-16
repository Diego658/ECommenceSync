using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace ECommenceSync.Prestashop.Helpers
{
    public class PrestashopDatabaseHelper : IPrestashopDatabaseHelper
    {
        private IDictionary _categoryLinks;
        private IDictionary _ProductLinks;
        private IDictionary _brandLinks;
        private IDictionary _tagsLinks;

        private readonly object lockCategory = new object();
        private readonly object lockProducts = new object();
        private readonly object lockBrands = new object();
        private readonly object lockTags = new object();
        private readonly string _connectionString;

        public string ApiUrl { get; set; }
        public string ApiSecret { get; set; }

        public int SyncLanguage { get; }
        public int TaxRuleGroup { get; }

        public int TimeToRetryPosponed { get; }

        public PrestashopDatabaseHelper(IConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _connectionString = configuration.GetConnectionString("AutomatizerSQLDB");
            var prestashopSection = configuration.GetSection("ECommenceSync").GetSection("Stores").GetSection("Prestashop");
            if (!prestashopSection.Exists()) throw new InvalidOperationException("No se encuentra la seccion de configuracion Prestashop");
            ApiUrl = prestashopSection["ApiUrl"] ?? throw new ArgumentNullException(nameof(ApiUrl));
            ApiSecret = prestashopSection["ApiSecret"] ?? throw new ArgumentNullException(nameof(ApiSecret));
            SyncLanguage = int.Parse(prestashopSection["SyncLanguage"]);
            TaxRuleGroup = int.Parse(prestashopSection["TaxRuleGroup"]);
            TimeToRetryPosponed = int.Parse(prestashopSection["TimeToRetryPosponed"]);
        }



        public DbConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }


        public async Task<ConcurrentDictionary<TExternalKey, long>> GetCategoryLinks<TExternalKey>() where TExternalKey : struct
        {
            if (_categoryLinks is null)
            {
                await Task.Delay(200);
                lock (lockCategory)
                {
                    if (_categoryLinks is null)
                    {
                        var links = new ConcurrentDictionary<TExternalKey, long>();
                        using var sqlConex = GetConnection();
                        sqlConex.Open();
                        using var command = (SqlCommand)sqlConex.CreateCommand();
                        command.CommandText = "SELECT CategoriaID, PrestashopID FROM StoreSync_Categorias_Prestashop";
                        using var reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            links.TryAdd(reader.GetFieldValue<TExternalKey>(0), reader.GetInt64(1));
                        }
                        _categoryLinks = links;
                    }
                }
            }
            return (ConcurrentDictionary<TExternalKey, long>)_categoryLinks;
        }



        public async Task<ConcurrentDictionary<TExternalKey, long>> GetProductsLinks<TExternalKey>() where TExternalKey : struct
        {
            if (_ProductLinks is null)
            {
                await Task.Delay(200);
                lock (lockProducts)
                {
                    if (_ProductLinks is null)
                    {
                        var links = new ConcurrentDictionary<TExternalKey, long>();
                        using var sqlConex = GetConnection();
                        sqlConex.Open();
                        using var command = (SqlCommand)sqlConex.CreateCommand();
                        command.CommandText = "SELECT ItemID, PrestashopID FROM StoreSync_Productos_Prestashop";
                        using var reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            links.TryAdd(reader.GetFieldValue<TExternalKey>(0), reader.GetInt64(1));
                        }
                        _ProductLinks = links;
                    }
                }
            }
            return (ConcurrentDictionary<TExternalKey, long>)_ProductLinks;
        }

        public async Task<ConcurrentDictionary<TExternalKey, long>> GetBrandsLinks<TExternalKey>() where TExternalKey : struct
        {
            if (_brandLinks is null)
            {
                await Task.Delay(200);
                lock (lockBrands)
                {
                    var links = new ConcurrentDictionary<TExternalKey, long>();
                    using var sqlConex = GetConnection();
                    sqlConex.Open();
                    using var command = (SqlCommand)sqlConex.CreateCommand();
                    command.CommandText = "SELECT MarcaId, PrestashopID FROM StoreSync_Marcas_Prestashop";
                    using var reader = command.ExecuteReader(); //sqlConex.ExecuteReaderAsync("");
                    while (reader.Read())
                    {
                        links.TryAdd(reader.GetFieldValue<TExternalKey>(0), reader.GetInt64(1));
                    }
                    _brandLinks = links;
                }
            }
            return (ConcurrentDictionary<TExternalKey, long>)_brandLinks;
        }

        public async Task<ConcurrentDictionary<TExternalKey, long>> GetTagsLinks<TExternalKey>() where TExternalKey : struct
        {
            if (_tagsLinks is null)
            {
                await Task.Delay(50);
                lock (lockTags)
                {
                    var links = new ConcurrentDictionary<TExternalKey, long>();
                    using var sqlConex = GetConnection();
                    sqlConex.Open();
                    using var command = (SqlCommand)sqlConex.CreateCommand();
                    command.CommandText = "SELECT TagId, PrestashopId FROM StoreSync_Tags_Prestashop";
                    using var reader = command.ExecuteReader(); //sqlConex.ExecuteReaderAsync("");
                    while (reader.Read())
                    {
                        links.TryAdd(reader.GetFieldValue<TExternalKey>(0), reader.GetInt64(1));
                    }
                    _tagsLinks = links;
                }
            }
            return (ConcurrentDictionary<TExternalKey, long>)_tagsLinks;
        }



    }
}
