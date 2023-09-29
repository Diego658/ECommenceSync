import React, { useState } from 'react'
import { LoadIndicator, Button, ScrollView } from 'devextreme-react';
import { useEffect } from 'react';
import { FieldGroup } from '../../../../components/fieldGroup/fieldGroup';
import { Field } from '../../../../components/field/field';
import { PrestashopService } from '../../../../_services/stores/prestashop-service';
import notify from 'devextreme/ui/notify';
import DataGrid, { Column, StateStoring, Export, ColumnChooser, ColumnFixing } from 'devextreme-react/data-grid';
import DataSource from 'devextreme/data/data_source';
import Form, { Tab, TabbedItem } from 'devextreme-react/form';
import PedidoPrestashopCustomerService from './pedido-customer-service';
import { BlobService } from '../../../../_services/blob.service';
import { CssLoadingIndicator } from '../../../../components/cssLoader/css-loader';
import { useCallback } from 'react';
import { Popup } from 'devextreme-react';
import CellIngresarSerieProductoPrestashop from './cell-series-producto';
import { ArbolInventarioItem } from '../../../../components/arbol-inventario/arbol-inventario-item';
import ButtonFacturarPedidoPrestashop from './button-facturar-pedido';
import PedidoRegistrarTracking from './pedido-registrar-tracking';
import PedidoCancelarPedido from './pedido-cancelar-pedido';

export default function PedidoPrestashop({ id, toogleZoom }) {
  const [gridHeigth, setgridHeigth] = useState(window.innerHeight - 100);
  const [loading, setloading] = useState(true);
  const [pedido, setPedido] = useState(null);
  const [invoiceAddress, setInvoiceAddress] = useState({});
  const [deliverAddress, setDeliverAddress] = useState({});
  const [carrier, setCarrier] = useState({});
  const [carrierInfo, setCarrierInfo] = useState({});
  const [orderCarrierHistory, setOrderCarrierHistory] = useState([]);
  const [customer, setCustomer] = useState(null);
  const [status, setStatus] = useState(null);
  const [detalles, setDetalles] = useState([]);
  //const [detallesSeries, setDetallesSeries] = useState([]);
  const [paymentInfo, setPaymentInfo] = useState({});
  const [error, setError] = useState({ isOk: true });
  const [confirmarTranferenciaVisible, setConfirmarTranferenciaVisible] = useState(false);
  const [estadoPagoAceptado, setestadoPagoAceptado] = useState(null);
  const [infoFactura, setInfoFactura] = useState(null);
  const [showRegistrarGuia, setshowRegistrarGuia] = useState(false);
  const [showCancelarPedido, setshowCancelarPedido] = useState(false);

  const cargarPedido = async (ordenId) => {
    try {
      const p = await PrestashopService.getOrden(ordenId);
      if (p) {
        const a1 = await PrestashopService.getAddres(p.id_address_invoice);
        setInvoiceAddress(a1);
        const a2 = await PrestashopService.getAddres(p.id_address_delivery);
        setDeliverAddress(a2);
        const c = await PrestashopService.getCustomer(p.id_customer);
        setCustomer(c);
        const e = await PrestashopService.getStatusOrden(p.current_state);
        setStatus(e);
        const d = await PrestashopService.getOrdenDetalles(ordenId);
        //const ds = await PrestashopService.getOrdenDetallesSeries(ordenId);
        //setDetallesSeries(ds);
        setDetalles(d);
        const carInfo = await PrestashopService.getOrdenCarrier(ordenId);
        setCarrierInfo(carInfo);
        const carHis = await PrestashopService.getOrderCarrierHistory(carInfo.id);
        setOrderCarrierHistory(carHis);
        const car = await PrestashopService.getCarrier(carInfo.id_carrier);
        setCarrier(car);
        const pay = await PrestashopService.getOrderPayment(p.reference);
        setPaymentInfo(pay);
        const pa = await PrestashopService.statuPayAcepted();
        setestadoPagoAceptado(pa);
        const inf = await PrestashopService.getInfoFacturacion(ordenId);
        setInfoFactura(inf);
        setPedido(p);
      }
    } catch (ex) {
      setError({ isOk: false, error: ex });
    }
    finally {
      setloading(false);
    }

  };

  const actualizarEstado = async () => {
    const e = await PrestashopService.getStatusOrden(pedido.current_state);
    setStatus(e);
    const inf = await PrestashopService.getInfoFacturacion(pedido.id);
    setInfoFactura(inf);
  }

  const gridDetallesSource = new DataSource({
    store: {
      key: 'id',
      type: 'array',
      data: detalles
    },
    sort: { getter: 'product_name', desc: false }
  });

  const confirmarPagoTranferencia = async () => {
    try {
      var res = await PrestashopService.confirmPaymentByTransfer(id);
      if (res.isOk) {
        const p = await PrestashopService.getOrden(id);
        const e = await PrestashopService.getStatusOrden(p.current_state);
        setStatus(e);
        setPedido(p);
        notify('Pago confirmado correctamente', 'success', 2500);
      } else {
        notify(res.message, 'error', 3500);
      }
    } catch (error) {

    }
    //await cargarPedido(id);
  }

  const confirmarEntrega = async () => {
    try {
      var res = await PrestashopService.confirmarEntrega(id);
      if (res.isOk) {
        const p = await PrestashopService.getOrden(id);
        const e = await PrestashopService.getStatusOrden(p.current_state);
        setStatus(e);
        setPedido(p);
        notify('Entrega confirmada correctamente', 'success', 2500);
      } else {
        notify('Error al confirmar entrega ' + res.message, 'error', 3500);
      }
    } catch (error) {
      notify(error, 'error', 3500);
    }
  }


  useEffect(() => {
    cargarPedido(id);
  }, [id]);

  if (loading) {
    return (<LoadIndicator />);
  }

  if (!error.isOk) {
    return <div className='errorContainer'> <p className='errroMessage'>{error.error}</p></div>
  }

  return (
    <div className='pedidoContainer'  >
      <div className='pedidoContent' >
        <FieldGroup>
          <Field label='#' size='xs1' >
            <span>{pedido.id}</span>
          </Field>
          <Field label='Estado'>
            <span >{status.name}</span>
          </Field>
          <Field label='Referencia' size='s1'>
            <span>{pedido.reference}</span>
          </Field>
          <Field label="Fecha" size='s2'>
            <span>{formateDate(pedido.date_add)}</span>
          </Field>
          <Field label='Cliente' >
            <span>{customer.Firstname + ' ' + customer.LastName}</span>
          </Field>
          <Field label='Email'>
            <span>{customer.Email}</span>
          </Field>
        </FieldGroup>
        <Form >
          <TabbedItem>
            <Tab title='Detalles'>
              <DataGrid
                dataSource={gridDetallesSource}
                wordWrapEnabled={true}
                focusedRowEnabled={true}
                allowColumnReordering={true}
                allowColumnResizing={true}
                showBorders={true}
                width={window.innerWidth - 80}
                height={window.innerHeight - 250}
              >
                <ColumnChooser enabled={true} />
                <ColumnFixing enabled={true} />
                <Export enabled={true} fileName={`Orden ${pedido.reference} - Detalles`} allowExportSelectedData={true} />
                <StateStoring enabled={true} type="localStorage" storageKey="pedido-detalles-grid" />
                <Column caption='id' width={50} dataField='id' visible={false} allowExporting={false} allowFiltering={false} >
                </Column>
                <Column caption='Producto'>
                  <Column
                    caption='Series'
                    allowSorting={false}
                    allowHiding={false}
                    cellRender={pedido.current_state === 2 ? cellSeriesRender : cellSeriesRenderNotEditable}
                  />
                  <Column dataField="Picture"
                    allowSorting={false}
                    cellRender={cellProductPictureRender}
                  />
                  <Column caption='Referencia' dataField='product_reference' />
                  <Column caption='Nombre' dataField='product_name' />
                </Column>
                <Column caption='Valores'>
                  <Column dataField='product_weight' caption='Peso' />
                  <Column dataField='product_quantity' caption='Cantidad' />
                  <Column dataField='saldoActual' caption='Disponible' />
                  <Column dataField='product_price' caption='Precio' />
                  <Column caption='Subtotal' calculateCellValue={calculateSubtotalCellValue} alignment='right' />
                  <Column caption='Descuentos' calculateCellValue={calculateDescuentosCellValue} alignment='right' />
                  <Column caption='Impuestos' calculateCellValue={calculateImpuestosCellValue} alignment='right' />
                  <Column dataField='total_price_tax_incl' caption='Total' dataType='decimal' />
                </Column>
                <Column caption='Detalle Descuentos'>
                  <Column dataField='original_product_price' caption='Precio Original' />
                  <Column dataField='reduction_percent' caption='% Desc' />
                  <Column dataField='reduction_amount' caption='$ Desc' />
                  <Column dataField='product_quantity_discount' caption='Desc. X Cant.' />
                </Column>
              </DataGrid>
              <div className='totals'>
                <div className='total'>
                  <div className='totalLabel'>TOTAL</div>
                  <div className='totalValue'>{(calculateTotal(detalles) + pedido.total_shipping_tax_incl).toFixed(2)}</div>
                </div>
                <div className='total'>
                  <div className='totalLabel'>Envío</div>
                  <div className='totalValue'>{pedido.total_shipping_tax_incl}</div>
                </div>
                <div className='total'>
                  <div className='totalLabel'>IMPUESTOS</div>
                  <div className='totalValue'>{calculateImpuestos(detalles).toFixed(2)}</div>
                </div>

                <div className='total'>
                  <div className='totalLabel'>DESCUENTOS</div>
                  <div className='totalValue'>{calculateDescuentos(detalles).toFixed(2)}</div>
                </div>

                <div className='total'>
                  <div className='totalLabel'>SUBTOTAL</div>
                  <div className='totalValue'>{calculateSubtotal(detalles).toFixed(2)}</div>
                </div>
              </div>
            </Tab>
            <Tab title='Dirección Envío'>
              <FieldGroup title='DIRECCIÓN'>
                <Field label='Persona' size='x2'>
                  <span>{deliverAddress.firstname + ' ' + deliverAddress.lastname}</span>
                </Field>
                <Field label='Compañia' size="x3">
                  <span>{deliverAddress.company}</span>
                </Field>
                <Field label='CI/RUC'>
                  <span>{deliverAddress.dni}</span>
                </Field>
                <Field label='Nombre Dirección'>
                  <span>{deliverAddress.alias}</span>
                </Field>
                <Field label='Pais'>
                  <span>{}</span>
                </Field>
                <Field label='Provincia / Estado'>
                  <span></span>
                </Field>
                <Field label='Ciudad'>
                  <span>{deliverAddress.city}</span>
                </Field>
                <Field label='Calle Principal' size='x2' >
                  <span>{deliverAddress.address1}</span>
                </Field>
                <Field label='Calle Secundaria' size='x2'>
                  <span>{deliverAddress.address2}</span>
                </Field>
                <Field label='Código Postal'>
                  <span>{deliverAddress.postcode}</span>
                </Field>
                <Field label='Celular'>
                  <span>{deliverAddress.phone_mobile}</span>
                </Field>
                <Field label='Teléfono'>
                  <span>{deliverAddress.phone + ' ' + deliverAddress.lastname}</span>
                </Field>
                <Field label='Otro'>
                  <span>{deliverAddress.other}</span>
                </Field>
                <Field label=''>
                  <span></span>
                </Field>
              </FieldGroup>

              <FieldGroup title='TRANSPORTE'>
                <Field label='Servicio Envío'>
                  <span>{carrier.name}</span>
                </Field>
                <Field label='Tracking'>
                  <span>{carrierInfo.tracking_number}</span>
                </Field>
                <Field label='Peso Productos'>
                  <span>{carrierInfo.weight} Kg</span>
                </Field>
                <Field label='Costo transporte'>
                  <span>{carrierInfo.shipping_cost_tax_incl}</span>
                </Field>
              </FieldGroup>
              {orderCarrierHistory && orderCarrierHistory.length > 0 &&
                <FieldGroup title='SEGUIMIENTO DE ENVÍO'>
                  <ScrollView
                    height={200}
                  >
                    <table className='tablaTrackingInfo'>
                      <thead>
                        <tr>
                          <th>Fecha</th>
                          <th>Estado</th>
                          <th>Ubicación</th>
                        </tr>
                      </thead>
                      <tbody>
                        {orderCarrierHistory.map(hist => {
                          return (
                            <tr>
                              <td>{hist.fecha}</td>
                              <td>{hist.estado}</td>
                              <td>{hist.ubicacion}</td>
                            </tr>
                          )
                        })

                        }
                      </tbody>
                    </table>
                  </ScrollView>
                </FieldGroup>
              }
            </Tab>
            <Tab title='Dirección Facturación'>
              <FieldGroup title='DATOS TRIBUTARIOS'>
                <Field label='Nombre Dirección'>
                  <span>{invoiceAddress.alias}</span>
                </Field>
                <Field label='Persona' size='x2'>
                  <span>{invoiceAddress.firstname + ' ' + invoiceAddress.lastname}</span>
                </Field>
                <Field label='Compañia' size='x2'>
                  <span>{invoiceAddress.company}</span>
                </Field>
                <Field label='CI/RUC'>
                  <span>{invoiceAddress.dni}</span>
                </Field>
              </FieldGroup>
              <FieldGroup title='CONTACTO'>
                <Field label='Celular'>
                  <span>{invoiceAddress.phone_mobile}</span>
                </Field>
                <Field label='Teléfono'>
                  <span>{invoiceAddress.phone + ' ' + deliverAddress.lastname}</span>
                </Field>
                <Field label='Otro'>
                  <span>{invoiceAddress.other}</span>
                </Field>
                <Field label=''>
                  <span></span>
                </Field>
              </FieldGroup>
              <FieldGroup title='DIRECCIÓN'>
                <Field label='Pais'>
                  <span>{}</span>
                </Field>
                <Field label='Provincia / Estado'>
                  <span></span>
                </Field>
                <Field label='Ciudad'>
                  <span>{invoiceAddress.city}</span>
                </Field>
                <Field label='Calle Principal' size='x2' >
                  <span>{invoiceAddress.address1}</span>
                </Field>
                <Field label='Calle Secundaria' size='x2'>
                  <span>{invoiceAddress.address2}</span>
                </Field>
                <Field label='Código Postal'>
                  <span>{invoiceAddress.postcode}</span>
                </Field>
              </FieldGroup>
              {infoFactura &&
                <FieldGroup title='DATOS SISTEMA'>
                  <Field label='Transacción' size='x2' >
                    <span>{infoFactura.Transaccion}</span>
                  </Field>
                  <Field label='Factura' size='x1' >
                    <span>{infoFactura.Numero}</span>
                  </Field>
                  <Field label='Fecha Factura' size='x1' >
                    <span>{infoFactura.Fecha}</span>
                  </Field>
                </FieldGroup>
              }
            </Tab>

            <Tab title='Pago'>
              <FieldGroup>
                <Field label='Forma pago' size='x1' >
                  <span>{pedido.payment}</span>
                </Field>
                {!paymentInfo &&
                  <Field label='Valor' size='x1' >
                    <span>Pago Pendiente</span>
                  </Field>
                }
                {paymentInfo &&
                  <Field label='Valor' size='x1' >
                    <span>{paymentInfo.amount}</span>
                  </Field>
                }
              </FieldGroup>
            </Tab>
            {/* <Tab title='Atención al Cliente'>
              <PedidoPrestashopCustomerService idOrden={id} key={'customerservice-' + id} />
            </Tab> */}
          </TabbedItem>
        </Form>
      </div>
      <div className='commandBar'>

        <Button
          text='Salir'
          stylingMode='contained'
          type='success'
          width={180}
          height={40}
          onClick={toogleZoom}
        />

        {status.module_name === 'ps_wirepayment' && !status.paid &&
          <Button
            id='buttonConfirmarTranferencia'
            onClick={async () => {
              setConfirmarTranferenciaVisible(true);
              await confirmarPagoTranferencia();
              setConfirmarTranferenciaVisible(false);
            }}

            width={180}
            height={40}
          >
            <LoadIndicator className="button-indicator" visible={confirmarTranferenciaVisible} />
            <span className="dx-button-text">{(confirmarTranferenciaVisible ? 'Confirmando...' : 'Confirmar pago')}</span>

          </Button>
        }

        {status.id === estadoPagoAceptado.id &&
          <ButtonFacturarPedidoPrestashop id={id} actualizarEstado={actualizarEstado} />
        }

        {status.id === 4 &&
          <Button
            id='buttonConfirmarEntrega'
            onClick={async () => {
              setConfirmarTranferenciaVisible(true);
              await confirmarEntrega();
              setConfirmarTranferenciaVisible(false);
            }}

            width={180}
            height={40}
          >
            <LoadIndicator className="button-indicator" visible={confirmarTranferenciaVisible} />
            <span className="dx-button-text">{(confirmarTranferenciaVisible ? 'Confirmando...' : 'Confirmar Entrega')}</span>

          </Button>
        }


        {infoFactura && status.id === 3 &&
          <div>
            <Button
              text='Registrar Guía'
              width={180}
              height={40}
              onClick={() => { setshowRegistrarGuia(true) }}
            />
            <Popup
              title='Registrar Guía'
              showTitle={true}
              visible={showRegistrarGuia}
              onHiding={() => setshowRegistrarGuia(false)}
              closeOnOutsideClick={true}
              width='900px'
              height='400px'
            >
              <div id={'registrarGuia' + id}>
                {showRegistrarGuia &&
                  <PedidoRegistrarTracking idPedido={id} ></PedidoRegistrarTracking>
                }

              </div>
            </Popup>
          </div>
        }

        {!infoFactura && status.id !== 6 &&
          <div>
            <Button
              text='Cancelar Pedido'
              width={180}
              height={40}
              stylingMode='contained'
              type='danger'
              onClick={() => { setshowCancelarPedido(true) }}
            />
            <Popup
              title='Cancelar Pedido'
              showTitle={true}
              visible={showCancelarPedido}
              onHiding={() => setshowCancelarPedido(false)}
              closeOnOutsideClick={false}
              width='350px'
              height='250px'
            >
              <div id={'cancelarOrden' + id}>
                {showCancelarPedido &&
                  <PedidoCancelarPedido id={id} />
                }

              </div>
            </Popup>
          </div>
        }




      </div>
    </div>
  );
}


function formateDate(dateString) {
  const values = dateString.split('-');
  return `${values[2].substring(0, 2)}/${values[1]}/${values[0]}`;
}

function calculateSubtotal(detalles) {
  const valores = detalles.map((item) => item.product_quantity * item.product_price);
  return valores.reduce((total, item) => {
    return total + item;
  });
}
function calculateDescuentos(detalles) {
  const valores = detalles.map((item) => (item.product_price - item.unit_price_tax_excl));
  return valores.reduce((total, item) => {
    return total + item;
  });
}
function calculateImpuestos(detalles) {
  const valores = detalles.map((item) => (item.product_quantity * item.unit_price_tax_excl) * 0.12);
  return valores.reduce((total, item) => {
    return total + item;
  });
}

function calculateTotal(detalles) {
  const valores = detalles.map((item) => item.total_price_tax_incl);
  return valores.reduce((total, item) => {
    return total + item;
  });
}


function calculateSubtotalCellValue(data) {
  return Math.fround(data.product_quantity * data.product_price).toFixed(2);
}

function calculateImpuestosCellValue(data) {
  return Math.fround((data.product_quantity * data.unit_price_tax_excl) * 0.12).toFixed(2);
}

function calculateDescuentosCellValue(data) {
  return Math.fround((data.product_price - data.unit_price_tax_excl) * data.product_quantity).toFixed(2);
}

function cellProductPictureRender(data) {
  return (
    <ItemImageRender idProduct={data.data.product_id} idItemAutomatizer={data.data.idItemAutomatizer} />
  )
}

function cellSeriesRender(data) {
  if (data.data.TieneSeries) {
    return <CellIngresarSerieProductoPrestashop idDetalle={data.data.id} seriesActuales={data.data.Series} cantidad={data.data.product_quantity} />
  } else {
    return (
      <>NO APLICA</>
    )
  }
}

function cellSeriesRenderNotEditable(data) {
  if (data.data.TieneSeries && data.data.Series) {
    return <div className='cellSeries'>{data.data.Series.split(';').map(x => <span>{x}</span>)}</div>
  } else {
    return (
      <></>
    )
  }
}


function ItemImageRender({ idProduct, idItemAutomatizer }) {
  const [loading, setLoading] = useState(true);
  const [zoom, setZoom] = useState(false);
  const [imagen, setImagen] = useState(null);

  const toogleZoom = useCallback(() => {
    setZoom(!zoom);
  }, [zoom]);

  const cargarImagen = useCallback(async () => {
    var image = await PrestashopService.getProductImage(idProduct);
    var blob = await BlobService.getBlobData(image.BlobID);
    const tmp = {
      blob: blob,
      url: URL.createObjectURL(blob),
    };
    setImagen(tmp);
    setLoading(false);
  }, [idProduct])

  useEffect(() => {
    cargarImagen();
  }, [cargarImagen])

  if (loading) {
    return (
      <div style={{ width: 60, height: 60 }}   >
        <CssLoadingIndicator />
      </div>
    )
  }

  return (
    <div onDoubleClick={toogleZoom} className='imagenProdcuto' style={{ backgroundImage: `url(${imagen.url})` }} >
      <Popup
        width='80%'
        height='90%'
        visible={zoom}
        title='Ficha del producto'
        onHiding={toogleZoom}
      >
        <div>
          {zoom &&
            <ArbolInventarioItem itemId={idItemAutomatizer} height={(window.innerHeight * 0.75)} />
          }
        </div>
      </Popup>
    </div>);

}