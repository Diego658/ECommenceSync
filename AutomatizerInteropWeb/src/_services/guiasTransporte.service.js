import { FechHelper } from '../_helpers/fech-helper';

export const GuiasTransporteService = {
  getCompaniasTransporte,
  getUsuariosGuias,
  getGuias,
  getGuia,
  getClientesParaGuia,
  getFacturasParaGuia,
  getInfofacturaParaGuia
}

async function getCompaniasTransporte() {
  return FechHelper.get("GuiasTransporte", "CompaniasTransporte");
}


async function getUsuariosGuias() {
  return FechHelper.get("GuiasTransporte", "GuiasTransporte/UsuariosGuias");
}

async function getGuias(filtro) {
  return FechHelper.get("GuiasTransporte", "GuiasTransporte/Guias", null, filtro);
}

async function getGuia(idGuia) {
  return FechHelper.get("GuiasTransporte", "GuiasTransporte/Guia?idGuia=" + idGuia);
}


async function getClientesParaGuia(fecha) {
  return FechHelper.get("GuiasTransporte", "GuiasTransporte/ClientesParaGuia?fecha=" + fecha);
}

async function getFacturasParaGuia(clienteCodigo, fecha) {
  return FechHelper.get("GuiasTransporte", `GuiasTransporte/FacturasParaGuia?clienteCodigo=${clienteCodigo}&fecha=${fecha}` );
}

async function getInfofacturaParaGuia(codigo, numero) {
  return FechHelper.get("GuiasTransporte", `GuiasTransporte/GetInfofacturaParaGuia?codigo=${codigo}&numero=${numero}` );
}