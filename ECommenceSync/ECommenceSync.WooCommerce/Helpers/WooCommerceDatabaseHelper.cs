using ECommenceSync.WooCommerce.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Threading.Tasks;

namespace ECommenceSync.WooCommerce.Helpers
{
    public class WooCommerceDatabaseHelper : IWooCommerceDatabaseHelper
    {
        private readonly int _timeToRetryPosponed;
        private readonly string _connectionString;
        private IDictionary _categoryLinks;
        private IDictionary _ProductLinks;
        private IDictionary _brandLinks;
        private IDictionary _tagsLinks;
        private IDictionary _attributesLinks;
        private IDictionary _attributesTermsLinks;
        private IDictionary _productsVariationsLinks;
        private IDictionary _productsVariationsVsVariantsLinks;

        private readonly object lockCategory = new();
        private readonly object lockProducts = new();
        private readonly object lockBrands = new();
        private readonly object lockTags = new();
        private readonly object lockAttributes = new();
        private readonly object lockAttributesTerms = new();
        private readonly object lockProductsVariationsLinks = new();
        private readonly object lockProductsVariationsVsVariatsLinks = new();

        public string ApiUrl { get; set; }
        public string ApiUser { get; set; }
        public string ApiPassword { get; set; }

        public int TimeToRetryPosponed => _timeToRetryPosponed;

        private string _apiUrlWordpress;
        public string ApiUrlWordpress => _apiUrlWordpress;
        private string _apiWpAppPwd;
        public string ApiWpAppPwd => _apiWpAppPwd ?? ApiPassword;
        private string _apiWpAppUser;
        public string ApiWpAppUser => _apiWpAppUser;

        public WooCommerceDatabaseHelper(IConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            _connectionString = configuration.GetConnectionString("AutomatizerSQLDB");
            var wooSection = configuration.GetSection("ECommenceSync").GetSection("Stores").GetSection("WooCommerce");
            if (!wooSection.Exists()) throw new InvalidOperationException("No se encuentra la seccion de configuracion WooCommerce");
            ApiUrl = wooSection["ApiUrl"] ?? throw new ArgumentNullException(nameof(ApiUrl));
            _apiUrlWordpress = wooSection[nameof(ApiUrlWordpress)] ?? throw new ArgumentNullException(nameof(ApiUrlWordpress));
            _apiWpAppPwd = wooSection[nameof(ApiWpAppPwd)] ?? throw new ArgumentNullException(nameof(ApiWpAppPwd));
            _apiWpAppUser = wooSection[nameof(ApiWpAppUser)] ?? throw new ArgumentNullException(nameof(ApiWpAppUser));
            ApiUser = wooSection["ApiKey"] ?? throw new ArgumentNullException("ApiKey");
            ApiPassword = wooSection["ApiSecret"] ?? throw new ArgumentNullException("ApiSecret");
            _timeToRetryPosponed = int.Parse(wooSection["TimeToRetryPosponed"]);
        }

        public async Task<ConcurrentDictionary<TExternalKey, long>> GetBrandsLinks<TExternalKey>() where TExternalKey : struct
        {
            if (_brandLinks is null)
            {
                await Task.Delay(200);
                lock (lockBrands)
                {
                    if(_brandLinks is null)
                    {
                        var links = new ConcurrentDictionary<TExternalKey, long>();
                        using var sqlConex = GetConnection();
                        sqlConex.Open();
                        using var command = (SqlCommand)sqlConex.CreateCommand();
                        command.CommandText = "SELECT MarcaId, WooCommerceID FROM StoreSync_Marcas_WooCommerce";
                        using var reader = command.ExecuteReader(); //sqlConex.ExecuteReaderAsync("");
                        while (reader.Read())
                        {
                            links.TryAdd(reader.GetFieldValue<TExternalKey>(0), reader.GetInt64(1));
                        }
                        _brandLinks = links;
                    }
                }
            }
            return (ConcurrentDictionary<TExternalKey, long>)_brandLinks;
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
                        command.CommandText = "SELECT CategoriaID, WooCommerceID FROM StoreSync_Categorias_WooCommerce";
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

        public DbConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
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
                        command.CommandText = "SELECT ItemID, WooCommerceID FROM StoreSync_Productos_WooCommerce";
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

        public async Task<ConcurrentDictionary<TExternalKey, long>> GetTagsLinks<TExternalKey>() where TExternalKey : struct
        {
            if (_tagsLinks is null)
            {
                await Task.Delay(50);
                lock (lockTags)
                {
                    if (_tagsLinks != null) return (ConcurrentDictionary<TExternalKey, long>)_tagsLinks;
                    var links = new ConcurrentDictionary<TExternalKey, long>();
                    using var sqlConex = GetConnection();
                    sqlConex.Open();
                    using var command = (SqlCommand)sqlConex.CreateCommand();
                    command.CommandText = "SELECT TagId, WooCommerceID FROM StoreSync_Tags_WooCommerce";
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

        public async Task<ConcurrentDictionary<TExternalKey, long>> GetAttributesLinks<TExternalKey>() where TExternalKey : struct
        {
            if (_attributesLinks is null)
            {
                await Task.Delay(50);
                lock (lockAttributes)
                {
                    if (_attributesLinks != null) return (ConcurrentDictionary<TExternalKey, long>)this._attributesLinks;
                    using var sqlConex = GetConnection();
                    sqlConex.Open();
                    using var cmd2 = sqlConex.CreateCommand();
                    cmd2.CommandText = "SELECT AttributeId, WooCommerceID FROM StoreSync_ProductsAttributes_WooCommerce";
                    //await sqlConex.OpenAsync();
                    using var reader2 = cmd2.ExecuteReader();
                    var attributesLinks = new ConcurrentDictionary<TExternalKey, long>();
                    while (reader2.Read())
                    {
                        attributesLinks.TryAdd(reader2.GetFieldValue<TExternalKey>(0), reader2.GetInt64(1));
                    }
                    this._attributesLinks = attributesLinks;
                }
            }
            return (ConcurrentDictionary<TExternalKey, long>)_attributesLinks;
        }

        public async Task<ConcurrentDictionary<TExternalKey, long>> GetAttributesTermsLinks<TExternalKey>() where TExternalKey : struct
        {
            if (_attributesTermsLinks is null)
            {
                await Task.Delay(50);
                lock (lockAttributesTerms)
                {
                    if (_attributesTermsLinks != null) return (ConcurrentDictionary<TExternalKey, long>)_attributesTermsLinks;
                    using var sqlConex = GetConnection();
                    using var cmd = sqlConex.CreateCommand();
                    cmd.CommandText = "SELECT AttributeTermId, WooCommerceID FROM StoreSync_ProductsAttributesTerms_WooCommerce";
                    sqlConex.Open();
                    using var reader = cmd.ExecuteReader();
                    var localAttributesTermsLinks = new ConcurrentDictionary<TExternalKey, long>();
                    while (reader.Read())
                    {
                        localAttributesTermsLinks.TryAdd(reader.GetFieldValue<TExternalKey>(0), reader.GetInt64(1));
                    }
                    reader.Close();
                    this._attributesTermsLinks = localAttributesTermsLinks;
                }
            }
            return (ConcurrentDictionary<TExternalKey, long>)_attributesTermsLinks;
        }

        public async Task<ConcurrentDictionary<TExternalKey, long>> GetProductsVariationsLinks<TExternalKey>() where TExternalKey : struct
        {
            if (_productsVariationsLinks is null)
            {
                await Task.Delay(50);
                lock (lockProductsVariationsLinks)
                {
                    if (_productsVariationsLinks != null) return (ConcurrentDictionary<TExternalKey, long>)_productsVariationsLinks;
                    using var sqlConex = GetConnection();
                    using var cmd = sqlConex.CreateCommand();
                    cmd.CommandText = "SELECT VariationID, WooCommerceID FROM StoreSync_Variations_WooCommerce";
                    sqlConex.Open();
                    using var reader = cmd.ExecuteReader();
                    var localProductsVariations = new ConcurrentDictionary<TExternalKey, long>();
                    while (reader.Read())
                    {
                        localProductsVariations.TryAdd(reader.GetFieldValue<TExternalKey>(0), reader.GetInt64(1));
                    }
                    reader.Close();
                    _productsVariationsLinks = localProductsVariations;
                }
            }
            return (ConcurrentDictionary<TExternalKey, long>)_productsVariationsLinks;
        }

        public async Task<ConcurrentDictionary<TExternalKey, WooProductVariationVsVariants<TExternalKey>>> GetProductVariationsVsVariants<TExternalKey>() where TExternalKey : struct
        {

            if (_productsVariationsVsVariantsLinks is null)
            {
                await Task.Delay(50);
                lock (lockProductsVariationsVsVariatsLinks)
                {
                    if (_productsVariationsVsVariantsLinks != null) return (ConcurrentDictionary<TExternalKey, WooProductVariationVsVariants<TExternalKey>>)_productsVariationsVsVariantsLinks;
                    using var sqlConex = GetConnection();
                    using var cmd = sqlConex.CreateCommand();
                    cmd.CommandText = "SELECT WooProductId, WooVariationId, ExternalId FROM StoreSync_VariationsVsVariants_WooCommerce";
                    sqlConex.Open();
                    using var reader = cmd.ExecuteReader();
                    var localProductsVariationsVsVariantsLinks = new ConcurrentDictionary<TExternalKey, WooProductVariationVsVariants<TExternalKey>>();
                    while (reader.Read())
                    {
                        localProductsVariationsVsVariantsLinks.TryAdd(reader.GetFieldValue<TExternalKey>(2), new() 
                        {
                            WooProductId = reader.GetInt64(0),
                            WooVariationId = reader.GetInt64(1),
                            ExternalId = reader.GetFieldValue<TExternalKey>(2)
                        } );
                    }
                    reader.Close();
                    _productsVariationsVsVariantsLinks = localProductsVariationsVsVariantsLinks;
                }
            }
            return (ConcurrentDictionary<TExternalKey, WooProductVariationVsVariants<TExternalKey>>)_productsVariationsVsVariantsLinks;
        }
    }
}
