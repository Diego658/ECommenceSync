import React from 'react';
import ContextMenu from 'devextreme-react/context-menu';
import { useState } from 'react';
import { Popup } from 'devextreme-react';
import ArbolInventarioAgregarCategoria from './arbol-inventario-agregar-categoria';
import { useRef } from 'react';
import ArbolInventarioAgregarProducto from './arbol-inventario-agregar-producto';
import ArbolInventarioDuplicarProducto from './arbol-inventario-duplicar-producto';


export default function ArbolInventarioTreeViewContextMenu({ item, idDiv }) {
  const [show, setShow] = useState(false);
  const [mode, setMode] = useState(0);

  const onMenuItemClick = async (button, item) => {
    //console.log(item);
    //await item.component.selectRows([8690], false);
    //await item.component.refresh();
    setMode(button.id);
    setShow(true);
  };


  const onCreated = async (item, newCategory) => {
    //console.log(item);
    //console.log(newCategory);
    setShow(false);
    await item.component.refresh();
    await item.component.selectRows([newCategory.itemID], false);
    await item.component.selectRows([newCategory.itemID], false);
    const index = await item.component.getRowIndexByKey(newCategory.itemID);
    const rowElement = await item.component.getRowElement(index);
    await item.component.getScrollable().scrollToElement(rowElement);

  }


  const onCanceled = () => {
    setShow(false);
  }


  const menuItems = [];
  if (item.data.tipo === 'G') {
    if (item.data.nivel + 1 < item.data.maximoNiveles) {
      menuItems.push({ id: 1, text: 'Agregar Subcategoría', icon: 'dx-icon-add' });
    }
    menuItems.push({ id: 2, text: 'Agregar Producto', icon: 'dx-icon-add' });
  }

  if (item.data.tipo === 'I') {
    menuItems.push({ id: 3, text: 'Duplicar Producto', icon: 'dx-icon-copy' });
  }



  return (
    <>
      <ContextMenu
        dataSource={menuItems}
        width={200}
        target={`#${idDiv}`}
        onItemClick={async (e) => await onMenuItemClick(e.itemData, item)} />

      <Popup
        title={`${mode===1 || mode === 2? "Agregar ": "Duplicar"} ${mode === 2 || mode === 3 ? 'Producto' : 'Categoría'}`}
        width={mode === 1 ? 600 : 700}
        height={mode === 1 ? 270 : 290}
        closeOnOutsideClick={false}
        showTitle={true}
        showCloseButton={false}
        visible={show}
      >
        <div id='agregarItemInventario'>
          {show &&
            <>
              {mode === 1 &&
                <ArbolInventarioAgregarCategoria padreId={item.data.itemID} onCreated={(n) => onCreated(item, n)} onCanceled={onCanceled} />
              }
              {mode === 2 &&
                <ArbolInventarioAgregarProducto padreId={item.data.itemID} onCreated={(n) => onCreated(item, n)} onCanceled={onCanceled} />
              }
              {mode === 3 &&
                <ArbolInventarioDuplicarProducto origenId={item.data.itemID} onCreated={(n) => onCreated(item, n)} onCanceled={onCanceled} />
              }
            </>
          }
        </div>
      </Popup>
    </>

  );
}