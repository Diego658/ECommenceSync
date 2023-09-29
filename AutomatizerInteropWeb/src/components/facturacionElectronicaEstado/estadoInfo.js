import React from 'react';
//import { Button } from 'devextreme-react';
import './facturacionElectronicaEstado.scss';

export default function EstadoInfo(item) {
  return (
    <React.Fragment>
      <div className="configuracionName">{item.configuracionName}</div>
      <table>
        <tr>
          <th>Tipo Documento</th>
          <th className="header-valor">Pendientes</th>
        </tr>
        <tr>
          <td>Facturas</td>
          <td className="number">{item.factura}</td>
        </tr>
        <tr>
          <td>Notas de Crédito</td>
          <td className="number">{item.notaCredito}</td>
        </tr>
        <tr>
          <td>Notas de Débito</td>
          <td className="number">{0}</td>
        </tr>
        <tr>
          <td>Retenciones</td>
          <td className="number">{item.retencion}</td>
        </tr>
      </table>
    </React.Fragment>
  );
}