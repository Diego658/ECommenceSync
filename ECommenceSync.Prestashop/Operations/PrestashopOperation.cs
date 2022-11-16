using Dapper;
using ECommenceSync.Interfaces;
using ECommenceSync.Prestashop.Helpers;
using System;
using System.Threading.Tasks;

namespace ECommenceSync.Prestashop.Operations
{
    public abstract class PrestashopOperation<TValue> : GenericSourceOperation<long, TValue>
        where TValue : IEntity<long>
    {
        private const string SqlGetOperationTimeSpan = "SELECT LastSyncTime FROM StoreSync_PrestashopOperations WHERE Operation = @Operation";
        private const string SqlUpdateOperationTImeSpan = "UPDATE StoreSync_PrestashopOperations SET LastSyncTime = @LastSyncTime, Error = @Error, StackTrace = @StackTrace WHERE Operation = @Operation";
        DateTimeOffset? _dbSyncTime;
        
        

        
        public override Guid Identifier => Guid.NewGuid();

        
        public IPrestashopDatabaseHelper DatabaseHelper { get; set; }


        public override async Task BeginSync()
        {
            SyncTimeInfo = new SyncTimeInfo
            {
                CurrentSyncTime = DateTimeOffset.Now,
            };
            if (_dbSyncTime is null)
            {
                using var sqlConn = DatabaseHelper.GetConnection();
                _dbSyncTime = await sqlConn.ExecuteScalarAsync<DateTimeOffset>(SqlGetOperationTimeSpan, new { Operation = Operation.ToString() });
            }
            SyncTimeInfo.LastSyncTime = _dbSyncTime.Value;
        }

        public override async Task EndSync()
        {
            using var sqlConn = DatabaseHelper.GetConnection();
            await sqlConn.ExecuteAsync(SqlUpdateOperationTImeSpan, new { Operation = Operation.ToString(), LastSyncTime = SyncTimeInfo.CurrentSyncTime, Error = "", StackTrace = "" });
            _dbSyncTime = SyncTimeInfo.CurrentSyncTime; //Guardamos el tiempo se sincornizacion para evitar consultarlo a la base de datos en cada ejecución
            SyncTimeInfo = null;
        }


        public async Task EndSyncWithError(Exception exception)
        {
            using var sqlConn = DatabaseHelper.GetConnection();
            await sqlConn.ExecuteAsync(SqlUpdateOperationTImeSpan, new { Operation = Operation.ToString(), LastSyncTime = SyncTimeInfo.LastSyncTime, Error = exception.Message.GetTruncated(5000), StackTrace = exception.StackTrace.GetTruncated(2000) });
            _dbSyncTime = SyncTimeInfo.LastSyncTime; //Guardamos el tiempo se sincornizacion para evitar consultarlo a la base de datos en cada ejecución
            SyncTimeInfo = null;
        }

    }
}
