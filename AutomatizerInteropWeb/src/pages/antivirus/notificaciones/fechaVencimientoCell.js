import React from 'react';
import Moment from 'react-moment';

export default function FechaVencimientoCell(cellData) {
  var cssClass = "";
  switch (cellData.data.grupoVencimiento) {
    case 'MÁS DE TRES MESES':
      cssClass = "antivirus-text4";
      break;
    case 'HACE DOS MESES':
      cssClass = "antivirus-text3";
      break;
    case 'EL MES PASADO':
      cssClass = "antivirus-text2";
      break;
    case 'ESTE MES':
      cssClass = "antivirus-text1";
      break;
    default:
      cssClass = "antivirus-text1";
  }
  return (
    <Moment format="YYYY/MM/DD" className={cssClass}>
      {cellData.data.fechaVencimiento}
    </Moment>
  );
}