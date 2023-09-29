import React from 'react'
import './arbol-inventario-item.scss'
import { CustomHtmlEditor } from '../html-editor/html-Editor';

export function ArbolInventarioItemPaginaWeb({ item, updateDataFunction, height, updateHtmlEditorData }) {


  return (
    <>
      <h4>Información</h4>
      <CustomHtmlEditor markup={item['Description-Short-Html']} height='100%' useToolBar={true} updateMarkupFunction={(value) => updateDataFunction({ ...item, 'Description-Short-Html': value }, 'Description-Short-Html')} >

      </CustomHtmlEditor>

      <h4>Detalle</h4>
      <CustomHtmlEditor markup={item['Description-Html']} height='100%' useToolBar={true} updateMarkupFunction={(value) => updateDataFunction({ ...item, 'Description-Html': value }, 'Description-Html')} >

      </CustomHtmlEditor>

    </>
  );


}