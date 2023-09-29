import { config } from '../constants/constants'
import { authHeader, handleResponse } from '../_helpers';
import UserSesion from '../utils/userSesion';

export const TransaccionesService= {
    getTransacciones
}


function getTransacciones(moduloId, categoriaCodigo){
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    const requestOptions = { method: 'GET', headers: authHeader() };
    return fetch(`${config.url.API_URL}/api/Transacciones/GetTransacciones?configuracionId=${configuracionId}&modulo=${moduloId}&categoria=${categoriaCodigo}`, requestOptions)
        .then(handleResponse);
}