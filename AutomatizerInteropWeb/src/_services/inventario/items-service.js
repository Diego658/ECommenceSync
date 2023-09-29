import { FechHelper } from '../../_helpers/fech-helper';
export const ItemsService = {
  getSeriesDisponibles
}

async function getSeriesDisponibles(codigoBodega, idItem) {
  const res = await FechHelper.get("Inventario",
    `Items/SeriesDisponibles?codigoBodega=${codigoBodega}&idItem=${idItem}`);
  return res;
}
