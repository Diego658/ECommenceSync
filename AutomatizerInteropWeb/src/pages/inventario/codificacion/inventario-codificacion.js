import React, { useState } from 'react'
import ArbolInventario  from '../../../components/arbol-inventario/arbol-inventario';

const InventarioCodificacion=(props)=> {
  const [gridHeight] = useState(window.innerHeight - 100 );
  return (
    <React.Fragment>
      <div className={'content-block'} style={{height:gridHeight}} >
        <div className={'dx-card'}  >
          <ArbolInventario height={gridHeight-20} />
        </div>
      </div>
    </React.Fragment>
  );
}

export default React.memo(InventarioCodificacion);