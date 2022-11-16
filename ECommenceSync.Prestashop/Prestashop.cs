using ECommenceSync.Entities;
using ECommenceSync.Interfaces;
using ECommenceSync.Prestashop.Helpers;
using ECommenceSync.Prestashop.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommenceSync.Prestashop
{
    public class Prestashop : IStore
    {
        //private readonly IConfiguration configuration;
        private readonly IErp _erp;
        private readonly IPrestashopDatabaseHelper _databaseHelper;

        public string Name => "Prestashop_1_7";

        public IReadOnlyCollection<IOperation> Operations { get => operations; }
        public IReadOnlyCollection<IProcessor> Processors { get => processors; }

        readonly List<IOperation> operations;
        readonly List<IProcessor> processors;
        readonly IPrestashopOperationsHelper _operationsHelper;

        public Prestashop(IErp erp, IStoresCollection storesCollection, IPrestashopDatabaseHelper databaseHelper, IPrestashopOperationsHelper operationsHelper)
        {
            _operationsHelper = operationsHelper ?? throw new ArgumentNullException(nameof(operationsHelper));
            if (storesCollection is null)
            {
                throw new ArgumentNullException(nameof(storesCollection));
            }

            //this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _erp = erp ?? throw new ArgumentNullException(nameof(erp));
            _databaseHelper = databaseHelper ?? throw new ArgumentNullException(nameof(databaseHelper));
            operations = new List<IOperation>();
            processors = new List<IProcessor>();
            storesCollection.AddStore(this);
            Configure();
        }

        private void Configure()
        {
            operations.Add(new CronRequester(_databaseHelper, _operationsHelper));
            operations.Add(new CustomersOperation(_databaseHelper, _operationsHelper));
            operations.Add(new CarriersOperation(_databaseHelper, _operationsHelper));
            operations.Add(new OrdersStatesOperation(_databaseHelper, _operationsHelper));
            operations.Add(new OrdersOperation(_databaseHelper, _operationsHelper));
            operations.Add(new AddressesOperation(_databaseHelper, _operationsHelper));
            operations.Add(new CustomerServiceThreadsOperation(_databaseHelper, _operationsHelper));
        }

        public void ConfigureOperations<TErpKey>() where TErpKey : struct
        {

            //Configuramos operaciones que comienzan en el erp y terminan en la tienda
            foreach (var op in _erp.Operations.Where(op => op.Direction == OperationDirections.ErpToStore))
            {
                switch (op.Operation)
                {
                    case ECommenceSync.Operations.Brands:
                        if (op is ISourceOperation<TErpKey, Brand<TErpKey>> sourceOp)
                        {
                            var processor = new PrestashopChangeProcessor<TErpKey, Brand<TErpKey>>(_databaseHelper, sourceOp, _operationsHelper.Configuration);
                            var destination = new BrandsOperation<TErpKey>(_databaseHelper);
                            processor.Destination = destination;
                            sourceOp.AddChangeProcessor(processor);
                            operations.Add(destination);
                            processors.Add(processor);
                        }
                        break;
                    case ECommenceSync.Operations.Tags:
                        if (op is ISourceOperation<TErpKey, Tag<TErpKey>> tagOp)
                        {
                            var processor = new PrestashopChangeProcessor<TErpKey, Tag<TErpKey>>(_databaseHelper, tagOp, _operationsHelper.Configuration);
                            var destination = new TagsOperation<TErpKey>(_databaseHelper);
                            processor.Destination = destination;
                            tagOp.AddChangeProcessor(processor);
                            operations.Add(destination);
                            processors.Add(processor);
                        }
                        break;
                    case ECommenceSync.Operations.ProductsCategories:
                        if (op is ISourceOperation<TErpKey, ProductCategory<TErpKey>> catOp)
                        {
                            var processor = new PrestashopChangeProcessor<TErpKey, ProductCategory<TErpKey>>(_databaseHelper, catOp, _operationsHelper.Configuration);
                            var destination = new ProductCategoryOperation<TErpKey>(_databaseHelper);
                            processor.Destination = destination;
                            catOp.AddChangeProcessor(processor);
                            operations.Add(destination);
                            processors.Add(processor);
                        }
                        break;
                    case ECommenceSync.Operations.Products:
                        if (op is ISourceOperation<TErpKey, Product<TErpKey>> proOp)
                        {
                            var processor = new PrestashopChangeProcessor<TErpKey, Product<TErpKey>>(_databaseHelper, proOp, _operationsHelper.Configuration);
                            var destination = new ProductOperation<TErpKey>(_databaseHelper, proOp.GetHierarchy());
                            processor.Destination = destination;
                            proOp.AddChangeProcessor(processor);
                            operations.Add(destination);
                            processors.Add(processor);
                        }
                        break;
                    case ECommenceSync.Operations.ProductImages:
                        if (op is ISourceOperation<TErpKey, EntityImage<TErpKey>> imgOp)
                        {
                            var processor = new PrestashopChangeProcessor<TErpKey, EntityImage<TErpKey>>(_databaseHelper, imgOp, _operationsHelper.Configuration);
                            var destination = new ImagesOperation<TErpKey>(_databaseHelper);
                            processor.Destination = destination;
                            imgOp.AddChangeProcessor(processor);
                            operations.Add(destination);
                            processors.Add(processor);
                        }
                        break;
                    case ECommenceSync.Operations.ProductStocks:
                        if (op is ISourceOperation<TErpKey, ProductStock<TErpKey>> stockOp)
                        {
                            var processor = new PrestashopChangeProcessor<TErpKey, ProductStock<TErpKey>>(_databaseHelper, stockOp, _operationsHelper.Configuration);
                            var destination = new ProductStocksOperation<TErpKey>(_databaseHelper);
                            processor.Destination = destination;
                            stockOp.AddChangeProcessor(processor);
                            operations.Add(destination);
                            processors.Add(processor);
                        }
                        break;
                    case ECommenceSync.Operations.ProductPrices:
                        if (op is ISourceOperation<TErpKey, ProductPrice<TErpKey>> priceOp)
                        {
                            if (priceOp is ISpecificPricesOperation<TErpKey> sPriceOp)
                            {
                                var processor = new PrestashopChangeProcessor<TErpKey, ProductPrice<TErpKey>>(_databaseHelper, priceOp, _operationsHelper.Configuration);
                                var destination = new ProductSpecificPricesOperation<TErpKey>(_databaseHelper, _operationsHelper, sPriceOp.GetPricesToSync());
                                processor.Destination = destination;
                                priceOp.AddChangeProcessor(processor);
                                operations.Add(destination);
                                processors.Add(processor);
                            }

                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public Task Start()
        {
            foreach (var op in Operations.Where(op => op.Mode == OperationModes.Automatic))
            {
                op.Start();
            }
            foreach (var proc in Processors)
            {
                proc.Start();
            }
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            throw new NotImplementedException();
        }
    }
}
