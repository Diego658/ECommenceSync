import { config } from '../constants/constants'
import { authHeader, handleResponse } from '../_helpers';
import UserSesion from '../utils/userSesion';
import { FechHelper } from '../_helpers/fech-helper';


export const InventarioValidationService = {
  atributosValidationName,
  atributoValorValidationName
}

function atributosValidationName(params) {
  const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
  const auth =authHeader();

  return fetch(`${config.url.API_URL}/api/Automatizer/Inventario/Atributos/CheckUniqueName?configuracionId=${configuracionId}`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json;',
      'Authorization': auth.Authorization
    },
    body: JSON.stringify({
      id: params.data.id,
      value: params.value
    })
  }).then(response => response.json());
}

function atributoValorValidationName(params, atributoId){
  const postData = {
    id: params.data.id,
    value: params.value
  };
  return FechHelper.post('Inventario', `AtributoValores/CheckUniqueName?atributoId=${atributoId}`, postData);
}