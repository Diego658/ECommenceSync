import React, { useCallback, useState } from 'react';
import './arbol-inventario.scss'
import { ArbolInventarioLeftPanel } from './arbol-inventario-left-panel'
import { ArbolInventarioContentPanel } from './arbol-inventario-content-panel'



const ArbolInventario=({ height }) => {
  const [itemData, setItemData] = useState(null);

  const onSelectedItemChanged = useCallback((item) => {
    setItemData(item)
  }, [])

  return (
    <div className='content block' style={{ display: 'grid', width: '100%', height: '100%', gridTemplateColumns: `40% 60%`, gridGap: '5px' }} >
      <div>
        <ArbolInventarioLeftPanel selectItem={onSelectedItemChanged} height={height} />
      </div>
      <div>
        <ArbolInventarioContentPanel item={itemData} height={height} />
      </div>
    </div>
  );

}

export default React.memo(ArbolInventario);