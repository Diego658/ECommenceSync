using Bukimedia.PrestaSharp.Entities;
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
    public class OrdersStatesOperation : PrestashopOperation<IEntity<long>>
    {
        OperationStatus _status;
        readonly IPrestashopOperationsHelper _operationsHelper;
        readonly OrderStateFactory _orderStatesFactory;
        readonly string _tableName;
        public override ECommenceSync.Operations Operation => ECommenceSync.Operations.OrdersStates ;

        public override OperationModes Mode => OperationModes.Manual;

        public override OperationDirections Direction => OperationDirections.StoreToStore;

        public override OperationStatus Status => _status;


        public OrdersStatesOperation(IPrestashopDatabaseHelper dataHelper, IPrestashopOperationsHelper operationsHelper, string tableName = "StoreSync_OrdersStates_Prestashop")
        {
            _tableName = tableName;
            _operationsHelper = operationsHelper;
            DatabaseHelper = dataHelper;
            _status = OperationStatus.Created;
            _orderStatesFactory = new OrderStateFactory(dataHelper.ApiUrl, dataHelper.ApiSecret, "");
        }

        public override Dictionary<long, long> GetHierarchy()
        {
            throw new NotImplementedException();
        }

        public override async Task<List<IEntity<long>>> GetUpdated()
        {
            if (SyncTimeInfo is null) throw new InvalidOperationException("You must call beginsync!!!");
            var (psOrderStates, error) = await MethodHelper.ExecuteMethodAsync(async () =>
            {
                return await _orderStatesFactory.GetAllAsync();
            }, MethodHelper.NotRetryOnBadrequest);

            if (Processors.Count == 0)
            {
                await SaveOrderStates(psOrderStates);
                return null;
            }
            else
            {
                throw new NotSupportedException("Operación no compatible con procesamiento de cambios!!!");
            }
        }
        async Task SaveOrderStates(List<order_state> psOrderStates)
        {
            try
            {
                using var conex = DatabaseHelper.GetConnection();
                await conex.OpenAsync();
                using var tran = await conex.BeginTransactionAsync();
                await conex.ExecuteAsync($"DELETE FROM {_tableName} where id >0", null, tran);
                foreach (var state in psOrderStates)
                {
                    await conex.ExecuteAsync(@"INSERT INTO StoreSync_OrdersStates_Prestashop
                         (id, unremovable, delivery, hidden, send_email, module_name, invoice, 
                        color, logable, shipped, paid, pdf_delivery, pdf_invoice, deleted, name, template)
                        VALUES (@id, @unremovable, @delivery, @hidden, @send_email, @module_name, @invoice, 
                        @color, @logable, @shipped, @paid, @pdf_delivery, @pdf_invoice, @deleted, @name, @template)",
                         new
                         {
                             state.id,
                             state.unremovable,
                             state.delivery,
                             state.hidden,
                             state.send_email,
                             state.module_name,
                             state.invoice,
                             state.color,
                             state.logable,
                             state.shipped,
                             state.paid,
                             pdf_delivery = false,
                             pdf_invoice = false,
                             state.deleted,
                             name = state.name.First(x=> x.id == DatabaseHelper.SyncLanguage).Value,
                             template = state.template.First(x => x.id == DatabaseHelper.SyncLanguage).Value

                         }, tran);
                }
                await tran.CommitAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override Task<List<IEntity<long>>> ResolveEntities(List<long> keys)
        {
            throw new NotSupportedException("Operación no compatible con resolución de entidades!!!");
        }

        public override async Task Work()
        {
            _status = OperationStatus.Working;
            await BeginSync();
            await GetUpdated();
            await EndSync();
            _status = OperationStatus.Stopped;
        }

        public override Task<IEntity<long>> ResolveEntity(long key)
        {
            throw new NotImplementedException();
        }
    }
}
