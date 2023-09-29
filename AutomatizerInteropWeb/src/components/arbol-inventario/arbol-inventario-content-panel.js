import React, { useState } from 'react';
import './arbol-inventario.scss'
import { ArbolInventarioCategoria } from './arbol-inventario-categoria'
import { ArbolInventarioItem } from './arbol-inventario-item';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';


export function ArbolInventarioContentPanel({ item, height }) {

  if (item == null) {
    return (
      <div className='infoContainer'>
        <FontAwesomeIcon icon='info-circle' className='fa-4x infoicon' />
        <p className='infoText'>
          Seleccione un elemento del árbol izquierdo para visualizar.
        </p>
      </div>
    );
  }
  return (
    <React.Fragment key='ArbolInventarioContentPanel'>
      {item.tipo === "G" &&
        <React.Fragment key= {`arbolInventarioCategoria`}>
          <ArbolInventarioCategoria  itemId={item.itemID} />
        </React.Fragment>

      }
      {item.tipo === "I" &&
        <React.Fragment key= {`arbolInventarioItem`}> 
          <ArbolInventarioItem itemId={item.itemID} height={height} ></ArbolInventarioItem>
        </React.Fragment>
      }

    </React.Fragment>
  );
}


