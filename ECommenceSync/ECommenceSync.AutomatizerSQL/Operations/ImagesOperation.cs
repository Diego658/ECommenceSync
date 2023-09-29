using Dapper;
using ECommenceSync.AutomatizerSQL.Helpers;
using ECommenceSync.Entities;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ECommenceSync.AutomatizerSQL.Operations
{
    public class ImagesOperation : AutomatizerSQLOperation<EntityImage<int>>
    {
        public override ECommenceSync.Operations Operation => ECommenceSync.Operations.ProductImages;

        public override OperationModes Mode => OperationModes.Automatic;

        public override OperationDirections Direction => OperationDirections.ErpToStore;

        public override Guid Identifier => Guid.NewGuid();

        OperationStatus _status;
        readonly int _timeToSleep;
        public override OperationStatus Status { get => _status; }

        public ImagesOperation(IAutomatizerDataHelper dataHelper, IAutomatizerSQLOperationsHelper operationsHelper)
        {
            DataHelper = dataHelper;
            OperationHelper = operationsHelper;
            _timeToSleep = OperationHelper.GetSearchTime(Operation);
            _status = OperationStatus.Created;
        }

        public override async Task<List<EntityImage<int>>> GetUpdated()
        {
            if (SyncTimeInfo is null) throw new InvalidOperationException("You must call beginsync!!!");
            using var sqlConn = (SqlConnection)DataHelper.GetConnection();
            using var sqlCmd = sqlConn.CreateCommand();
            await sqlConn.OpenAsync();
            sqlCmd.CommandText = "SELECT * FROM StoreSync_GetImagenesActualizadas(@CodigoEmpresa, @LastSyncTime)";
            sqlCmd.Parameters.AddWithValue("CodigoEmpresa", DataHelper.CodigoEmpresa);
            sqlCmd.Parameters.AddWithValue("LastSyncTime", SyncTimeInfo.LastSyncTime);
            using var reader = await sqlCmd.ExecuteReaderAsync();
            var rowParser = reader.GetRowParser<EntityImage<int>>();
            var images = new List<EntityImage<int>>();
            var colBlobId = reader.GetOrdinal("BlobID");
            while (await reader.ReadAsync())
            {
                var image = rowParser(reader);
                image.Blob = new AutomatizerSQLBlobImage(DataHelper, reader.GetInt64(colBlobId));
                images.Add(image);
            }
            return images;
        }

        public override async Task<List<EntityImage<int>>> ResolveEntities(List<int> keys)
        {
            using var sqlConn = (SqlConnection)DataHelper.GetConnection();
            using var sqlCmd = sqlConn.CreateCommand();
            await sqlConn.OpenAsync();
            sqlCmd.CommandText = "SELECT * FROM StoreSync_GetImagenesByKeys(@Keys)";
            sqlCmd.Parameters.AddWithValue("Keys", string.Join(',', keys.Select(k => k.ToString())));
            using var reader = await sqlCmd.ExecuteReaderAsync();
            var rowParser = reader.GetRowParser<EntityImage<int>>();
            var images = new List<EntityImage<int>>();
            var colBlobId = reader.GetOrdinal("BlobID");
            while (await reader.ReadAsync())
            {
                var image = rowParser(reader);
                image.Blob = new AutomatizerSQLBlobImage(DataHelper, reader.GetInt64(colBlobId));
                images.Add(image);
            }
            return images;
        }

        public  override async Task Work()
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
                if (updated.Count > 0)
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
