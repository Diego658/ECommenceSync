import { config } from '../constants/constants'
import { authHeader, handleResponse } from '../_helpers';
import UserSesion from '../utils/userSesion';

export const ClientesService = {
    getContactosCliente,
    updateEmail
};

function getContactosCliente(configuracionId, clienteCodigo){
    const requestOptions = { method: 'GET', headers: authHeader() };
    return fetch(`${config.url.API_URL}/api/Clientes/GetContactosCliente?configuracionId=${configuracionId}&clienteCodigo=${clienteCodigo}`, requestOptions).then(handleResponse);
}

function updateEmail(clienteCodigo, email){
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    const requestOptions = { method: 'POST', headers: authHeader() };
    return fetch(`${config.url.API_URL}/api/Clientes/UpdateEmail?configuracionId=${configuracionId}&clienteCodigo=${clienteCodigo}&email=${email}`, requestOptions).then(handleResponse);
}