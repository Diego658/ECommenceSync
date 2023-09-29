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
    public class AddressesOperation : PrestashopOperation<IEntity<long>>
    {
        OperationStatus _status;
        readonly string _tableName;
        readonly IPrestashopOperationsHelper _operationsHelper;
        readonly AddressFactory _addressFactory;
        readonly int _timeToSleep;
        public override ECommenceSync.Operations Operation => ECommenceSync.Operations.Addresses;

        public override OperationModes Mode => OperationModes.Automatic;

        public override OperationDirections Direction => OperationDirections.StoreToStore;

        public override OperationStatus Status => _status;


        public AddressesOperation(IPrestashopDatabaseHelper dataHelper, IPrestashopOperationsHelper operationsHelper, string tableName = "StoreSync_Addresses_Prestashop")
        {
            _tableName = tableName;
            _operationsHelper = operationsHelper;
            DatabaseHelper = dataHelper;
            _status = OperationStatus.Created;
            _addressFactory = new AddressFactory(dataHelper.ApiUrl, dataHelper.ApiSecret, "");

            _timeToSleep = operationsHelper.GetSearchTime(Operation);
        }

        public override Dictionary<long, long> GetHierarchy()
        {
            throw new NotImplementedException();
        }

        public override async Task<List<IEntity<long>>> GetUpdated()
        {
            if (SyncTimeInfo is null) throw new InvalidOperationException("You must call beginsync!!!");

            var filter = new Dictionary<string, string>
            {
                {"date_upd",  SyncTimeInfo.LastSyncTime.AddHours(_operationsHelper.HoursToAdjust).ToString(">[yyyy-MM-dd HH:mm:ss]")}
            };

            var (psAddresses, error) = await MethodHelper.ExecuteMethodAsync(async () =>
            {
                return await _addressFactory.GetByFilterAsync(filter, "", "");
            }, MethodHelper.NotRetryOnBadrequest);


            if (Processors.Count == 0)
            {
                await SaveAddresses(psAddresses);
                return null;
            }
            else
            {
                throw new NotSupportedException("Operación no compatible con procesamiento de cambios!!!");
            }
        }
        async Task SaveAddresses(List<address> psAddresses)
        {

            using var conex = DatabaseHelper.GetConnection();
            await conex.OpenAsync();
            using var tran = await conex.BeginTransactionAsync();
            foreach (var address in psAddresses)
            {
                await conex.ExecuteAsync($"DELETE FROM {_tableName} WHERE id = @id", address, tran);
                await conex.ExecuteAsync(@"INSERT INTO StoreSync_Addresses_Prestashop
                         (Id, deleted, dni, phone_mobile, phone, other, city, postcode, address2, address1, vat_number, firstname, lastname, company, alias, id_state, id_country, id_warehouse, id_supplier, id_manufacturer, id_customer)
                    VALUES (@Id, @deleted, @dni, @phone_mobile, @phone, @other, @city, @postcode, @address2, @address1, @vat_number, @firstname, @lastname, @company, @alias, @id_state, @id_country, @id_warehouse, @id_supplier, @id_manufacturer, @id_customer)",
                     address, tran);


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
            await Sleep(_timeToSleep);
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
