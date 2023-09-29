import { config } from '../constants/constants'
import { authHeader, handleResponse } from '../_helpers';
import UserSesion from '../utils/userSesion';
import CustomStore from 'devextreme/data/custom_store';

//import { createStore } from 'devextreme-aspnet-data-nojquery';


export const InventarioDatasourceService = {
  dataStoreAtributos,
  dataStoreAtributosValues,
  dataStoreAtributosWithValues,
  dataStoreAtributosWithValuesGrouped
}



function dataStoreAtributos() {
  const url = `${config.url.API_URL}/api/Automatizer/Inventario/Atributos/`;
  const dataSource = new CustomStore({
    key: 'id',
    load: () => sendRequest(url),
    insert: (values) => sendRequest(url, 'POST', JSON.stringify(values)),
    update: (key, values) => sendRequest(`${url}${key}`, 'PUT', JSON.stringify({ ...values, id: key })),
    remove: (key) => sendRequest(`${url}${key}`, 'DELETE')

  });
  return dataSource;
}


function dataStoreAtributosValues(atributoId) {
  const url = `${config.url.API_URL}/api/Automatizer/Inventario/AtributoValores`;
  const dataSource = new CustomStore({
    key: 'id',
    load: () => sendRequest(`${url}?atributoId=${atributoId}`),
    insert: (values) => sendRequest(`${url}?atributoId=${atributoId}`, 'POST', JSON.stringify(values)),
    update: (key, values) => sendRequest(`${url}/${key}`, 'PUT', JSON.stringify({ ...values, id: key })),
    remove: (key) => sendRequest(`${url}/${key}`, 'DELETE')

  });
  return dataSource;
}


function dataStoreAtributosWithValues() {
  const url = `${config.url.API_URL}/api/Automatizer/Inventario/AtributoValores/GetAllAtributesWithValues`;
  const dataSource = new CustomStore({
    key: 'valorId',
    load: (options) => sendRequest(`${url}`, 'GET', options)
  });
  return dataSource;
}

function dataStoreAtributosWithValuesGrouped() {
  const url = `${config.url.API_URL}/api/Automatizer/Inventario/AtributoValores/GetAllAtributesWithValuesGrouped`;
  const dataSource = new CustomStore({
    key: 'valorId',
    load: (options) => sendRequest(`${url}`, 'GET', options),
  });
  return dataSource;
}

function isNotEmpty(value) {
  return value !== undefined && value !== null && value !== '';
}

const sendRequest = (url, method, data) => {
  const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
  const auth = authHeader();
  method = method || 'GET';
  data = data || {};

  //this.logRequest(method, url, data);

  if (method === 'GET') {
    let params = '?';
    [
      'skip',
      'take',
      'requireTotalCount',
      'requireGroupCount',
      'sort',
      'filter',
      'totalSummary',
      'group',
      'groupSummary',
      'searchOperation',
      'searchValue',
      'searchExpr'
    ].forEach(function (i) {
      if (i in data && isNotEmpty(data[i])) { params += `${i}=${data[i]}&`; }
    });
    params = params.slice(0, -1);

    return fetch(url + params, {
      method: method,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': auth.Authorization,
        'ConfiguracionId': configuracionId
      }
    }).then(result => result.json().then(json => {
      return json;
    }, error => { throw error; }));
  }


  return fetch(url, {
    method: method,
    body: data,
    headers: {
      'Content-Type': 'application/json',
      'Authorization': auth.Authorization,
      'ConfiguracionId': configuracionId
    }
  }).then(result => {
    if (result.ok) {
      return result.text().then(text => text && JSON.parse(text));
    } else {
      return result.json().then(json => {
        throw json.Message;
      });
    }
  });
}
