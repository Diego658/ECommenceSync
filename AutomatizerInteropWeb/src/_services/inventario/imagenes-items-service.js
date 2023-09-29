import { FechHelper } from "../../_helpers/fech-helper";

export const InventarioImagenesItemsService = {
  imagenes,
  remove,
  update
};

const modulo = 'Inventario';
const controller= 'ImagenesItems';


async function imagenes(itemId){
  try {
    const imagenes = await FechHelper.get(modulo, controller, null, {'itemId':itemId});
    return {ok : true, data : imagenes};  
  } catch (error) {
    return {ok : false, data : error};  
  }
  
}

async function imagen(imageId){

}


async function update(imageId, imageData){
  try {
    const respuesta = await FechHelper.put(modulo, controller + '/', imageData ,imageId);
    return {ok : true, data : respuesta};  
  } catch (error) {
    return {ok : false, data : error};  
  }
}

async function remove(imageId){
  try {
    const respuesta = await FechHelper.remove(modulo, controller + '/', imageId);
    return {ok : true, data : respuesta};  
  } catch (error) {
    return {ok : false, data : error};  
  }
}
