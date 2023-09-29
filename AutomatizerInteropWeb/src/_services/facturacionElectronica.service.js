import { config } from '../constants/constants'
import { authHeader, handleResponse } from '../_helpers';

export const facturacionElectronicaService = {
    GetNumeroPendientes,
    getFacturas,
    getNotasCredito,
    getRetenciones,
    reintentarDocumento,
    descargarDocumento,
    descargarDocumentoXml,
    descargarDocumentoPdf,
    getBlobPdf,
    iniciarProcesoRetencion,
    reenviarEmail
};

function GetNumeroPendientes(configuracionId) {
    const requestOptions = { method: 'GET', headers: authHeader() };
    return fetch(config.url.API_URL + "/api/FacturacionElectronica/GetNumeroPendientes?configuracionId=" + configuracionId, requestOptions).then(handleResponse);
}


function getFacturas(configuracionId, fechaInicio, fechaFin){
    const requestOptions = { method: 'GET', headers: authHeader() };
    return fetch(config.url.API_URL + `/api/FacturacionElectronica/Facturas?idConfiguracion=${configuracionId}&fechaInicio=${fechaInicio.toJSON()}&fechaFin=${fechaFin.toJSON()}`, requestOptions).then(handleResponse);
}

function getNotasCredito(configuracionId, fechaInicio, fechaFin){
    const requestOptions = { method: 'GET', headers: authHeader() };
    return fetch(config.url.API_URL + `/api/FacturacionElectronica/NotasCredito?idConfiguracion=${configuracionId}&fechaInicio=${fechaInicio.toJSON()}&fechaFin=${fechaFin.toJSON()}`, requestOptions).then(handleResponse);
}


function getRetenciones(configuracionId, fechaInicio, fechaFin){
    const requestOptions = { method: 'GET', headers: authHeader() };
    return fetch(config.url.API_URL + `/api/FacturacionElectronica/Retenciones?idConfiguracion=${configuracionId}&fechaInicio=${fechaInicio.toJSON()}&fechaFin=${fechaFin.toJSON()}`, requestOptions).then(handleResponse);
}


function reintentarDocumento(idDocumento){
    //const requestOptions = { method: 'POST', headers: authHeader() };
    const auth = authHeader();
    var request = new Request(config.url.API_URL + "/api/FacturacionElectronica/ReintentarDocumento?idDocumento=" + idDocumento, {
        method: 'POST', 
        headers: new Headers({
          'Content-Type': 'application/json',
          'Authorization': auth.Authorization
        })
      });

    return fetch(request).then(handleResponse);
}

function descargarDocumentoXml(idDocumento, downloadName){
    const requestOptions = { method: 'GET', headers: authHeader() };
    return fetch(config.url.API_URL + `/api/FacturacionElectronica/GetXml?idDocumento=${idDocumento}`, requestOptions)
        .then((response)=> response.blob() )
        .then((blob)=>{
            const url = window.URL.createObjectURL(new Blob([blob]));
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', `${downloadName}`);
            document.body.appendChild(link);
            link.click();
            link.parentNode.removeChild(link);
        });
}

function descargarDocumentoPdf(idDocumento, downloadName){
    const requestOptions = { method: 'GET', headers: authHeader() };
    return fetch(config.url.API_URL + `/api/FacturacionElectronica/GetPdf?idDocumento=${idDocumento}`, requestOptions)
        .then((response)=> response.blob() )
        .then((blob)=>{
            const url = window.URL.createObjectURL(new Blob([blob]));
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', `${downloadName}`);
            document.body.appendChild(link);
            link.click();
            link.parentNode.removeChild(link);
        });
}

function getBlobPdf(tipoDocumento, codigo, numero){
    //GetPdf2
    const requestOptions = { method: 'GET', headers: authHeader() };
    return fetch(config.url.API_URL + `/api/FacturacionElectronica/GetPdf2?tipoDocumento=${tipoDocumento}&codigo=${codigo}&numero=${numero}`, requestOptions)
}


function descargarDocumento(idDocumento){

}


function iniciarProcesoRetencion(configuracionId, retencion){
    const auth = authHeader();
    var request = new Request(config.url.API_URL + "/api/FacturacionElectronica/IniciarProcesoRetencion?idConfiguracion=" + configuracionId, {
        method: 'POST', 
        headers: new Headers({
          'Content-Type': 'application/json',
          'Authorization':auth.Authorization
        }),
        body: JSON.stringify(retencion)
      });
      
      //request.headers.append('Authorization', auth.Authorization)
    return fetch(request).then(handleResponse);
}



function reenviarEmail(configuracionId, docId, mails){
    const auth = authHeader();
    var request = new Request(config.url.API_URL + `/api/FacturacionElectronica/ReintentarEnvioCorreo?idConfiguracion=${configuracionId}&documentoId=${docId}&emails=${mails}`, {
        method: 'POST', 
        headers: new Headers({
          'Content-Type': 'application/json',
          'Authorization':auth.Authorization
        })
      });
      
      //request.headers.append('Authorization', auth.Authorization)
    return fetch(request).then(handleResponse);
}
