import { config } from '../constants/constants'
import { authHeader, handleResponse } from '../_helpers';

export const interopConfiguracionesService = {
    getAll,
    getOne
};

function getAll() {
    const requestOptions = { method: 'GET', headers: authHeader() };
    return fetch(config.url.API_URL + "/api/ConfiguracionesInterop/GetAll", requestOptions).then(handleResponse);
}

function getOne(configuracionId) {
    const requestOptions = { method: 'GET', headers: authHeader() };
    return fetch(config.url.API_URL + "/api/FacturacionElectronica/Get", requestOptions).then(handleResponse);
}
