import { config } from '../constants/constants'
import { authHeader, handleResponse } from '../_helpers';
import UserSesion from '../utils/userSesion';

export const BodegaService = {
    getItemDetalleBodegas
};


function getItemDetalleBodegas(itemId){
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    const requestOptions = { method: 'GET', headers: authHeader() };
    return fetch(`${config.url.API_URL}/api/Bodega/GetItemDetalleBodegas?configuracionId=${configuracionId}&itemId=${itemId}`, requestOptions)
        .then(handleResponse);
}