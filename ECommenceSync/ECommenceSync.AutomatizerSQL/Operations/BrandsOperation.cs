using ECommenceSync.AutomatizerSQL.Helpers;
using ECommenceSync.Entities;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ECommenceSync.AutomatizerSQL.Operations
{
    public class BrandsOperation : AutomatizerSQLOperation<Brand<int>>
    {
        private const string SqlBrands = "SELECT * FROM dbo.StoreSync_GetMarcasActualizadas(@CodigoEmpresa, @LastSyncTime)";
        private const string SqlResolveBrands = @"SELECT Id, ID As BrandId, MarDsc As Name, Description, DescriptionShort, LogoBlobId
            FROM StoreSync_Marcas
                INNER JOIN Marcas ON StoreSync_Marcas.EmpCod = Marcas.EmpCod AND StoreSync_Marcas.MarcaID = Marcas.MarCod
            WHERE ID IN (SELECT * FROM string_split(@Keys, ',') )";
        private readonly int _timeToSleep;
        Dictionary<string, int> _updatedColsSchema;

        public override ECommenceSync.Operations Operation => ECommenceSync.Operations.Brands;

        public override OperationModes Mode => OperationModes.Automatic;

        public override OperationDirections Direction => OperationDirections.ErpToStore;

        public override Guid Identifier { get; } = Guid.NewGuid();

        OperationStatus status;
        public override OperationStatus Status { get => status; }

        public BrandsOperation(IAutomatizerDataHelper dataHelper, IAutomatizerSQLOperationsHelper operationsHelper)
        {
            DataHelper = dataHelper;
            OperationHelper = operationsHelper;
            _timeToSleep = OperationHelper.GetSearchTime(Operation);
            status = OperationStatus.Created;
        }




        public override async Task<List<Brand<int>>> GetUpdated()
        {
            
            
            
            
            if (SyncTimeInfo is null) throw new InvalidOperationException("You must call beginsync!!!");
            using var sqlConn = (SqlConnection)DataHelper.GetConnection();
            using var sqlCmd = sqlConn.CreateCommand();
            await sqlConn.OpenAsync();
            sqlCmd.CommandText = SqlBrands;
            sqlCmd.Parameters.AddWithValue("CodigoEmpresa", DataHelper.CodigoEmpresa);
            sqlCmd.Parameters.AddWithValue("LastSyncTime", SyncTimeInfo.LastSyncTime);
            using var reader = await sqlCmd.ExecuteReaderAsync();


            if (_updatedColsSchema is null)
            {
                ConfigureColsSchema(reader);
            }
            List<Brand<int>> brands = await LoadBrands(reader);
            await reader.CloseAsync();
            await sqlConn.CloseAsync();
            return brands;
        }

        private async Task<List<Brand<int>>> LoadBrands(SqlDataReader reader)
        {
            if (_updatedColsSchema is null)
            {
                ConfigureColsSchema(reader);
            }

            var brands = new List<Brand<int>>(10);
            while (await reader.ReadAsync())
            {
                var brand = new Brand<int>
                {
                    Id = reader.GetInt32(_updatedColsSchema["Id"]),
                    Name = reader.GetString(_updatedColsSchema["Name"]),
                    Description = reader.GetString(_updatedColsSchema["Description"]),
                    DescriptionShort = reader.GetString(_updatedColsSchema["DescriptionShort"])
                };

                if (reader.GetInt64(_updatedColsSchema["LogoBlobId"]) > 0)
                {
                    brand.ImageBlob = new AutomatizerSQLBlobImage(DataHelper, reader.GetInt64(_updatedColsSchema["LogoBlobId"]));
                }


                brands.Add(brand);
            }
            return brands;
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

        public override async Task Work()
        {
            if (Processors.Count == 0)
            {
                status = OperationStatus.Stopped;
                return;
            }
            status = OperationStatus.Working;
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
            status = OperationStatus.Stopped;
        }

        public override async Task<List<Brand<int>>> ResolveEntities(List<int> keys)
        {
            if (keys.Count == 0) return new List<Brand<int>>();
            using var sqlConn = (SqlConnection)DataHelper.GetConnection();
    
            using var sqlCmd = sqlConn.CreateCommand();
            await sqlConn.OpenAsync();
            sqlCmd.CommandText = SqlResolveBrands;
            sqlCmd.Parameters.AddWithValue("Keys", string.Join(',', keys.Select(k => k.ToString())));

            using var reader = await sqlCmd.ExecuteReaderAsync();
            var brands = await LoadBrands(reader);
            await reader.CloseAsync();
            await sqlConn.CloseAsync();
            return brands;
        }
    }
}
