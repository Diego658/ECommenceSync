using ECommenceSync.AutomatizerSQL.Helpers;
using ECommenceSync.Entities;
using ECommenceSync.Interfaces;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ECommenceSync.AutomatizerSQL.Operations
{
    public class ProductSpecificPricesOperation : AutomatizerSQLOperation<ProductPrice<int>>, ISpecificPricesOperation<int>
    {
        OperationStatus _status;
        readonly int _timeToSleep;
        Dictionary<string, int> pricesToSync;
        public override ECommenceSync.Operations Operation => ECommenceSync.Operations.ProductPrices;

        public override OperationModes Mode => OperationModes.Automatic;

        public override OperationDirections Direction => OperationDirections.ErpToStore;

        public override Guid Identifier => Guid.NewGuid();

        public override OperationStatus Status => _status;

        public ProductSpecificPricesOperation(IAutomatizerDataHelper dataHelper, IAutomatizerSQLOperationsHelper operationsHelper)
        {
            DataHelper = dataHelper;
            OperationHelper = operationsHelper;
            _timeToSleep = OperationHelper.GetSearchTime(Operation);
            _status = OperationStatus.Created;
        }

        public Dictionary<string, int> GetPricesToSync()
        {
            if (pricesToSync == null)
            {
                pricesToSync = new Dictionary<string, int>();
                var specificPriccesSection = OperationHelper.GetConfiguration().GetSection("SpecificPrices");

                foreach (var priceSection in specificPriccesSection.GetChildren())
                {
                    pricesToSync.Add(priceSection.Key, int.Parse(priceSection.GetSection("IdTipoCliente").Value));
                }
            }

            return pricesToSync;
        }

        public override async Task<List<ProductPrice<int>>> GetUpdated()
        {
            if (SyncTimeInfo is null) throw new InvalidOperationException("You must call beginsync!!!");
            using var sqlConn = (SqlConnection)DataHelper.GetConnection();
            using var sqlCmd = sqlConn.CreateCommand();
            await sqlConn.OpenAsync();
            sqlCmd.CommandText = @"SELECT StoreSync_Productos_Precios.Id As PriceId , StoreSync_Productos_Precios.ItemId, StoreSync_Productos_Precios.TipoClienteId, StoreSync_Productos_Precios.Precio As Price
            FROM StoreSync_Productos_Precios INNER JOIN
                StoreSync_Productos ON StoreSync_Productos.ItemID = StoreSync_Productos_Precios.ItemId
            WHERE StoreSync_Productos_Precios.FechaModificacion >= @LastSyncTime AND StoreSync_Productos_Precios.TipoClienteId IN ( select CAST(value as smallint) from string_split(@Keys, ',') )";
            sqlCmd.Parameters.AddWithValue("LastSyncTime", SyncTimeInfo.LastSyncTime);
            sqlCmd.Parameters.AddWithValue("@Keys", string.Join(',', GetPricesToSync().Values.Select(v => v.ToString())));
            using var reader = await sqlCmd.ExecuteReaderAsync();
            var prices = new List<ProductPrice<int>>();
            while (await reader.ReadAsync())
            {
                prices.Add(new ProductPrice<int>
                {
                    Id = reader.GetInt32(0),
                    ParentId = reader.GetInt32(1),
                    ClientGroupId = reader.GetByte(2),
                    Price = reader.GetDecimal(3),
                    Name = ""
                    
                });
            }
            await sqlConn.CloseAsync();
            return prices;
        }

        public override async Task<List<ProductPrice<int>>> ResolveEntities(List<int> keys)
        {
            using var sqlConn = (SqlConnection)DataHelper.GetConnection();
            using var sqlCmd = sqlConn.CreateCommand();
            await sqlConn.OpenAsync();
            sqlCmd.CommandText = @"SELECT StoreSync_Productos_Precios.Id As PriceId , StoreSync_Productos_Precios.ItemId, StoreSync_Productos_Precios.TipoClienteId, StoreSync_Productos_Precios.Precio As Price
            FROM StoreSync_Productos_Precios INNER JOIN
                StoreSync_Productos ON StoreSync_Productos.ItemID = StoreSync_Productos_Precios.ItemId
				WHERE StoreSync_Productos_Precios.id IN (SELECT cast(value as int) from string_split(@Keys, ','))";
            sqlCmd.Parameters.AddWithValue("@Keys", string.Join(',', keys.Select(v => v.ToString())));
            using var reader = await sqlCmd.ExecuteReaderAsync();
            var prices = new List<ProductPrice<int>>();
            while (await reader.ReadAsync())
            {
                prices.Add(new ProductPrice<int>
                {
                    Id = reader.GetInt32(0),
                    ParentId = reader.GetInt32(1),
                    ClientGroupId = reader.GetByte(2),
                    Price = reader.GetDecimal(3),
                    Name = ""

                });
            }
            await sqlConn.CloseAsync();
            return prices;
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
