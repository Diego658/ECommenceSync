using ECommenceSync.AutomatizerSQL.Helpers;
using ECommenceSync.Entities;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ECommenceSync.AutomatizerSQL.Operations
{
    public class ProductStocksOperation : AutomatizerSQLOperation<ProductStock<int>>
    {
        OperationStatus _status;
        readonly int _timeToSleep;
        public override ECommenceSync.Operations Operation => ECommenceSync.Operations.ProductStocks;

        public override OperationModes Mode => OperationModes.Automatic;

        public override OperationDirections Direction => OperationDirections.ErpToStore;

        public override Guid Identifier => Guid.NewGuid();

        public override OperationStatus Status => _status;


        public ProductStocksOperation(IAutomatizerDataHelper dataHelper, IAutomatizerSQLOperationsHelper operationsHelper)
        {
            DataHelper = dataHelper;
            OperationHelper = operationsHelper;
            _timeToSleep = OperationHelper.GetSearchTime(Operation);
            _status = OperationStatus.Created;
        }

        public override async Task<List<ProductStock<int>>> GetUpdated()
        {
            if (SyncTimeInfo is null) throw new InvalidOperationException("You must call beginsync!!!");
            using var sqlConn = (SqlConnection)DataHelper.GetConnection();
            using var sqlCmd = sqlConn.CreateCommand();
            await sqlConn.OpenAsync();
            sqlCmd.CommandText = "SELECT  * FROM StoreSync_GetProductosStocksActualizados(@CodigoEmpresa, @LastSyncTime,@CodigoBodega)";
            sqlCmd.Parameters.AddWithValue("CodigoEmpresa", DataHelper.CodigoEmpresa);
            sqlCmd.Parameters.AddWithValue("LastSyncTime", SyncTimeInfo.LastSyncTime);
            sqlCmd.Parameters.AddWithValue("CodigoBodega", DataHelper.CodigoBodega );
            using var reader = await sqlCmd.ExecuteReaderAsync();
            var stocks = new List<ProductStock<int>>();
            while (await reader.ReadAsync())
            {
                stocks.Add(new ProductStock<int>
                {
                    Id = reader.GetInt32(0),
                    Name = "",
                    Existencia = reader.GetDecimal(1),
                    ProductId = reader.GetInt32(2),
                });
            }
            await sqlConn.CloseAsync();
            return stocks;
        }



        public override async Task<List<ProductStock<int>>> ResolveEntities(List<int> keys)
        {
            
            using var sqlConn = (SqlConnection)DataHelper.GetConnection();
            using var sqlCmd = sqlConn.CreateCommand();
            await sqlConn.OpenAsync();
            sqlCmd.CommandText = "SELECT  * FROM StoreSync_GetProductosStocksByKeys(@Keys)";
            sqlCmd.Parameters.AddWithValue("Keys", string.Join(',', keys.Select(k => k.ToString())));
            
            using var reader = await sqlCmd.ExecuteReaderAsync();
            var stocks = new List<ProductStock<int>>();
            while (await reader.ReadAsync())
            {
                stocks.Add(new ProductStock<int>
                {
                    Id = reader.GetInt32(0),
                    Name = "",
                    Existencia = reader.GetDecimal(1),
                    ProductId = reader.GetInt32(2),
                });
            }
            await sqlConn.CloseAsync();
            return stocks;
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
