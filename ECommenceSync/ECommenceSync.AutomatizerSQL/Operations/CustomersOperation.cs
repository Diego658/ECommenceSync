using Dapper;
using ECommenceSync.AutomatizerSQL.Helpers;
using ECommenceSync.Entities;
using ECommenceSync.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace ECommenceSync.AutomatizerSQL.Operations
{
    public class CustomersOperation<TExternalKey> : IDestinationOperation<TExternalKey, Customer<TExternalKey>>
        where TExternalKey : struct
    {
        readonly ConcurrentQueue<Customer<TExternalKey>> _workQueue;
        readonly IAutomatizerDataHelper _dataHelper;
        CancellationTokenSource _taskCancelTokenSource;
        CancellationTokenSource _cancelTokenSource;
        Task _taskProcessor;
        ConcurrentDictionary<TExternalKey, int> _links;
        readonly IDatabaseHelper<SqlConnection, int> _databaseHelper;
        public Action<Customer<TExternalKey>, SyncResult, Exception> OnSynchronized { get; set; }

        public ECommenceSync.Operations Operation => ECommenceSync.Operations.Customers;

        public OperationModes Mode => OperationModes.Automatic;

        public OperationDirections Direction => OperationDirections.StoreToErp;

        public Guid Identifier => Guid.NewGuid();

        public OperationStatus Status { get; set; }


        public CustomersOperation(IAutomatizerDataHelper dataHelper, IDatabaseHelper<SqlConnection,int> databaseHelper)
        {
            _databaseHelper = databaseHelper;
            Status = OperationStatus.Created;
            _workQueue = new ConcurrentQueue<Customer<TExternalKey>>();
            _dataHelper = dataHelper ?? throw new ArgumentNullException(nameof(dataHelper));
        }

        public void AddWork(List<Customer<TExternalKey>> values)
        {
            foreach (var item in values)
            {
                _workQueue.Enqueue(item);
            }
        }

        public void Start()
        {
            _taskCancelTokenSource = new CancellationTokenSource();
            _cancelTokenSource = new CancellationTokenSource();
            _taskProcessor = new Task(async () => await Work(), _taskCancelTokenSource.Token, TaskCreationOptions.LongRunning);
            _taskProcessor.Start();
        }

        public void Stop(TimeSpan timeOut)
        {
            throw new NotImplementedException();
        }

        public async Task Work()
        {
            if (!await LoadLinks())
            {
                return;
            }

            Status = OperationStatus.Working;
            while (!_cancelTokenSource.IsCancellationRequested)
            {
                while (_workQueue.TryDequeue(out var tag))
                {
                    var (result, ex) = await SyncCustomer(tag);
                    OnSynchronized(tag, result, ex);
                }
                await Task.Delay(1000);
            }
            Status = OperationStatus.Stopped;
        }

        private async Task<bool> LoadLinks()
        {
            _links = await _databaseHelper.GetCustomersLinks<TExternalKey>();
            return true;
            //using var conex = (SqlConnection)_dataHelper.GetConnection();
            //using var cmd = conex.CreateCommand();
            //cmd.CommandText = "SELECT Id, PrestashopId FROM StoreSync_Customers_Prestashop WHERE(EmpCod = @EmpCod)";
            //cmd.Parameters.AddWithValue("EmpCod", _dataHelper.CodigoEmpresa);
            //await conex.OpenAsync();
            //using var reader = cmd.ExecuteReader();
            //_links = new Dictionary<TExternalKey, int>();
            //while (await reader.ReadAsync())
            //{
            //    _links.Add(reader.GetFieldValue<TExternalKey>(1), reader.GetInt32(0));

            //}

            //return true;
        }
        async Task<Tuple<SyncResult, Exception>> SyncCustomer(Customer<TExternalKey> customer)
        {
            var prestashopId = _links.ContainsKey(customer.Id) ? _links[customer.Id] : 0;
            Tuple<SyncResult, Exception> resultado;
            if (prestashopId == 0)
            {
                resultado = await AddCustomer(customer);
            }
            else
            {
                resultado = await UpdateCustomer(customer, prestashopId);
            }
            return resultado;
        }
        async Task<Tuple<SyncResult, Exception>> AddCustomer(Customer<TExternalKey> customer)
        {
            try
            {
                using var conex = (SqlConnection)_dataHelper.GetConnection();
                await conex.ExecuteAsync(@"INSERT INTO StoreSync_Customers_Prestashop(EmpCod, PrestashopId, Firstname, LastName, Email, IdGender, Newsletter, Active, IsGuest) VALUES 
                (@EmpCod, @PrestashopId, @Firstname, @LastName, @Email, @IdGender, @Newsletter, @Active, @IsGuest)",
                    new
                    {
                        EmpCod = _dataHelper.CodigoEmpresa,
                        PrestashopId = customer.Id,
                        customer.Firstname,
                        customer.LastName,
                        customer.Email,
                        customer.IdGender,
                        customer.Newsletter,
                        customer.Active,
                        customer.IsGuest
                    });
                var id = await conex.ExecuteScalarAsync<int>("SELECT Id FROM StoreSync_Customers_Prestashop WHERE PrestashopId = @PrestashopId", new
                {
                    PrestashopId = customer.Id
                });
                _links.TryAdd(customer.Id, id);
                return Tuple.Create(SyncResult.Created, default(Exception));
            }
            catch (Exception  ex)
            {

                return Tuple.Create(SyncResult.Error, ex);
            }
            
        }
        async Task<Tuple<SyncResult, Exception>> UpdateCustomer(Customer<TExternalKey> customer, long idPrestashop)
        {
            try
            {
                using var conex = (SqlConnection)_dataHelper.GetConnection();
                await conex.ExecuteAsync(@"UPDATE StoreSync_Customers_Prestashop SET Firstname = @Firstname, LastName = @LastName,
                    Email = @Email, IdGender = @IdGender, Newsletter = @Newsletter, Active = @Active, IsGuest = @Active WHERE PrestashopId = @PrestashopId",
                    new
                    {
                        PrestashopId = customer.Id,
                        customer.Firstname,
                        customer.LastName,
                        customer.Email,
                        customer.IdGender,
                        customer.Newsletter,
                        customer.Active,
                        customer.IsGuest
                    });
                return Tuple.Create(SyncResult.Updated, default(Exception));
            }
            catch (Exception ex)
            {

                return Tuple.Create(SyncResult.Error, ex);
            }
        }




    }
}
