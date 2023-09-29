using ECommenceSync.Entities;
using ECommenceSync.Interfaces;
using ECommenceSync.WooCommerce.Helpers;
using ECommenceSync.WooCommerce.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommenceSync.WooCommerce
{
    public class WooCommerce : IStore
    {
        private readonly IErp _erp;
        private readonly IWooCommerceDatabaseHelper _databaseHelper;
        readonly List<IOperation> _operations;
        readonly List<IProcessor> _processors;
        private readonly IWooCommerceOperationsHelper _operationsHelper;

        public string Name => "WooCommerce_5_4";

        public IReadOnlyCollection<IOperation> Operations => _operations;

        public IReadOnlyCollection<IProcessor> Processors => _processors;



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

        public WooCommerce(IErp erp, IStoresCollection storesCollection,
            IWooCommerceDatabaseHelper databaseHelper,
            IWooCommerceOperationsHelper operationsHelper)
        {
            _operationsHelper = operationsHelper ?? throw new ArgumentNullException(nameof(operationsHelper));
            if (storesCollection is null)
            {
                throw new ArgumentNullException(nameof(storesCollection));
            }
            _erp = erp ?? throw new ArgumentNullException(nameof(erp));
            _databaseHelper = databaseHelper ?? throw new ArgumentNullException(nameof(databaseHelper));
            _operations = new List<IOperation>();
            _processors = new List<IProcessor>();
            storesCollection.AddStore(this);
            Configure();
        }

        private void Configure()
        {

        }

        public void ConfigureOperations<TErpKey>() where TErpKey : struct
        {
            foreach (var op in _erp.Operations.Where(op => op.Direction == OperationDirections.ErpToStore))
            {
                switch (op.Operation)
                {
                    case ECommenceSync.Operations.ProductsCategories:
                        if (op is ISourceOperation<TErpKey, ProductCategory<TErpKey>> catOp)
                        {
                            var processor = new WooCommerceChangeProcessor<TErpKey, ProductCategory<TErpKey>>(_databaseHelper, catOp, _operationsHelper.Configuration);
                            var destination = new ProductCategoryOperation<TErpKey>(_databaseHelper, _operationsHelper);
                            processor.Destination = destination;
                            catOp.AddChangeProcessor(processor);
                            _operations.Add(destination);
                            _processors.Add(processor);
                        }
                        break;

                    case ECommenceSync.Operations.Products:
                        if (op is ISourceOperation<TErpKey, Product<TErpKey>> proOp)
                        {
                            var processor = new WooCommerceChangeProcessor<TErpKey, Product<TErpKey>>(_databaseHelper, proOp, _operationsHelper.Configuration);
                            var destination = new ProductOperation<TErpKey>(_databaseHelper, _operationsHelper, proOp.GetHierarchy());
                            processor.Destination = destination;
                            proOp.AddChangeProcessor(processor);
                            _operations.Add(destination);
                            _processors.Add(processor);
                        }
                        break;
                    case ECommenceSync.Operations.ProductImages:
                        if (op is ISourceOperation<TErpKey, EntityImage<TErpKey>> imgOp)
                        {
                            var processor = new WooCommerceChangeProcessor<TErpKey, EntityImage<TErpKey>>(_databaseHelper, imgOp, _operationsHelper.Configuration);
                            var destination = new ImagesOperation<TErpKey>(_databaseHelper);
                            processor.Destination = destination;
                            imgOp.AddChangeProcessor(processor);
                            _operations.Add(destination);
                            _processors.Add(processor);
                        }
                        break;
                    case ECommenceSync.Operations.Attributes:
                        if (op is ISourceOperation<TErpKey, ProductAttribute<TErpKey>> attOp)
                        {
                            var processor = new WooCommerceChangeProcessor<TErpKey, ProductAttribute<TErpKey>>(_databaseHelper, attOp, _operationsHelper.Configuration);
                            var destination = new ProductsAttributesOperation<TErpKey>(_databaseHelper);
                            processor.Destination = destination;
                            attOp.AddChangeProcessor(processor);
                            _operations.Add(destination);
                            _processors.Add(processor);
                        }
                        break;
                    case ECommenceSync.Operations.AttributesTerms:
                        if (op is ISourceOperation<TErpKey, ProductAttributeTerm<TErpKey>> attTermOp)
                        {
                            var processor = new WooCommerceChangeProcessor<TErpKey, ProductAttributeTerm<TErpKey>>(_databaseHelper, attTermOp, _operationsHelper.Configuration);
                            var destination = new ProductsAttributesTermsOperation<TErpKey>(_databaseHelper);
                            processor.Destination = destination;
                            attTermOp.AddChangeProcessor(processor);
                            _operations.Add(destination);
                            _processors.Add(processor);
                        }
                        break;
                    case ECommenceSync.Operations.ProductsVariations:
                        //Se requeire la operación de atributtos funcional en el erp para sincronziar variaciones pues se necesita resolver id de atributos en atributos.
                        //Y cada operacion tiene un método para resolver un key en una entidad de su tipo procesado.
                        var attOperation = _erp.Operations.FirstOrDefault(x => x.Direction == OperationDirections.ErpToStore && x.Operation == ECommenceSync.Operations.AttributesTerms);
                        if (attOperation != null && op is ISourceOperation<TErpKey, ProductVariant<TErpKey>> prdvariantsOp)
                        {
                            var processor = new WooCommerceChangeProcessor<TErpKey, ProductVariant<TErpKey>>(_databaseHelper, prdvariantsOp, _operationsHelper.Configuration);
                            var destination = new ProductVariationOperation<TErpKey>(_databaseHelper, _operationsHelper, prdvariantsOp.GetHierarchy(), (GenericSourceOperation<TErpKey, ProductAttributeTerm<TErpKey>>)attOperation);
                            processor.Destination = destination;
                            prdvariantsOp.AddChangeProcessor(processor);
                            _operations.Add(destination);
                            _processors.Add(processor);
                        }
                        break;
                    default:
                        break;
                }
            }
        }


    }
}
