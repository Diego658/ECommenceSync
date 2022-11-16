using Bukimedia.PrestaSharp.Entities;
using Bukimedia.PrestaSharp.Factories;
using Dapper;
using ECommenceSync.Interfaces;
using ECommenceSync.Prestashop.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommenceSync.Prestashop.Operations
{
    public class CustomerServiceThreadsOperation : PrestashopOperation<IEntity<long>>
    {
        OperationStatus _status;
        readonly string _tableName;
        readonly IPrestashopOperationsHelper _operationsHelper;
        readonly CustomerThreadFactory _customerServiceThreadsFactory;
        readonly CustomerMessageFactory _customerMessageFactory;
        readonly int _timeToSleep;
        readonly string _tableNameMessages;
        public override ECommenceSync.Operations Operation => ECommenceSync.Operations.CustomerServiceThreads;

        public override OperationModes Mode => OperationModes.Automatic;

        public override OperationDirections Direction => OperationDirections.StoreToStore;

        public override OperationStatus Status => _status;

        public CustomerServiceThreadsOperation(IPrestashopDatabaseHelper dataHelper, IPrestashopOperationsHelper operationsHelper, string tableName = "StoreSync_CustomerThreads_Prestashop", string tableNameMessages = "StoreSync_CustomerMessages_Prestashop")
        {
            _tableName = tableName;
            _tableNameMessages = tableNameMessages;
            _operationsHelper = operationsHelper;
            DatabaseHelper = dataHelper;
            _status = OperationStatus.Created;
            _customerServiceThreadsFactory = new CustomerThreadFactory(dataHelper.ApiUrl, dataHelper.ApiSecret, "");
            _customerMessageFactory = new CustomerMessageFactory(dataHelper.ApiUrl, dataHelper.ApiSecret, "");
            _timeToSleep = operationsHelper.GetSearchTime(Operation);
        }




        public override Dictionary<long, long> GetHierarchy()
        {
            throw new NotImplementedException();
        }

        async Task SaveThreads(List<customer_thread> psThreads)
        {
            using var conex = DatabaseHelper.GetConnection();
            await conex.OpenAsync();

            foreach (var thread in psThreads)
            {
                using var tran = await conex.BeginTransactionAsync();
                var filter = new Dictionary<string, string>();
                filter.Add("id_customer_thread", thread.id.ToString());
                var messages = await _customerMessageFactory.GetByFilterAsync(filter, "", "");
                await conex.ExecuteAsync($"DELETE FROM {_tableName} WHERE id = @id", thread, tran);
                await conex.ExecuteAsync($"DELETE FROM {_tableNameMessages} WHERE id_customer_thread = @id", thread, tran);
                await conex.ExecuteAsync(@"INSERT INTO StoreSync_CustomerThreads_Prestashop
                         (Id, id_lang, id_shop, id_customer, id_order, id_product, id_contact, email, token, status)
                        VALUES    (@Id, @id_lang, @id_shop, @id_customer, @id_order, @id_product, @id_contact, @email, @token, @status)",
                     thread, tran);

                foreach (var message in messages)
                {
                    await conex.ExecuteAsync(@"INSERT INTO StoreSync_CustomerMessages_Prestashop
                         (id, id_employee, id_customer_thread, ip_address, message, file_name, user_agent, private, date_add, [read])
                VALUES       (@id, @id_employee, @id_customer_thread, @ip_address, @message, @file_name, @user_agent, @private, @date_add, @read)", message, tran);
                }
                await tran.CommitAsync();
            }

        }

        public override async Task<List<IEntity<long>>> GetUpdated()
        {
            if (SyncTimeInfo is null) throw new InvalidOperationException("You must call beginsync!!!");
            var filter = new Dictionary<string, string>
            {
                {"date_upd",  SyncTimeInfo.LastSyncTime.AddHours(_operationsHelper.HoursToAdjust).ToString(">[yyyy-MM-dd HH:mm:ss]")}
            };


            var (psThreads, error) = await MethodHelper.ExecuteMethodAsync(async () =>
            {
                return await _customerServiceThreadsFactory.GetByFilterAsync(filter, "", "");
            }, MethodHelper.NotRetryOnBadrequest);


            if (Processors.Count == 0)
            {

                await SaveThreads(psThreads);

                return null;
            }
            else
            {
                throw new NotSupportedException("Operación no compatible con procesamiento de cambios!!!");
            }
        }

        public override Task<List<IEntity<long>>> ResolveEntities(List<long> keys)
        {
            throw new NotSupportedException("Operación no compatible con resolución de entidades!!!");
        }

        public override async Task Work()
        {
            _status = OperationStatus.Working;
            while (!CancelTokenSource.IsCancellationRequested)
            {
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
                
                await Sleep(_timeToSleep);
            }
            _status = OperationStatus.Stopped;
        }

        public override Task<IEntity<long>> ResolveEntity(long key)
        {
            throw new NotImplementedException();
        }
    }
}
