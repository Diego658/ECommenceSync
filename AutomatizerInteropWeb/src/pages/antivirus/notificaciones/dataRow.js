import React from 'react';
import './notificaciones.scss'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'


function formatDate(date) {
  return date;
}


export default function DataRow(rowInfo) {

  return (
    <tbody className={`employee dx-row ${rowInfo.rowIndex % 2 ? 'dx-row-alt' : ''}`}>
      <tr className="main-row">

        <td>{rowInfo.data.cliente}</td>
        <td>{rowInfo.data.producto}</td>
        <td>{rowInfo.data.periodoLicencia}</td>
        <td>{rowInfo.data.numeroNotificaciones}</td>
        <td>{formatDate(rowInfo.data.fechaVencimiento)}</td>
        <td>{formatDate(rowInfo.data.fechaUltimaNotificacion)}</td>
        <td>
          {rowInfo.data.observacionesNotificacion &&
            <div>
              <FontAwesomeIcon icon="comment" fixedWidth size="1x" />
            </div>
          }

        </td>
      </tr>
      {rowInfo.data.observacionesNotificacion &&
        <tr className="notes-row">
          <td colSpan="6"><div>{rowInfo.data.observacionesNotificacion}</div></td>
        </tr>
      }
    </tbody>
  );
}