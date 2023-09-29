import { authenticationService } from '../_services/authentication.service';

export function handleResponse(response) {
    if (response.status === 400) {
        return response.text().then(text => {
            return Promise.reject(text);
        });
    }
    else if (response.status === 500) {
        return response.text().then(text => {
            return Promise.reject(text);
        });
    } else {
        return response.text().then(text => {
            const data = text && JSON.parse(text);
            if (!response.ok) {
                if ([401, 403].indexOf(response.status) !== -1) {
                    // auto logout if 401 Unauthorized or 403 Forbidden response returned from api
                    authenticationService.logout();
                    //location.reload();
                    //location.reload(true);
                }
                if (response.url.indexOf('authenticate') > 0 && response.status === 400) {
                    return Promise.reject('Usuario o contraseña incorrectos!!!');
                }
                //data = text && JSON.parse(text)
                const error = (data && data.message) || response.statusText;
                return Promise.reject(error);
            }

            return data;
        });
    }


}