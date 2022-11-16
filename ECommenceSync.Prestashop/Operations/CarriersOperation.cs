using Bukimedia.PrestaSharp.Factories;
using Dapper;
using ECommenceSync.Interfaces;
using ECommenceSync.Prestashop.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommenceSync.Prestashop.Operations
{
    public class CarriersOperation : PrestashopOperation<IEntity<long>>
    {
        OperationStatus _status;
        readonly IPrestashopOperationsHelper _operationsHelper;
        readonly CarrierFactory _carriersFactory;
        readonly string _tableName;
        public override ECommenceSync.Operations Operation => ECommenceSync.Operations.Carriers;

        public override OperationModes Mode => OperationModes.Manual;

        public override OperationDirections Direction => OperationDirections.StoreToStore;

        public override OperationStatus Status => _status;

        public override Dictionary<long, long> GetHierarchy()
        {
            throw new NotImplementedException();
        }


        public CarriersOperation(IPrestashopDatabaseHelper dataHelper, IPrestashopOperationsHelper operationsHelper, string tableName = "StoreSync_Carriers_Prestashop")
        {
            _tableName = tableName;
            _operationsHelper = operationsHelper;
            DatabaseHelper = dataHelper;
            _status = OperationStatus.Created;
            _carriersFactory = new CarrierFactory(dataHelper.ApiUrl, dataHelper.ApiSecret, "");
        }


        public override async Task<List<IEntity<long>>> GetUpdated()
        {
            if (SyncTimeInfo is null) throw new InvalidOperationException("You must call beginsync!!!");
            //var filter = new Dictionary<string, string>
            //{
            //    {"date_upd",  SyncTimeInfo.LastSyncTime.ToString(">[yyyy-MM-dd HH:mm:ss]")}
            //};

            var (psCarriers, error) = await MethodHelper.ExecuteMethodAsync(async () =>
            {
                return await _carriersFactory.GetAllAsync();
            }, MethodHelper.NotRetryOnBadrequest);

            if (Processors.Count == 0)
            {
                await SaveCarriers(psCarriers);
                return null;
            }
            else
            {
                throw new NotSupportedException("Operación no compatible con procesamiento de cambios!!!");
            }
        }


        private async Task SaveCarriers(List<Bukimedia.PrestaSharp.Entities.carrier> carriers)
        {


            using var conex = DatabaseHelper.GetConnection();
            await conex.OpenAsync();
            using var tran = await conex.BeginTransactionAsync();
            await conex.ExecuteAsync($"DELETE FROM {_tableName}", null, tran);
            foreach (var carrier in carriers)
            {
                await conex.ExecuteAsync(@"INSERT INTO StoreSync_Carriers_Prestashop
                         (id, deleted, is_module, id_tax_rules_group, id_reference, name, active, is_free, url, 
                            shipping_handling, shipping_external, range_behavior, shipping_method, max_width, max_height, 
                            max_depth, max_weight, grade, external_module_name, need_range, position, delay)
                        VALUES (@id, @deleted, @is_module, @id_tax_rules_group, @id_reference, @name, @active, 
                        @is_free, @url, @shipping_handling, @shipping_external, @range_behavior, 
                        @shipping_method, @max_width, @max_height, @max_depth, @max_weight, @grade, 
                         @external_module_name, @need_range, @position, @delay)",
                     new
                     {
                         carrier.id,
                         carrier.deleted,
                         carrier.is_module,
                         carrier.id_tax_rules_group,
                         carrier.id_reference,
                         carrier.name,
                         carrier.active,
                         carrier.is_free,
                         carrier.url,
                         carrier.shipping_handling,
                         carrier.shipping_external,
                         carrier.range_behavior,
                         carrier.shipping_method,
                         carrier.max_width,
                         carrier.max_height,
                         carrier.max_depth,
                         carrier.max_weight,
                         carrier.grade,
                         carrier.external_module_name,
                         carrier.need_range,
                         carrier.position,
                         delay = carrier.delay.First(x => x.id == DatabaseHelper.SyncLanguage).Value
                     }, tran);
            }
            await tran.CommitAsync();

        }
        public override Task<List<IEntity<long>>> ResolveEntities(List<long> keys)
        {
            throw new NotSupportedException("Operación no compatible con resolución de entidades!!!");
        }

        public override async Task Work()
        {
            _status = OperationStatus.Working;
            await BeginSync();
            try
            {
                await GetUpdated();
                await EndSync();
            }
            catch (Exception ex)
            {
                await EndSyncWithError(ex);
            }

            _status = OperationStatus.Stopped;
        }

        public override Task<IEntity<long>> ResolveEntity(long key)
        {
            throw new NotImplementedException();
        }
    }
}
