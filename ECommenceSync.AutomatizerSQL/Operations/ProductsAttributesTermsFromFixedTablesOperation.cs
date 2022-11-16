using ECommenceSync.AutomatizerSQL.Helpers;
using ECommenceSync.Entities;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommenceSync.AutomatizerSQL.Operations
{
    public class ProductsAttributesTermsFromFixedTablesOperation : AutomatizerSQLOperation<ProductAttributeTerm<int>>
    {
        public override ECommenceSync.Operations Operation => ECommenceSync.Operations.AttributesTerms;

        public override OperationModes Mode => OperationModes.Automatic;

        public override OperationDirections Direction => OperationDirections.ErpToStore;

        public override Guid Identifier => Guid.NewGuid();

        private int _timeToSleep;
        private OperationStatus _status;
        public override OperationStatus Status => _status;

        private ConcurrentDictionary<int, ProductAttributeTerm<int>> _cachedTerms;

        public ProductsAttributesTermsFromFixedTablesOperation(IAutomatizerDataHelper dataHelper, IAutomatizerSQLOperationsHelper operationsHelper)
        {
            DataHelper = dataHelper;
            OperationHelper = operationsHelper;
            _timeToSleep = OperationHelper.GetSearchTime(Operation);
            _status = OperationStatus.Created;
            _cachedTerms = new ConcurrentDictionary<int, ProductAttributeTerm<int>>();
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

        public override async Task<List<ProductAttributeTerm<int>>> GetUpdated()
        {
            using var sqlConex = (SqlConnection)DataHelper.GetConnection();
            using var cmd = sqlConex.CreateCommand();
            cmd.CommandText = "SELECT  * FROM dbo.StoreSync_GetFixedAtributosValuesActualizados(@CodigoEmpresa, @Fecha)  ORDER BY Id";
            cmd.Parameters.AddWithValue("CodigoEmpresa", DataHelper.CodigoEmpresa);
            cmd.Parameters.AddWithValue("Fecha", SyncTimeInfo.LastSyncTime);
            await sqlConex.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            var attributes = new List<ProductAttributeTerm<int>>();
            while (await reader.ReadAsync())
            {
                ProductAttributeTerm<int> term = new()
                {
                    Id = reader.GetInt32(0),
                    AttributeId = reader.GetByte(1),
                    Name = reader.GetString(2),
                    RetryCount = 0,
                    Updated = false
                };
                attributes.Add(term);
                _cachedTerms.AddOrUpdate(reader.GetInt32(0), term, (k, v) => v);
            }
            await sqlConex.CloseAsync();
            return attributes;
        }

        public override async Task<List<ProductAttributeTerm<int>>> ResolveEntities(List<int> keys)
        {
            using var sqlConex = (SqlConnection)DataHelper.GetConnection();
            using var cmd = sqlConex.CreateCommand();
            cmd.CommandText = "SELECT  * FROM dbo.StoreSync_GetFixedAttributesTermnsByKeys(@CodigoEmpresa, @Keys) ORDER BY Id";
            cmd.Parameters.AddWithValue("CodigoEmpresa", DataHelper.CodigoEmpresa);
            cmd.Parameters.AddWithValue("@Keys", string.Join(',', keys.Select(k => k.ToString())));
            await sqlConex.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            var attributes = new List<ProductAttributeTerm<int>>();
            while (await reader.ReadAsync())
            {
                ProductAttributeTerm<int> term = new()
                {
                    Id = reader.GetInt32(0),
                    AttributeId = reader.GetByte(1),
                    Name = reader.GetString(2),
                    RetryCount = 0,
                    Updated = false
                };
                attributes.Add(term);
                _cachedTerms.AddOrUpdate(reader.GetInt32(0), term, (k, v) => v);
            }
            await sqlConex.CloseAsync();
            return attributes;
        }


        public override async Task<ProductAttributeTerm<int>> ResolveEntity(int key)
        {
            if (_cachedTerms.ContainsKey(key))
            {
                return _cachedTerms[key];
            }
            var terms = await ResolveEntities(new() { key });
            terms.ForEach(x => _cachedTerms.AddOrUpdate(key, x, (k, v) => v));
            return terms.First();
        }
    }
}
