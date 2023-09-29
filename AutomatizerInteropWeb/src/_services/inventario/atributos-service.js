import { FechHelper } from "../../_helpers/fech-helper";

export const InventarioAtributosService = {
  getAtributo
};

const moudulo = 'Inventario';
const controller= 'Atributos/';

async function getAtributo(atributoId) {
  const atributo = await FechHelper.get(moudulo, controller, atributoId);
  return atributo;
}


