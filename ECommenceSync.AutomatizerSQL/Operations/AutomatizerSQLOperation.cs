using Dapper;
using ECommenceSync.AutomatizerSQL.Helpers;
using ECommenceSync.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommenceSync.AutomatizerSQL.Operations
{
    public abstract class AutomatizerSQLOperation<TValue> : GenericSourceOperation<int, TValue>
        where TValue : IEntity<int>
    {
        private const string SqlGetOperationTimeSpan = "SELECT [dbo].[StoreSync_GetOperationLastTimestamp](@CodigoEmpresa, @Operation)";
        private const string SqlUpdateOperationTImeSpan = "UPDATE StoreSync_OperationsTimestamps SET LastSyncTime = @LastSyncTime, LastSyncResult = 1, LastSyncInfo = 'OK'  WHERE EmpCod = @CodigoEmpresa AND Operation = @Operation";
        DateTimeOffset? _dbSyncTime;

        public IAutomatizerDataHelper DataHelper { get; set; }
        public IAutomatizerSQLOperationsHelper OperationHelper { get; set; }


        public override async Task BeginSync()
        {
            SyncTimeInfo = new SyncTimeInfo
            {
                CurrentSyncTime = DateTimeOffset.Now,
            };
            if (_dbSyncTime is null)
            {
                using var sqlConn = DataHelper.GetConnection();
                _dbSyncTime = await sqlConn.ExecuteScalarAsync<DateTimeOffset>(SqlGetOperationTimeSpan, new { DataHelper.CodigoEmpresa, Operation = Operation.ToString() });
            }
            SyncTimeInfo.LastSyncTime = _dbSyncTime.Value;
        }

        public override async Task EndSync()
        {
            using var sqlConn = DataHelper.GetConnection();
            await sqlConn.ExecuteAsync(SqlUpdateOperationTImeSpan, new { DataHelper.CodigoEmpresa, Operation = Operation.ToString(), LastSyncTime = SyncTimeInfo.CurrentSyncTime });
            _dbSyncTime = SyncTimeInfo.CurrentSyncTime; //Guardamos el tiempo se sincornizacion para evitar consultarlo a la base de datos en cada ejecución
            SyncTimeInfo = null;
        }


        public override Dictionary<int, int> GetHierarchy()
        {
            throw new NotImplementedException();
        }

        public override Task<TValue> ResolveEntity(int key)
        {
            throw new NotImplementedException();
        }

    }
}
