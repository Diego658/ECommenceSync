using ECommenceSync.AutomatizerSQL.Helpers;
using ECommenceSync.AutomatizerSQL.Operations;
using ECommenceSync.Entities;
using ECommenceSync.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ECommenceSync.AutomatizerSQL
{
    public class AutomatizerSQL : IErp
    {
        public string Name => "AutomatizerSQL";
        private List<IOperation> _operations;
        private readonly IConfiguration configuration;
        private readonly IAutomatizerDataHelper dataHelper;
        private readonly IAutomatizerSQLOperationsHelper automatizerSQLOperationsHelper;
        readonly IDatabaseHelper<SqlConnection, int> _databaseHelper;
        readonly List<IProcessor> _processors;

        public IReadOnlyCollection<IOperation> Operations { get => _operations; set => _operations = value.Select(op => op).ToList(); }

        public AutomatizerSQL(IConfiguration configuration, 
            IDatabaseHelper<SqlConnection, int> databaseHelper,
            IAutomatizerDataHelper dataHelper, 
            IAutomatizerSQLOperationsHelper automatizerSQLOperationsHelper)
        {
            _databaseHelper = databaseHelper;
            _operations = new List<IOperation>();
            _processors = new List<IProcessor>();

            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.dataHelper = dataHelper ?? throw new ArgumentNullException(nameof(dataHelper));
            this.automatizerSQLOperationsHelper = automatizerSQLOperationsHelper ?? throw new ArgumentNullException(nameof(automatizerSQLOperationsHelper));
            Configure();
        }

        private void Configure()
        {
            _operations.Add(new BrandsOperation(dataHelper, automatizerSQLOperationsHelper));
            _operations.Add(new TagsOperation(dataHelper, automatizerSQLOperationsHelper));
            _operations.Add(new ProductCategoryOperation(dataHelper, automatizerSQLOperationsHelper));
            _operations.Add(new ProductOperation(dataHelper, automatizerSQLOperationsHelper));
            _operations.Add(new ImagesOperation(dataHelper, automatizerSQLOperationsHelper));
            _operations.Add(new ProductStocksOperation(dataHelper, automatizerSQLOperationsHelper));
            _operations.Add(new ProductSpecificPricesOperation(dataHelper, automatizerSQLOperationsHelper));
            _operations.Add(new ProductsAttributesFromFixedTablesOperation(dataHelper, automatizerSQLOperationsHelper));
            _operations.Add(new ProductsAttributesTermsFromFixedTablesOperation(dataHelper, automatizerSQLOperationsHelper));
            _operations.Add(new ProductVariationOperation(dataHelper, automatizerSQLOperationsHelper));
        }

        public void ConfigureOperations<TStoreKey>(IStore store) where TStoreKey : struct
        {
            foreach (var op in store.Operations.Where(op => op.Direction == OperationDirections.StoreToErp))
            {
                switch (op.Operation)
                {
                    case ECommenceSync.Operations.Customers:
                        if(op is ISourceOperation<TStoreKey, Customer<TStoreKey>> customerOp)
                        {
                            var processor = new GenericChangeProcessor<SqlConnection, TStoreKey, int, Customer<TStoreKey>>(
                                _databaseHelper, customerOp, int.MinValue, null, "StoreSync_PrestashopCambiosPendientes");  //PrestashopChangeProcessor<TErpKey, Brand<TErpKey>>(_databaseHelper, sourceOp);
                            var destination = new CustomersOperation<TStoreKey>( dataHelper, _databaseHelper);
                            processor.Destination = destination;
                            customerOp.AddChangeProcessor(processor);
                            _operations.Add(destination);
                            _processors.Add(processor);
                            //processor.Destination = destination;
                            //sourceOp.AddChangeProcessor(processor);
                            //operations.Add(destination);
                            //processors.Add(processor);
                        }
                        break;
                    default:
                        break;
                }
            }
        }


        public  Task Start()
        {
            foreach (var op in _operations)
            {
                op.Start();
            }
            foreach (var op in _processors)
            {
                op.Start();
            }
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            throw new NotImplementedException();
        }
    }
}
