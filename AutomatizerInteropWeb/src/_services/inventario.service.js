import { config } from '../constants/constants'
import { authHeader, handleResponse } from '../_helpers';
import UserSesion from '../utils/userSesion';
import { FechHelper } from '../_helpers/fech-helper';

export const InventarioService = {
    getArbolItems,
    getSubitems,
    getCategoriaInventario,
    getItemDynamic,
    actualizarCategoria,
    getImagesItem,
    deleteImageCategory,
    addImageItem,
    migrarImagenesCategoriasAutomatizerToBlob,
    migrarImagenesItemsAutomatizerToBlob,
    getUnidades,
    getMarcas,
    getMarca,
    guardarMarca,
    getTallas,
    getColores,
    getProcedencias,
    getOrigenes,
    updateCategoriasToSync,
    deleteImageItem,
    saveItem,
    getErroresSincronizacionProductos,
    getProductosSinImagen,
    addNewCategory,
    addNewProduct,
    copyProduct
};

function getArbolItems(){
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    const requestOptions = { method: 'GET', headers: authHeader() };
    return fetch(`${config.url.API_URL}/api/Inventario/GetArbolItems?configuracionId=${configuracionId}`, requestOptions)
        .then(handleResponse);
}

function getSubitems(codigoPadre, nivel, loadOptions){
    const auth = authHeader();

    if(!loadOptions){
        loadOptions = "";
    }

    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    //console.log(JSON.stringify(loadOptions));
    let method = loadOptions===""? 'GET':'POST';
    const requestOptions = { 
        method: method, 
        headers: new Headers({
            'Authorization':auth.Authorization
          })
         };
    if(method ==='POST'){
        requestOptions.body = JSON.stringify(loadOptions);
        requestOptions.headers.append('Content-Type','application/json')
    }
        
    return fetch(`${config.url.API_URL}/api/Inventario/GetSubitems?configuracionId=${configuracionId}&codigoPadre=${codigoPadre}&nivel=${nivel}`, requestOptions)
        .then(handleResponse);
}



function getCategoriaInventario(categoriaId){
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    const requestOptions = { method: 'GET', headers: authHeader() };
    return fetch(`${config.url.API_URL}/api/Inventario/GetCategoriaInventario?configuracionId=${configuracionId}&categoriaID=${categoriaId}`, requestOptions)
        .then(handleResponse);
}


function getItemDynamic(itemId){
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    const requestOptions = { method: 'GET', headers: authHeader() };
    return fetch(`${config.url.API_URL}/api/Inventario/GetItemDynamic?configuracionId=${configuracionId}&itemID=${itemId}`, requestOptions)
        .then(handleResponse);
}


function actualizarCategoria(categoria){
    const datos = {};
    datos.Nombre = categoria.Nombre;
    datos['Description-Html'] = categoria['Description-Html'];
    datos['Description-Short-Html'] = categoria['Description-Short-Html'];
    return saveItem(datos, categoria.ItemID);
    //console.log(categoria);
}


function getImagesItem(itemID){
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    const requestOptions = { method: 'GET', headers: authHeader() };
    return fetch(`${config.url.API_URL}/api/Inventario/GetImagenesItem?configuracionId=${configuracionId}&itemID=${itemID}`, requestOptions)
        .then(handleResponse);
}



function addImageItem(itemID, type, blobId){
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    const requestOptions = { method: 'POST', headers: authHeader() };
    return fetch(`${config.url.API_URL}/api/Inventario/AddImagenItem?configuracionId=${configuracionId}&itemID=${itemID}&type=${type}&blobId=${blobId}`, requestOptions)
        .then(handleResponse);
}


async function deleteImageCategory(categoriaId, blobId){
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    const requestOptions = { method: 'DELETE', headers: authHeader() };
    let response = await fetch(`${config.url.API_URL}/api/Inventario/DeleteImageCategory?configuracionId=${configuracionId}&categoriaId=${categoriaId}&blobId=${blobId}`, requestOptions);
    return handleResponse(response);
}


async  function deleteImageItem(itemID, imageId, blobId, type){
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    const requestOptions = { method: 'DELETE', headers: authHeader() };
    let response = await fetch(`${config.url.API_URL}/api/Inventario/DeleteImagenItem?configuracionId=${configuracionId}&itemID=${itemID}&imageId=${imageId}&blobId=${blobId}&type=${type}`, requestOptions);
    return handleResponse(response);
}


async function migrarImagenesCategoriasAutomatizerToBlob(){
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    const requestOptions = { method: 'POST', headers: authHeader() };
    return fetch(`${config.url.API_URL}/api/Inventario/MigrarImagenesCategoriasAutomatizerToBlob?configuracionId=${configuracionId}`, requestOptions)
        .then(handleResponse);
}

async function migrarImagenesItemsAutomatizerToBlob(){
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    const requestOptions = { method: 'POST', headers: authHeader() };
    return fetch(`${config.url.API_URL}/api/Inventario/MigrarImagenesItemsAutomatizerToBlob?configuracionId=${configuracionId}`, requestOptions)
        .then(handleResponse);
}

function getUnidades(){
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    const requestOptions = { method: 'GET', headers: authHeader() };
    return fetch(`${config.url.API_URL}/api/Inventario/GetUnidades?configuracionId=${configuracionId}`, requestOptions)
        .then(handleResponse);
}


function getMarcas(){
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    const requestOptions = { method: 'GET', headers: authHeader() };
    return fetch(`${config.url.API_URL}/api/Inventario/GetMarcas?configuracionId=${configuracionId}`, requestOptions)
        .then(handleResponse);
}

function getMarca(marcaId){
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    const requestOptions = { method: 'GET', headers: authHeader() };
    return fetch(`${config.url.API_URL}/api/Inventario/GetMarca?configuracionId=${configuracionId}&marcaId=${marcaId}`, requestOptions)
        .then(handleResponse);
}

function guardarMarca(marcaId, marca){
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    const auth = authHeader();
    const requestOptions = { 
        method: 'POST', 
        headers: new Headers({
            'Content-Type': 'application/json',
            'Authorization':auth.Authorization
          }),
        body: JSON.stringify(marca)
    };
    return fetch(`${config.url.API_URL}/api/Inventario/GuardarMarca?configuracionId=${configuracionId}&marcaId=${marcaId}`, requestOptions)
        .then(handleResponse);
}


function getColores(){
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    const requestOptions = { method: 'GET', headers: authHeader() };
    return fetch(`${config.url.API_URL}/api/Inventario/GetColores?configuracionId=${configuracionId}`, requestOptions)
        .then(handleResponse);
}



function getProcedencias(){
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    const requestOptions = { method: 'GET', headers: authHeader() };
    return fetch(`${config.url.API_URL}/api/Inventario/GetProcedencias?configuracionId=${configuracionId}`, requestOptions)
        .then(handleResponse);
}


function getTallas(){
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    const requestOptions = { method: 'GET', headers: authHeader() };
    return fetch(`${config.url.API_URL}/api/Inventario/GetTallas?configuracionId=${configuracionId}`, requestOptions)
        .then(handleResponse);
}


function getOrigenes(){
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    const requestOptions = { method: 'GET', headers: authHeader() };
    return fetch(`${config.url.API_URL}/api/Inventario/GetOrigenes?configuracionId=${configuracionId}`, requestOptions)
        .then(handleResponse);
}




function updateCategoriasToSync(ids, idsOmitir){
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    const body = {};
    body.idsCategorias = ids.toString();
    body.idsCategoriasOmitir = idsOmitir.toString();
    const auth = authHeader();
    const requestOptions = { 
        method: 'POST', 
        headers: new Headers({
            'Content-Type': 'application/json',
            'Authorization':auth.Authorization
          }),
        body: JSON.stringify(body)
    };
    return fetch(`${config.url.API_URL}/api/StoreSync/SetCategoriasSincronizar?configuracionId=${configuracionId}`, requestOptions)
        .then(handleResponse);
}


function saveItem(modifiedData, itemId){
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    const auth = authHeader();
    const requestOptions = { 
        method: 'PUT', 
        headers: new Headers({
            'Content-Type': 'application/json',
            'Authorization':auth.Authorization
          }),
        body: JSON.stringify(modifiedData)
    };
    return fetch(`${config.url.API_URL}/api/Inventario/UpdateItem?configuracionId=${configuracionId}&itemId=${itemId}`, requestOptions)
        .then(handleResponse);
}

function  getErroresSincronizacionProductos(){
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    const requestOptions = { method: 'GET', headers: authHeader() };
    return fetch(`${config.url.API_URL}/api/StoreSync/GetErroresSincronizacionProductos?configuracionId=${configuracionId}`, requestOptions)
        .then(handleResponse);
}


function getProductosSinImagen(){
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    const requestOptions = { method: 'GET', headers: authHeader() };
    return fetch(`${config.url.API_URL}/api/StoreSync/GetProductosSinImagen?configuracionId=${configuracionId}`, requestOptions)
        .then(handleResponse);
}



async function addNewCategory(padreId, data){
    const re = await FechHelper.post("Inventario",
    `Inventario/AddProductCategory?padreId=${padreId}`, data);
    return re;   
}

async function addNewProduct(padreId, data){
    const re = await FechHelper.post("Inventario",
    `Inventario/AddProduct?padreId=${padreId}`, data);
    return re;   
}


async function copyProduct(origenid, data){
    const re = await FechHelper.post("Inventario",
    `Inventario/CopyProduct?origenid=${origenid}`, data);
    return re;   
}