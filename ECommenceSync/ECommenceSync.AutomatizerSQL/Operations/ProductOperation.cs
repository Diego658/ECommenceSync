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
    public class ProductOperation : AutomatizerSQLOperation<Product<int>>
    {
        private const string SqlGetUpdated = "SELECT * FROM dbo.StoreSync_GetProductosActualizados(@CodigoEmpresa, @LastSyncTime ,  @TipoPrecioPaginaWeb) WHERE HasVariants = 0";
        OperationStatus _status;
        readonly int _timeToSleep;
        public override ECommenceSync.Operations Operation => ECommenceSync.Operations.Products;

        public override OperationModes Mode => OperationModes.Automatic;

        public override OperationDirections Direction => OperationDirections.ErpToStore;

        public override Guid Identifier => Guid.NewGuid();


        public override OperationStatus Status => _status;

        public ProductOperation(IAutomatizerDataHelper dataHelper, IAutomatizerSQLOperationsHelper operationsHelper)
        {
            DataHelper = dataHelper;
            OperationHelper = operationsHelper;
            _timeToSleep = OperationHelper.GetSearchTime(Operation);
            _status = OperationStatus.Created;
        }

        public override async Task<List<Product<int>>> GetUpdated()
        {
            await using var sqlConex = (SqlConnection)DataHelper.GetConnection();
            await using var reader = await sqlConex.ExecuteReaderAsync(SqlGetUpdated, new { DataHelper.CodigoEmpresa, SyncTimeInfo.LastSyncTime, DataHelper.TipoPrecioPaginaWeb });
            var parser = reader.GetRowParser<Product<int>>();
            var products = new List<Product<int>>();
            while (await reader.ReadAsync())
            {
                products.Add(parser.Invoke(reader));
            }
            return products;
        }

        public override async Task<List<Product<int>>> ResolveEntities(List<int> keys)
        {
            await using var sqlConex = (SqlConnection)DataHelper.GetConnection();
            await using var reader = await sqlConex.ExecuteReaderAsync("SELECT * FROM StoreSync_GetProductosByKeys(@Keys, @PrecioPaginaWeb)", 
                new { Keys = string.Join(',', keys.Select(k => k.ToString())), PrecioPaginaWeb = DataHelper.TipoPrecioPaginaWeb });
            var parser = reader.GetRowParser<Product<int>>();
            var products = new List<Product<int>>();
            while (await reader.ReadAsync())
            {
                products.Add(parser.Invoke(reader));
            }
            return products;

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
                        System.Diagnostics.Debug.WriteLine($"{updated.First().Name}");
                    }
                }
                await EndSync();
                await Sleep(_timeToSleep);
            }
            _status = OperationStatus.Stopped;
        }

        public override Dictionary<int, int> GetHierarchy()
        {
            using var sqlConex = (SqlConnection)DataHelper.GetConnection();
            using var cmd = sqlConex.CreateCommand();
            cmd.CommandText = @"SELECT dbo.StoreSync_ItemsVsPadres.ItemID, dbo.ItmMae.Secuencial AS Padre
            FROM dbo.StoreSync_ItemsVsPadres INNER JOIN
                         dbo.ItmMae ON dbo.StoreSync_ItemsVsPadres.EmpCod = dbo.ItmMae.EmpCod AND dbo.StoreSync_ItemsVsPadres.CodigoPadre = dbo.ItmMae.ItmCod
            WHERE (dbo.StoreSync_ItemsVsPadres.EmpCod = @0) AND (dbo.StoreSync_ItemsVsPadres.Tipo = 'G')";
            cmd.Parameters.AddWithValue("@0", DataHelper.CodigoEmpresa);
            sqlConex.Open();
            using var reader = cmd.ExecuteReader();
            var hierarchy = new Dictionary<int, int>();
            while (reader.Read())
            {
                hierarchy.Add(reader.GetInt32(0), reader.GetInt32(1));
            }
            return hierarchy;
        }

    }
}
