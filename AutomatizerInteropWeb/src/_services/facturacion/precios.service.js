import { FechHelper } from "../../_helpers/fech-helper";


export const PreciosService = {
  getPrecioItem
}


async function getPrecioItem(tipoClienteId, itemId) {
  const precio = await FechHelper.get("Facturacion",
    `Precios/GetPrecioItem?tipoClienteId=${tipoClienteId}&itemId=${itemId}`);
  return precio;
}