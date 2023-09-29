import React from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'


export default function ClienteCell(cellData) {
  var idComment = `comment${cellData.data.kardexId}`;
  return (
    <div className={cellData.data.emailCliente === '' ? 'emailEmptyText' : ''} >
      {cellData.data.observacionesNotificacion &&

        <FontAwesomeIcon icon="comment" fixedWidth size="1x" id={idComment} />
      }
      {cellData.data.cliente}
    </div>
  );
}