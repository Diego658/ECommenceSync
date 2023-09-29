import React from 'react';
import './notificaciones.scss'

export default function NumeroNotificacionesCell(cellData) {
  return (
    <div >
        {cellData.data.numeroNotificaciones}
    </div>
  );
}