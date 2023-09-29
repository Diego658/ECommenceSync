using ECommenceSync.AutomatizerSQL.Helpers;
using ECommenceSync.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ECommenceSync.AutomatizerSQL.Operations
{
    public class ProductCategoryOperation : AutomatizerSQLOperation<ProductCategory<int>>
    {
        readonly int _timeToSleep;
        Dictionary<string, int> _updatedColsSchema;
        public override ECommenceSync.Operations Operation => ECommenceSync.Operations.ProductsCategories;

        public override OperationModes Mode => OperationModes.Automatic;

        public override OperationDirections Direction => OperationDirections.ErpToStore;

        public override Guid Identifier => Guid.NewGuid();

        private OperationStatus _status;
        public override OperationStatus Status { get => _status; }


        public ProductCategoryOperation(IAutomatizerDataHelper dataHelper, IAutomatizerSQLOperationsHelper operationsHelper)
        {
            DataHelper = dataHelper;
            OperationHelper = operationsHelper;
            _timeToSleep = OperationHelper.GetSearchTime(Operation);
            _status = OperationStatus.Created;
        }

        private void ConfigureColsSchema(SqlDataReader reader)
        {
            var schema = reader.GetColumnSchema();
            _updatedColsSchema = new Dictionary<string, int>(10);
            foreach (var col in schema)
            {
                _updatedColsSchema.Add(col.ColumnName, col.ColumnOrdinal.Value);
            }
        }


        public override async Task<List<ProductCategory<int>>> GetUpdated()
        {
            using var sqlConex = (SqlConnection)DataHelper.GetConnection();
            using var cmd = sqlConex.CreateCommand();
            cmd.CommandText = "SELECT  * FROM dbo.StoreSync_GetCategoriasActualizadas(@CodigoEmpresa, @Fecha) ORDER BY Code";
            cmd.Parameters.AddWithValue("CodigoEmpresa", DataHelper.CodigoEmpresa);
            cmd.Parameters.AddWithValue("Fecha", SyncTimeInfo.LastSyncTime);
            await sqlConex.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (_updatedColsSchema is null)
            {
                ConfigureColsSchema(reader);
            }
            var categories = await CargarCategorias(reader);
            await sqlConex.CloseAsync();
            return categories;
        }

        private async Task<List<ProductCategory<int>>> CargarCategorias(SqlDataReader reader)
        {
            var categories = new List<ProductCategory<int>>();

            while (await reader.ReadAsync())
            {
                var category = new ProductCategory<int>
                {
                    Id = reader.GetInt32(_updatedColsSchema["CategoryId"]),
                    ParentId = reader.GetSqlInt32(_updatedColsSchema["ParentId"]).IsNull ? default(int?) : reader.GetInt32(_updatedColsSchema["ParentId"]),
                    Code = reader.GetString(_updatedColsSchema["Code"]),
                    Name = reader.GetString(_updatedColsSchema["Name"]),
                    Active = reader.GetBoolean(_updatedColsSchema["Active"]),
                    IsRootCategory = reader.GetBoolean(_updatedColsSchema["IsRootCategory"]),
                    Position = reader.GetInt16(_updatedColsSchema["Position"]),
                    Description = reader.GetString(_updatedColsSchema["Description"]),
                    DescriptionShort = reader.GetString(_updatedColsSchema["DescriptionShort"]),
                    Selected = Convert.ToBoolean( reader.GetInt32(_updatedColsSchema["Selected"]))
                };

                categories.Add(category);
            }

            return categories;
        }

        public override async Task<List<ProductCategory<int>>> ResolveEntities(List<int> keys)
        {
            using var sqlConex = (SqlConnection)DataHelper.GetConnection();
            using var cmd = sqlConex.CreateCommand();
            cmd.CommandText = "SELECT  * FROM dbo.StoreSync_GetCategoriasByKeys(@Keys) ORDER BY Code";
            cmd.Parameters.AddWithValue("@Keys", string.Join(',', keys.Select(k => k.ToString())));
            await sqlConex.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (_updatedColsSchema is null)
            {
                ConfigureColsSchema(reader);
            }
            var categories = await CargarCategorias(reader);
            await sqlConex.CloseAsync();
            return categories;
        }


        public override async Task Work()
        {
            if (Processors.Count == 0)
            {
                _status = OperationStatus.Stopped;
                return;
            }

            _status = OperationStatus.Working;
            await Sleep(_timeToSleep);
            while (!CancelTokenSource.IsCancellationRequested)
            {
                await BeginSync();
                var updated = await GetUpdated();
                if (updated.Any())
                {
                    foreach (var proc in Processors)
                    {
                        await proc.ProcessChanges(updated);
                    }
                }
                await EndSync();
                await Sleep(_timeToSleep);
            }
            _status = OperationStatus.Stopped;
        }
    }
}
