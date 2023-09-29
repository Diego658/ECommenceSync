import React, { useMemo } from 'react';
import { Popover, ContextMenu } from 'devextreme-react';
import { useState } from 'react';
import notify from 'devextreme/ui/notify';
import { ArbolInventarioItemImagenes } from '../arbol-inventario/arbol-inventario-item-imagenes';

export default function BusquedaItemsNombreItemRender({ item, precioConIva }) {
  const [show, setShow] = useState(false);

  const contexMenuItemClick = (buttonId, itemData) => {
    if(buttonId===5){ //Ver imagenes
      setShow(true);
      return;
    }

    let text;
    switch (buttonId) {
      case 2: //Copiar Precio eventual
        text = `[${itemData.referencia}] - ${itemData.nombre} [STOCK ${itemData.b1}] ${itemData.eventual} ${precioConIva ? 'IVA INCLUIDO' : '+ IVA'}`;
        break;
      case 3: //Copiar Precio Mayorista
        text = `[${itemData.referencia}] - ${itemData.nombre} [STOCK ${itemData.b1}] ${itemData.mayorista} ${precioConIva ? 'IVA INCLUIDO' : '+ IVA'}`;
        break;
      case 4: //Copiar Precio Emp. Publica
        text = `[${itemData.referencia}] - ${itemData.nombre} [STOCK ${itemData.b1}] ${itemData.empPublica} ${precioConIva ? 'IVA INCLUIDO' : '+ IVA'}`;
        break;
      default:
        break;
    }
    if (itemData.linkWeb !== "") {
      text = text + '\n' + itemData.linkWeb;
    }
    navigator.clipboard.writeText(text);
    notify('Precio copiado!!!', 'success', 2000);
  }

  return (
    <>
      <span id={item.codigo} >{item.nombre}</span>
      <ContextMenu
        target={`#${item.codigo}`}
        items={contextMenuItems}
        onItemClick={(e) => {
          contexMenuItemClick(e.itemData.id, item);
        }}
      />
      <Popover
        visible={show}
        closeOnOutsideClick={true}
        onHiding={()=> setShow(false)}
        position='bottom'
        width={800}
        height={400}
        target={`#${item.codigo}`}
      >
        <div>
          {show &&
            <ArbolInventarioItemImagenes itemId={item.itemId} />
          }
        </div>
      </Popover>
    </>
  );
}

const contextMenuItems = [
  {
    text: 'Precio',
    id: 1,
    items: [
      { text: 'Copiar Eventual', id: 2 },
      { text: 'Copiar Mayorista', id: 3 },
      { text: 'Copiar Emp. Publica', id: 4 },
    ]
  },
  {
    text: 'Ver Imagenes',
    id:5
  },
];

