import { FechHelper } from '../../_helpers/fech-helper';

export const PrestashopService = {
  getErroresSync,
  getEstadosOrdenes,
  getOrdenes,
  getOrden,
  getAddres,
  getCarrier,
  getCustomer,
  getStatusOrden,
  getOrdenDetalles,
  getOrdenDetalle,
  getOrdenCarrier,
  getOrderPayment,
  getProductImage,
  confirmPaymentByTransfer,
  statuPayAcepted,
  getOrdenDetallesSeries,
  saveOrderDetailSeriesAutomatizer,
  facturarOrden,
  getAutomatizerCustomer,
  createCustomerFromAddress,
  getInfoFacturacion, 
  updateTracking,
  cancelarPedido,
  confirmarEntrega,
  getGuiaTransporteOrden,
  tieneSeriesCompletas,
  getOrderCarrierHistory,
  getTagsForText
}

function getErroresSync() {
  return FechHelper.get("Inventario", "Stores/Prestashop/Prestashop/GetErrores");
}


async function getEstadosOrdenes(year) {
  const estados = await FechHelper.get("Inventario",
    "Stores/Prestashop/Prestashop/EstadoOrdenes?year=" + year);
  return estados;
}


async function getOrdenes(year, states) {
  const estados = await FechHelper.get("Inventario",
    `Stores/Prestashop/Prestashop/Ordenes?year=${year}&states=${states}`);
  return estados;
}

async function getOrden(id) {
  const estados = await FechHelper.get("Inventario",
    `Stores/Prestashop/Prestashop/Orden?id=${id}`);
  return estados;
}


async function getAddres(id) {
  const estados = await FechHelper.get("Inventario",
    `Stores/Prestashop/Prestashop/Addres?id=${id}`);
  return estados;
}


async function getCustomer(id) {
  const estados = await FechHelper.get("Inventario",
    `Stores/Prestashop/Prestashop/Customer?id=${id}`);
  return estados;
}

async function getStatusOrden(id) {
  const estados = await FechHelper.get("Inventario",
    `Stores/Prestashop/Prestashop/EstadoOrden?id=${id}`);
  return estados;
}

async function getOrdenDetalles(idOrden) {
  const estados = await FechHelper.get("Inventario",
    `Stores/Prestashop/Prestashop/OrderDetails?idOrden=${idOrden}`);
  return estados;
};

async function getOrdenDetalle(idDetalle) {
  const estados = await FechHelper.get("Inventario",
    `Stores/Prestashop/Prestashop/OrderDetail?idDetail=${idDetalle}`);
  return estados;
};

async function getOrdenDetallesSeries(idOrden) {
  const series = await FechHelper.get("Inventario",
    `Stores/Prestashop/Prestashop/OrderDetailsSeries?idOrden=${idOrden}`);
  return series;
};

async function getCarrier(id) {
  const carrier = await FechHelper.get("Inventario",
    `Stores/Prestashop/Prestashop/Carrier?id=${id}`);
  return carrier;
}


async function getOrdenCarrier(idOrden) {
  const carrier = await FechHelper.get("Inventario",
    `Stores/Prestashop/Prestashop/OrderCarrier?idOrden=${idOrden}`);
  return carrier;
}


async function getOrderPayment(referene) {
  const carrier = await FechHelper.get("Inventario",
    `Stores/Prestashop/Prestashop/OrderPayment?reference=${referene}`);
  return carrier;
}



async function getProductImage(id) {
  const carrier = await FechHelper.get("Inventario",
    `Stores/Prestashop/Prestashop/ProductImage?id=${id}`);
  return carrier;
}

async function confirmPaymentByTransfer(idOrden) {
  const res = await FechHelper.post("Inventario",
    `Stores/Prestashop/Prestashop/ConfirmPaymentByTransfer?idOrden=${idOrden}`, null);
  return res;
}

async function statuPayAcepted() {
  const res = await FechHelper.get("Inventario",
    `Stores/Prestashop/Prestashop/PayAcceptedStatus`);
  return res;
}

async function saveOrderDetailSeriesAutomatizer(idDetail, series) {
  const res = await FechHelper.post("Inventario",
    `Stores/Prestashop/Prestashop/OrdeDetailSeries?idDetail=${idDetail}`, series);
  return res;
}

async function facturarOrden(idOrden, infoFactura) {
  const res = await FechHelper.post("Inventario",
    `Stores/Prestashop/Prestashop/FacturarOrden?idOrden=${idOrden}`, infoFactura);
  return res;
}

async function getAutomatizerCustomer(ciRuc) {
  const res = await FechHelper.get("Inventario",
    `Stores/Prestashop/Prestashop/AutomatizerCustomer?ciRuc=${ciRuc}`);
  return res;
}


async function createCustomerFromAddress(idAddress) {
  const res = await FechHelper.post("Inventario",
    `Stores/Prestashop/Prestashop/CreateCustomerFromAddres?idAddress=${idAddress}`);
  return res;
}

async function getInfoFacturacion(idOrden) {
  const res = await FechHelper.get("Inventario",
    `Stores/Prestashop/Prestashop/InfoFacturacion?idOrder=${idOrden}`);
  return res;
}


async function updateTracking(idOrden, data){
  const re = await FechHelper.post("Inventario",
    `Stores/Prestashop/Prestashop/UpdateTrackingNumber?idOrder=${idOrden}`, data);
  return re;
}

async function cancelarPedido(idOrden, data){
  const re = await FechHelper.post("Inventario",
    `Stores/Prestashop/Prestashop/CancelOrder?idOrder=${idOrden}`, data);
  return re;
}


async function confirmarEntrega(idOrden){
  const re = await FechHelper.post("Inventario",
    `Stores/Prestashop/Prestashop/ConfirmDelivery?idOrder=${idOrden}`);
    return re;
}

async function getGuiaTransporteOrden(idOrden){
  const re = await FechHelper.get("Inventario",
    `Stores/Prestashop/Prestashop/GuiaTransporteOrden?idOrder=${idOrden}`);
    return re;
}


async function tieneSeriesCompletas(idOrden){
  const re = await FechHelper.get("Inventario",
    `Stores/Prestashop/Prestashop/TieneSeriesCompletas?idOrder=${idOrden}`);
    return re;
}



async function getOrderCarrierHistory(idOrderCarrier){
  const re = await FechHelper.get("Inventario",
    `Stores/Prestashop/Prestashop/OrderCarrierTrankingInfo?idOrderCarrier=${idOrderCarrier}`);
    return re;
}

async function getTagsForText(text){
  const re = await FechHelper.get("Inventario",
    `Stores/Prestashop/Prestashop/GetTagsForText?text=${text}`);
    return re;
}