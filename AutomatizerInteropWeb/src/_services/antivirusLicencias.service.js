import { config } from '../constants/constants'
import { authHeader, handleResponse } from '../_helpers';
import UserSesion from '../utils/userSesion';
import { FechHelper } from '../_helpers/fech-helper';

export const antivirusLicenciasService = {
  getEstadoLicencias: getEstadoLicencias,
  getVentasPorRenovar: getVentasPorRenovar,
  getExistenciasVsRenovaciones,
  getPlantillasNotificacion,
  updatePlantillaNotificacion,
  getInformacionNotificacion,
  getInfoVentaAntivirus,
  registrarNotificacion,
  getFacturasParaRenovacion,
  registrarRenovacion,
  ocultarLicencia,
  renovarLicencia,
  getAniosRenovaciones,
  getMesesRenovaciones,
  getRenovaciones,
  getRenovacionesCostos
};

function getEstadoLicencias() {
  const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
  const requestOptions = { method: 'GET', headers: authHeader() };
  return fetch(config.url.API_URL + "/api/Antivirus/GetEstadoLicencias?configuracionId=" + configuracionId, requestOptions).then(handleResponse);
}


function getVentasPorRenovar(vencimientosFuturos, tipo, mostrarConfirmadas) {
  return FechHelper.get('Antivirus', `Antivirus/GetVentasPorRenovar?tipo=${tipo}&mostrarVencimientosFuturos=${vencimientosFuturos}&full=true`)
  // const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
  // const requestOptions = { method: 'GET', headers: authHeader() };
  // return fetch(config.url.API_URL + `/api/Antivirus/GetVentasPorRenovar?idConfiguracion=${configuracionId}&tipo=${tipo}&mostrarVencimientosFuturos=${vencimientosFuturos}&full=true`,
  //   requestOptions).then(handleResponse);
}



function getExistenciasVsRenovaciones() {
  const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
  const requestOptions = { method: 'GET', headers: authHeader() };
  return fetch(config.url.API_URL + `/api/Antivirus/GetSaldoLicenciaVsLicenciasPorRenovar?configuracionId=${configuracionId}`,
    requestOptions).then(handleResponse);
}

function getPlantillasNotificacion() {
  const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
  const requestOptions = { method: 'GET', headers: authHeader() };
  return fetch(config.url.API_URL + `/api/Antivirus/GetPlantillasNotificacion?configuracionId=${configuracionId}`,
    requestOptions).then(handleResponse);
}



function updatePlantillaNotificacion(plantilla, designData, htmlData) {
  const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
  var objPlantilla = {
    "plantillaID": plantilla.plantillaID,
    "nombrePlantilla": plantilla.nombrePlantilla,
    "contenido": designData,
    "contenidoHtml": htmlData
  };
  const auth = authHeader();
  var request = new Request(config.url.API_URL + "/api/Antivirus/GuardarPlantillaNotificacion?configuracionId=" + configuracionId, {
    method: 'PUT',
    headers: new Headers({
      'Content-Type': 'application/json',
      'Authorization': auth.Authorization
    }),
    body: JSON.stringify(objPlantilla)
  });

  return fetch(request).then(handleResponse);
}


function getInformacionNotificacion(notificacionId) {
  const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
  const requestOptions = { method: 'GET', headers: authHeader() };
  return fetch(config.url.API_URL + `/api/Antivirus/GetInformacionNotificacion?configuracionId=${configuracionId}&notificacionId=${notificacionId}&fullData=true`,
    requestOptions).then(handleResponse);


}

function getInfoVentaAntivirus(ventaId, fullInfo) {
  const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
  const auth = authHeader();
  const requestOptions = { method: 'GET', 
  headers: new Headers({
    'Content-Type': 'application/json',
    'Authorization': auth.Authorization,
    'ConfiguracionId':configuracionId
  }) };
  return fetch(config.url.API_URL + `/api/Antivirus/GetInformacionVentaAntivirus?configuracionId=${configuracionId}&ventaId=${ventaId}&fullData=${fullInfo}`,
    requestOptions).then(handleResponse);

}


function registrarNotificacion(ventaId, data) {
  const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
  const auth = authHeader();
  data.usuario = UserSesion.getCurrentUser().username;
  var request = new Request(config.url.API_URL + `/api/Antivirus/RegistrarNotificacion?configuracionId=${configuracionId}&ventaId=${ventaId}`, {
    method: 'POST',
    headers: new Headers({
      'Content-Type': 'application/json',
      'Authorization': auth.Authorization
    }),
    body: JSON.stringify(data)
  });

  return fetch(request).then(handleResponse);
}


function getFacturasParaRenovacion(transaccionCodigo, clienteCodigo) {
  const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
  const requestOptions = { method: 'GET', headers: authHeader() };
  return fetch(config.url.API_URL + `/api/Antivirus/GetFacturasParaRenovacion?configuracionId=${configuracionId}&transaccionCodigo=${transaccionCodigo}&clienteCodigo=${clienteCodigo}`,
    requestOptions).then(handleResponse);
}


function registrarRenovacion(ventaId, renovacionId, observaciones) {
  const auth = authHeader();
  const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
  const data = {
    empleadoId: UserSesion.getCurrentUser().id,
    ventaId: ventaId,
    renovacionId: renovacionId,
    observaciones: observaciones
  };
  const requestOptions =
  {
    method: 'POST',
    headers: new Headers({
      'Content-Type': 'application/json',
      'Authorization': auth.Authorization
    }),
    body:  JSON.stringify( data)
  };
  return fetch(config.url.API_URL + `/api/Antivirus/RegistrarRenovacion?configuracionId=${configuracionId}&ventaId=${ventaId}&empleadoId=${UserSesion.getCurrentUser().id}&renovacionId=${renovacionId}`,
    requestOptions).then(handleResponse);
}


async function ocultarLicencia(idVenta, data){
  const re = await FechHelper.post("Antivirus",
    `Antivirus/OcultarLicencia?ventaId=${idVenta}`, data);
    return re;
}


async function renovarLicencia(idVenta, data){
  const re = await FechHelper.post("Antivirus",
    `Antivirus/RenovarLicencia`, data);
    return re;
}


async function getAniosRenovaciones(){
  const re = await FechHelper.get("Antivirus",
    `Antivirus/GetAniosRenovaciones`);
    return re;
}


async function getMesesRenovaciones(year){
  const re = await FechHelper.get("Antivirus",
    `Antivirus/GetMesesRenovaciones?year=${year}`);
    return re;
}


async function getRenovaciones(year, month){
  const re = await FechHelper.get("Antivirus",
    `Antivirus/GetRenovaciones?year=${year}&month=${month}`);
    return re;
}

async function getRenovacionesCostos(year){
  const re = await FechHelper.get("Antivirus",
    `Antivirus/GetRenovacionesCostos?year=${year}`);
    return re;
}
