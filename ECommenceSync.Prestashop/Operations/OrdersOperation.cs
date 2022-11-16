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
    public class OrdersOperation : PrestashopOperation<IEntity<long>>
    {
        readonly string _carriersTableName;
        readonly string _detailsTableName;
        //readonly HashSet<long> _links;
        readonly IPrestashopOperationsHelper _operationsHelper;
        readonly OrderCarrierFactory _orderCarrierFactory;
        readonly OrderDetailFactory _orderDetailFactory;
        readonly OrderFactory _orderFactory;
        readonly OrderHistoryFactory _orderHistoryFactory;
        readonly OrderPaymentFactory _orderPaymentsFactory;
        readonly string _paymentsTableName;
        OperationStatus _status;
        readonly string _tableName;
        readonly int _timeToSleep;

        public OrdersOperation(IPrestashopDatabaseHelper dataHelper, IPrestashopOperationsHelper operationsHelper,
            string tableName = "StoreSync_Orders_Prestashop",
            string detailsTableName = "StoreSync_OrdersDetails_Prestashop",
            string paymentsTableName = "StoreSync_OrdersPayments_Prestashop",
            string carriersTableName = "StoreSync_OrdersCarriers_Prestashop")
        {
            this._carriersTableName = carriersTableName;
            this._paymentsTableName = paymentsTableName;
            this._detailsTableName = detailsTableName;
            _tableName = tableName;
            _operationsHelper = operationsHelper;
            DatabaseHelper = dataHelper;
            _status = OperationStatus.Created;
            _orderFactory = new OrderFactory(dataHelper.ApiUrl, dataHelper.ApiSecret, string.Empty);
            _orderDetailFactory = new OrderDetailFactory(dataHelper.ApiUrl, dataHelper.ApiSecret, string.Empty);
            _orderCarrierFactory = new OrderCarrierFactory(dataHelper.ApiUrl, dataHelper.ApiSecret, string.Empty);
            _orderPaymentsFactory = new OrderPaymentFactory(dataHelper.ApiUrl, dataHelper.ApiSecret, string.Empty);
            _orderHistoryFactory = new OrderHistoryFactory(dataHelper.ApiUrl, dataHelper.ApiSecret, string.Empty);
            _timeToSleep = operationsHelper.GetSearchTime(Operation);
        }


        async Task SaveOrders(List<order> psOrdes)
        {
            foreach (var order in psOrdes)
            {


                var filtroDetalles = new Dictionary<string, string>();
                //filtroDetalles.Add("id", string.Join(',', order.associations.order_rows.Select(x => x.id.ToString())));
                filtroDetalles.Add("id_order", order.id.ToString());
                var detalles = await _orderDetailFactory.GetByFilterAsync(filtroDetalles, "", "");
                //filtroDetalles.Clear();
                //filtroDetalles.Add("id_order", order.id.ToString());
                var carriers = await _orderCarrierFactory.GetByFilterAsync(filtroDetalles, "", "");
                var history = await _orderHistoryFactory.GetByFilterAsync(filtroDetalles, "", "");
                filtroDetalles.Clear();
                filtroDetalles.Add("order_reference", order.reference);
                var payments = await _orderPaymentsFactory.GetByFilterAsync(filtroDetalles, "", "");

                using var conex = DatabaseHelper.GetConnection();
                await conex.OpenAsync();
                using var tran = await conex.BeginTransactionAsync();
                await conex.ExecuteAsync($"DELETE FROM {_tableName} WHERE id = @id", new { order.id }, tran);
                await conex.ExecuteAsync($"DELETE FROM {_detailsTableName} WHERE id_order = @id", new { order.id }, tran);
                await conex.ExecuteAsync($"DELETE FROM {_carriersTableName} WHERE id_order = @id", new { order.id }, tran);
                await conex.ExecuteAsync($"DELETE FROM {_paymentsTableName} WHERE order_reference = @reference", new { order.reference }, tran);

                await conex.ExecuteAsync(@$"INSERT INTO {_tableName}
                         (id, id_address_delivery, id_address_invoice, id_cart, id_currency, id_lang, id_customer, 
                        current_state, module, invoice_number, invoice_date, delivery_number, delivery_date, valid, 
                        date_add, date_upd, shipping_number, secure_key, payment, recyclable, gift, gift_message, 
                        total_discounts, total_discounts_tax_incl, total_discounts_tax_excl, total_paid, total_paid_tax_incl, 
                        total_paid_tax_excl, total_paid_real, total_products, total_products_wt, total_shipping, 
                        total_shipping_tax_incl, total_shipping_tax_excl, carrier_tax_rate, total_wrapping, 
                        total_wrapping_tax_incl, total_wrapping_tax_excl, round_mode, round_type, conversion_rate, reference)
                        VALUES       (@id, @id_address_delivery, @id_address_invoice, @id_cart, @id_currency, @id_lang, @id_customer, 
                        @current_state, @module, @invoice_number, @invoice_date, @delivery_number, @delivery_date, 
                        @valid, @date_add, @date_upd, @shipping_number, @secure_key, @payment, @recyclable, 
                        @gift, @gift_message, @total_discounts, @total_discounts_tax_incl, @total_discounts_tax_excl, 
                        @total_paid, @total_paid_tax_incl, @total_paid_tax_excl, @total_paid_real, @total_products, 
                        @total_products_wt, @total_shipping, @total_shipping_tax_incl, @total_shipping_tax_excl, 
                        @carrier_tax_rate, @total_wrapping, @total_wrapping_tax_incl, @total_wrapping_tax_excl, 
                        @round_mode, @round_type, @conversion_rate, @reference)",
                    new
                    {
                        order.id,
                        order.id_address_delivery,
                        order.id_address_invoice,
                        order.id_cart,
                        order.id_currency,
                        order.id_lang,
                        order.id_customer,
                        order.current_state,
                        order.module,
                        order.invoice_number,
                        invoice_date = DateTime.TryParse(order.invoice_date, out var invoice_date) ? invoice_date : default(DateTime?),
                        order.delivery_number,
                        delivery_date = DateTime.TryParse(order.delivery_date, out var deliveryDate) ? deliveryDate : default(DateTime?),
                        order.valid,
                        order.date_add,
                        order.date_upd,
                        order.shipping_number,
                        order.secure_key,
                        order.payment,
                        order.recyclable,
                        order.gift,
                        order.gift_message,
                        order.total_discounts,
                        order.total_discounts_tax_incl,
                        order.total_discounts_tax_excl,
                        order.total_paid,
                        order.total_paid_tax_incl,
                        order.total_paid_tax_excl,
                        order.total_paid_real,
                        order.total_products,
                        order.total_products_wt,
                        order.total_shipping,
                        order.total_shipping_tax_incl,
                        order.total_shipping_tax_excl,
                        order.carrier_tax_rate,
                        order.total_wrapping,
                        order.total_wrapping_tax_incl,
                        order.total_wrapping_tax_excl,
                        round_mode = 1,
                        round_type = 1,
                        order.conversion_rate,
                        order.reference
                    }, tran);


                foreach (order_detail detalle in detalles)
                {
                    if (DateTime.TryParse(detalle.download_deadline, out var download_deadline))
                    {
                        detalle.download_deadline = download_deadline.ToString();
                    }
                    else
                    {
                        detalle.download_deadline = null;
                    }
                    await conex.ExecuteAsync(@$"INSERT INTO {_detailsTableName}
                         (id, id_order, product_id, product_attribute_id, product_quantity_reinjected, group_reduction, discount_quantity_applied, download_hash, download_deadline, id_order_invoice, id_warehouse, id_customization, product_name, 
                         product_quantity, product_quantity_in_stock, product_quantity_return, product_quantity_refunded, product_price, reduction_percent, reduction_amount, reduction_amount_tax_incl, reduction_amount_tax_excl, 
                         product_quantity_discount, product_ean13, product_isbn, product_upc, product_reference, product_supplier_reference, product_weight, tax_computation_method, id_tax_rules_group, ecotax, ecotax_tax_rate, download_nb, 
                         unit_price_tax_incl, unit_price_tax_excl, total_price_tax_incl, total_price_tax_excl, total_shipping_price_tax_excl, total_shipping_price_tax_incl, purchase_supplier_price, original_product_price, original_wholesale_price)
                         VALUES (@id, @id_order, @product_id, @product_attribute_id, @product_quantity_reinjected, @group_reduction, @discount_quantity_applied, 
                            @download_hash, @download_deadline, @id_order_invoice, @id_warehouse, 0, @product_name, 
                            @product_quantity, @product_quantity_in_stock, @product_quantity_return, @product_quantity_refunded, 
                            @product_price, @reduction_percent, @reduction_amount, @reduction_amount_tax_incl, @reduction_amount_tax_excl, 
                            @product_quantity_discount, @product_ean13, 0, @product_upc, @product_reference, 
                            @product_supplier_reference, @product_weight, @tax_computation_method, @id_tax_rules_group, @ecotax, @ecotax_tax_rate, 
                            @download_nb, @unit_price_tax_incl, @unit_price_tax_excl, @total_price_tax_incl, @total_price_tax_excl, 
                            @total_shipping_price_tax_excl, @total_shipping_price_tax_incl, @purchase_supplier_price, @original_product_price, 
                            @original_wholesale_price)",
                    detalle, tran);
                }

                foreach (order_carrier carrier in carriers)
                {
                    await conex.ExecuteAsync(@$"INSERT INTO {_carriersTableName}
                         (id, id_order, id_carrier, id_order_invoice, weight, shipping_cost_tax_excl, shipping_cost_tax_incl, tracking_number)
                        VALUES (@id, @id_order, @id_carrier, @id_order_invoice, @weight, @shipping_cost_tax_excl, @shipping_cost_tax_incl, @tracking_number)",
                    new
                    {
                        carrier.id,
                        carrier.id_order,
                        carrier.id_carrier,
                        carrier.id_order_invoice,
                        carrier.weight,
                        carrier.shipping_cost_tax_excl,
                        carrier.shipping_cost_tax_incl,
                        carrier.tracking_number
                    }, tran);
                }


                foreach (var payment in payments)
                {
                    await conex.ExecuteAsync(@$"INSERT INTO {_paymentsTableName}
                         (id, order_reference, id_currency, amount, payment_method, conversion_rate, transaction_id, card_number, card_brand, card_expiration, card_holder)
                        VALUES (@id, @order_reference, @id_currency, @amount, @payment_method, @conversion_rate, @transaction_id, @card_number, 
                            @card_brand, @card_expiration, @card_holder)",
                        new
                        {
                            payment.id,
                            payment.order_reference,
                            payment.id_currency,
                            payment.amount,
                            payment.payment_method,
                            payment.conversion_rate,
                            payment.transaction_id,
                            payment.card_number,
                            payment.card_brand,
                            payment.card_expiration,
                            payment.card_holder
                        }, tran);
                }

                await tran.CommitAsync();

            }
        }

        public override Dictionary<long, long> GetHierarchy()
        {
            throw new NotImplementedException();
        }

        public override async Task<List<IEntity<long>>> GetUpdated()
        {
            if (SyncTimeInfo is null) throw new InvalidOperationException("You must call beginsync!!!");
            var (psOrdes, error) = await MethodHelper.ExecuteMethodAsync(async () =>
            {
                var filter = new Dictionary<string, string>
                {
                    {"date_upd",  SyncTimeInfo.LastSyncTime.AddHours(_operationsHelper.HoursToAdjust).ToString(">[yyyy-MM-dd HH:mm:ss]")}
                };
                return await _orderFactory.GetByFilterAsync(filter, "", "");
            }, MethodHelper.NotRetryOnBadrequest);

            if (Processors.Count == 0)
            {
                await SaveOrders(psOrdes);
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

        //async Task LoadLinks()
        //{
        //    var conex = DatabaseHelper.GetConnection();
        //    var keys = await conex.QueryAsync<long>($"SELECT  id FROM {_tableName}");
        //    _links = new HashSet<long>(keys);
        //}
        public override async Task Work()
        {
            _status = OperationStatus.Working;
            //await LoadLinks();
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

        public override OperationDirections Direction => OperationDirections.StoreToStore;

        public override OperationModes Mode => OperationModes.Automatic;
        public override ECommenceSync.Operations Operation => ECommenceSync.Operations.Orders;

        public override OperationStatus Status => _status;
    }


    //[Table("")]
    //public class PocoOrder : order
    //{
    //    [ExplicitKey]
    //    public new long id { get; set; }
    //    [Dapper.Contrib.Extensions.Computed]
    //    public new string associations { get; set;}
    //}

}
