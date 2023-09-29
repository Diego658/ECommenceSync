import UserSesion from "../utils/userSesion";
import { authHeader } from "./auth-header";
import { config } from "../constants/constants";
import { handleResponse } from "./handle-response";

export const FechHelper = {
  get,
  post,
  put,
  remove,
  getUrl,
  getAutorizedHeader
}

function getUrl(modulo, controller, key, query) {
  let url = `${config.url.API_URL}/api/Automatizer/${modulo}/${controller}${(key === undefined || key === null) ? '' : key}`;
  let params = '?';
  if(query !== null && query !== undefined){
    const keys= Object.keys(query);
    for (let index = 0; index < keys.length; index++) {
      const value = query[keys[index]];
      params+=`${keys[index]}=${value}&`;
    }
  }
  params = params.slice(0, -1);
  return url +params;
}

async function get(modulo, controller, key, query ) {
  const url = getUrl(modulo, controller, key, query);
  const getData = await sendRequest(url, 'GET');
  return getData;
}

async function post(modulo, controller, data) {
  const url = getUrl(modulo, controller);
  const postResult = await sendRequest(url, 'POST', data);
  return postResult;
}

async function put(modulo, controller, data, key) {
  const url = getUrl(modulo, controller, key, null);
  const putResult = await sendRequest(url, 'PUT', data);
  return putResult;
}

async function remove(modulo, controller, key) {
  const url = getUrl(modulo, controller, key);
  const deleteResult = await sendRequest(url, 'DELETE');
  return deleteResult;
}

function getAutorizedHeader(){
  const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
  const auth = authHeader();
  const header= {
    'Authorization': auth.Authorization,
    'ConfiguracionId': configuracionId
  };
  return header;
}


function sendRequest(url, method, data) {
  const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
  const auth = authHeader();

  method = method || 'GET';
  data = data || {};

  if (method === 'GET') {
    return fetch(url, {
      method: method,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': auth.Authorization,
        'ConfiguracionId': configuracionId
      }
    }).then(handleResponse).then(json => {
      return json;
    }, eror=>{
      throw eror
    }
    )
  }


  return fetch(url, {
    method: method,
    body: JSON.stringify( data),
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
