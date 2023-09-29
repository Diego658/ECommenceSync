using ECommenceSync.Interfaces;
using ECommenceSync.Prestashop.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ECommenceSync.Prestashop.Operations
{
    internal class CronRequester : PrestashopOperation<IEntity<long>>
    {
        OperationStatus _status;
        readonly int _timeToSleep;
        public override ECommenceSync.Operations Operation =>  ECommenceSync.Operations.CronRequest;

        public override OperationModes Mode =>  OperationModes.Automatic;

        public override OperationDirections Direction =>  OperationDirections.StoreToStore ;

        public override OperationStatus Status => _status;

        private DateTime lastExecutedTask;
        private readonly IPrestashopOperationsHelper _operationsHelper;

        internal CronRequester(IPrestashopDatabaseHelper dataHelper, IPrestashopOperationsHelper operationsHelper)
        {
            DatabaseHelper = dataHelper;
            _status = OperationStatus.Created;
            _timeToSleep = operationsHelper.GetSearchTime(Operation);
            lastExecutedTask = DateTime.Now.Date.AddDays(-1);
            _operationsHelper = operationsHelper;
        }


        public override Dictionary<long, long> GetHierarchy()
        {
            throw new NotImplementedException();
        }

        public override Task<List<IEntity<long>>> GetUpdated()
        {
            throw new NotImplementedException();
        }

        public override Task<List<IEntity<long>>> ResolveEntities(List<long> keys)
        {
            throw new NotImplementedException();
        }

        public override Task<IEntity<long>> ResolveEntity(long key)
        {
            throw new NotImplementedException();
        }

        public override async Task Work()
        {
            _status = OperationStatus.Working;
            while (!CancelTokenSource.IsCancellationRequested)
            {
                await BeginSync();
                try
                {
                    if(DateTime.Now.Subtract(lastExecutedTask) > TimeSpan.FromHours(8))
                    {
                        var urls = _operationsHelper.Configuration.GetSection("CronUrls").GetChildren().ToList();
                        using var httpClient = new HttpClient();
                        httpClient.Timeout = TimeSpan.FromMinutes(5);
                        foreach (var url in urls)
                        {
                            try
                            {
                                var response = await httpClient.GetAsync(url.Value, CancelTokenSource.Token);

                            }
                            catch (Exception)
                            {
                            }
                            

                        }
                        lastExecutedTask = DateTime.Now;

                    }
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
    }
}
