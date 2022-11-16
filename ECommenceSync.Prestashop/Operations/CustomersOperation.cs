using Bukimedia.PrestaSharp.Factories;
using ECommenceSync.Entities;
using ECommenceSync.Prestashop.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommenceSync.Prestashop.Operations
{
    public class CustomersOperation : PrestashopOperation<Customer<long>>
    {

        readonly IPrestashopOperationsHelper _operationsHelper;
        OperationStatus _status;
        readonly int _timeToSleep;
        readonly CustomerFactory _customersFactory;
        readonly AddressFactory _addresFactory;


        public override ECommenceSync.Operations Operation => ECommenceSync.Operations.Customers;

        public override OperationModes Mode => OperationModes.Automatic;

        public override OperationDirections Direction => OperationDirections.StoreToErp;

        public override OperationStatus Status => _status;


        public CustomersOperation(IPrestashopDatabaseHelper dataHelper, IPrestashopOperationsHelper operationsHelper)
        {
            _operationsHelper = operationsHelper;
            DatabaseHelper = dataHelper;
            _timeToSleep = operationsHelper.GetSearchTime(Operation);
            _status = OperationStatus.Created;
            _customersFactory = new CustomerFactory(dataHelper.ApiUrl, dataHelper.ApiSecret, "");
            _addresFactory = new AddressFactory(dataHelper.ApiUrl, dataHelper.ApiSecret, "");
            
        }

        public override Dictionary<long, long> GetHierarchy()
        {
            throw new NotImplementedException();
        }

        public override async Task<List<Customer<long>>> GetUpdated()
        {
            if (SyncTimeInfo is null) throw new InvalidOperationException("You must call beginsync!!!");
            var filter = new Dictionary<string, string>
            {
                {"date_upd",  SyncTimeInfo.LastSyncTime.AddHours(_operationsHelper.HoursToAdjust) .ToString(">[yyyy-MM-dd HH:mm:ss]")}
            };

            var (psCustomers, error) = await MethodHelper.ExecuteMethodAsync(async () =>
            {
                return await _customersFactory.GetByFilterAsync(filter, "", "");
            }, MethodHelper.NotRetryOnBadrequest);

            if (error is null)
            {
                var updated = new List<Customer<long>>(psCustomers.Count);
                foreach (var psCustomer in psCustomers)
                {
                    var customer = new Customer<long>
                    {
                        Id = psCustomer.id.Value,
                        Firstname = psCustomer.firstname,
                        LastName = psCustomer.lastname,
                        Name = $"{psCustomer.lastname} {psCustomer.firstname}",
                        Email = psCustomer.email,
                        Active = Convert.ToBoolean(psCustomer.active),
                        Deleted = Convert.ToBoolean(psCustomer.deleted),
                        IsGuest = Convert.ToBoolean(psCustomer.is_guest),
                        MaxPaymentDays = Convert.ToInt32(psCustomer.max_payment_days),
                        Newsletter = Convert.ToBoolean(psCustomer.newsletter),
                        UserName = "",
                        IdGender = psCustomer.id_gender ?? 0
                    };
                    updated.Add(customer);
                }
                return updated;
            }
            else
            {
                return new List<Customer<long>>();
            }
        }

        public override async Task<List<Customer<long>>> ResolveEntities(List<long> keys)
        {
            var filter = new Dictionary<string, string>
            {
                {"id", string.Join(',', keys.OrderBy(x=> x).Select(x=> x.ToString())  )}
            };

            var (psCustomers, error) = await MethodHelper.ExecuteMethodAsync(async () =>
            {
                return await _customersFactory.GetByFilterAsync(filter, "", "");
            }, MethodHelper.NotRetryOnBadrequest);

            if (error is null)
            {
                var updated = new List<Customer<long>>(psCustomers.Count);
                foreach (var psCustomer in psCustomers)
                {
                    var customer = new Customer<long>
                    {
                        Id = psCustomer.id.Value,
                        Firstname = psCustomer.firstname,
                        LastName = psCustomer.lastname,
                        Name = $"{psCustomer.lastname} {psCustomer.firstname}",
                        Email = psCustomer.email,
                        Active = Convert.ToBoolean(psCustomer.active),
                        Deleted = Convert.ToBoolean(psCustomer.deleted),
                        IsGuest = Convert.ToBoolean(psCustomer.is_guest),
                        MaxPaymentDays = Convert.ToInt32(psCustomer.max_payment_days),
                        Newsletter = Convert.ToBoolean(psCustomer.newsletter),
                        UserName = "",
                        IdGender = psCustomer.id_gender.HasValue ? psCustomer.id_gender.Value : 0
                    };

                    updated.Add(customer);
                }
                return updated;
            }
            else
            {
                throw error;
            }


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

        public override Task<Customer<long>> ResolveEntity(long key)
        {
            throw new NotImplementedException();
        }
    }
}
