import React from 'react'
import './arbol-inventario.scss'
import ArbolInventarioTreeViewContextMenu from './arbol-inventario-treeview-contextmenu';



export function RenderTreeItem(item) {
  const iconName = item.data.tipo === 'G' ? 'activfolder' : (item.data.existenciaTotal <= 0 ? 'close textoSinExistencia' : 'check textoConExistencia');
  const idDiv = `treeItems${item.data.codigo}`;

  return (
    <div id={idDiv} className="treeItem box">
      <div className="box-item">
        <i className={"dx-icon-" + iconName}></i>
      </div>
      {!item.data.tieneImagenes &&
        <div className="box-item">
          <i className={"dx-icon-photo dx-icon-noimage"}></i>
        </div>
      }
      <div className="box-item" style={{fontSize:12}} >{item.data.nombre}</div>
      <div className="push">
        <span >{item.data.existenciaTotal}</span>
      </div>
      <ArbolInventarioTreeViewContextMenu item={item} idDiv={idDiv} />
    </div>
  );
}