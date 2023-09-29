using ECommenceSync.AutomatizerSQL.Helpers;
using ECommenceSync.Entities;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommenceSync.AutomatizerSQL.Operations
{
    public class ProductsAttributesFromFixedTablesOperation : AutomatizerSQLOperation<ProductAttribute<int>>
    {
        public override ECommenceSync.Operations Operation => ECommenceSync.Operations.Attributes ;

        public override OperationModes Mode =>  OperationModes.Automatic ;

        public override OperationDirections Direction => OperationDirections.ErpToStore ;

        public override Guid Identifier => Guid.NewGuid();

        private int _timeToSleep;
        private OperationStatus _status;
        public override OperationStatus Status => _status;

        public ProductsAttributesFromFixedTablesOperation(IAutomatizerDataHelper dataHelper, IAutomatizerSQLOperationsHelper operationsHelper)
        {
            DataHelper = dataHelper;
            OperationHelper = operationsHelper;
            _timeToSleep = OperationHelper.GetSearchTime(Operation);
            _status = OperationStatus.Created;
        }

        public override async Task<List<ProductAttribute<int>>> GetUpdated()
        {
            using var sqlConex = (SqlConnection)DataHelper.GetConnection();
            using var cmd = sqlConex.CreateCommand();
            cmd.CommandText = "SELECT  * FROM dbo.StoreSync_GetFixedAtributosActualizados(@CodigoEmpresa, @Fecha)  ORDER BY Id";
            cmd.Parameters.AddWithValue("CodigoEmpresa", DataHelper.CodigoEmpresa);
            cmd.Parameters.AddWithValue("Fecha", SyncTimeInfo.LastSyncTime);
            await sqlConex.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            var attributes = new List<ProductAttribute<int>>();
            while (await reader.ReadAsync())
            {
                attributes.Add(new() 
                {
                    Id = reader.GetByte(0),
                    Name = reader.GetString(1),
                    RetryCount = 0,
                     Updated = false
                });
            }
            await sqlConex.CloseAsync();
            return attributes;
        }

        public override async Task<List<ProductAttribute<int>>> ResolveEntities(List<int> keys)
        {
            using var sqlConex = (SqlConnection)DataHelper.GetConnection();
            using var cmd = sqlConex.CreateCommand();
            cmd.CommandText = "SELECT  * FROM dbo.StoreSync_GetFixedAttributesByKeys(@CodigoEmpresa, @Keys) ORDER BY Id";
            cmd.Parameters.AddWithValue("CodigoEmpresa", DataHelper.CodigoEmpresa);
            cmd.Parameters.AddWithValue("@Keys", string.Join(',', keys.Select(k => k.ToString())));
            await sqlConex.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            var attributes = new List<ProductAttribute<int>>();
            while (await reader.ReadAsync())
            {
                attributes.Add(new()
                {
                    Id = reader.GetByte(0),
                    Name = reader.GetString(1),
                    RetryCount = 0,
                    Updated = false
                });
            }
            await sqlConex.CloseAsync();
            return attributes;
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
