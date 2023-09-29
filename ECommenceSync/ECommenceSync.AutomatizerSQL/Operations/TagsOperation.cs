using ECommenceSync.AutomatizerSQL.Helpers;
using ECommenceSync.Entities;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ECommenceSync.AutomatizerSQL.Operations
{
    public class TagsOperation : AutomatizerSQLOperation<Tag<int>>
    {
        public override ECommenceSync.Operations Operation => ECommenceSync.Operations.Tags;

        public override OperationModes Mode => OperationModes.Automatic;

        public override OperationDirections Direction => OperationDirections.ErpToStore;

        public override Guid Identifier { get; } = Guid.NewGuid();

        OperationStatus _status;
        readonly int _timeToSleep;
        public override OperationStatus Status => _status;

        public TagsOperation(IAutomatizerDataHelper dataHelper, IAutomatizerSQLOperationsHelper operationsHelper)
        {
            DataHelper = dataHelper;
            OperationHelper = operationsHelper;
            _timeToSleep = OperationHelper.GetSearchTime(Operation);
            _status = OperationStatus.Created;
        }

        public override async Task<List<Tag<int>>> GetUpdated()
        {
            if (SyncTimeInfo is null) throw new InvalidOperationException("You must call beginsync!!!");
            using var sqlConn = (SqlConnection)DataHelper.GetConnection();
            using var sqlCmd = sqlConn.CreateCommand();
            await sqlConn.OpenAsync();
            sqlCmd.CommandText = "SELECT Id As TagId, Value FROM StoreSync_Tags WHERE EmpCod = @CodigoEmpresa AND FechaModificacion > @LastSyncTime";
            sqlCmd.Parameters.AddWithValue("CodigoEmpresa", DataHelper.CodigoEmpresa);
            sqlCmd.Parameters.AddWithValue("LastSyncTime", SyncTimeInfo.LastSyncTime);
            using var reader = await sqlCmd.ExecuteReaderAsync();
            var tags = new List<Tag<int>>();
            while (await reader.ReadAsync())
            {
                tags.Add(new Tag<int>
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
            }
            await sqlConn.CloseAsync();
            return tags;
        }

        public override async Task<List<Tag<int>>> ResolveEntities(List<int> keys)
        {
            if (keys.Count == 0) return new List<Tag<int>>();
            using var sqlConn = (SqlConnection)DataHelper.GetConnection();
            using var sqlCmd = sqlConn.CreateCommand();
            await sqlConn.OpenAsync();
            sqlCmd.CommandText = "SELECT Id As TagId, Value FROM StoreSync_Tags WHERE Id IN (SELECT * FROM string_split(@Keys, ','))";
            sqlCmd.Parameters.AddWithValue("Keys", string.Join(',', keys.Select(k => k.ToString())));

            using var reader = await sqlCmd.ExecuteReaderAsync();
            var tags = new List<Tag<int>>();
            while (await reader.ReadAsync())
            {
                tags.Add(new Tag<int>
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
            }
            await sqlConn.CloseAsync();
            return tags;
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
