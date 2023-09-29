import React from 'react';
import { RenderTreeItem } from './arbol-inventario-tree-item'
import AspNetData from 'devextreme-aspnet-data-nojquery';
import { config } from '../../constants/constants'
import TreeList, { RemoteOperations, Column, HeaderFilter, Selection, FilterPanel, FilterRow, StateStoring, SearchPanel } from 'devextreme-react/tree-list';
import { Template } from 'devextreme-react/core/template';
import { authHeader } from '../../_helpers';
import { useMemo } from 'react';
import { useCallback } from 'react';
import UserSesion from '../../utils/userSesion';
import './arbol-inventario-item.scss';

export function ArbolInventarioLeftPanel({selectItem, height}) {
  const auth = useMemo(() => authHeader(), []);

  const tasksData = useMemo(() => {
    return AspNetData.createStore({
      key: 'itemID',
      loadUrl: `${config.url.API_URL}/api/ProductsSearch/ArbolInventarioItems`,
      loadMode: 'processed',
      cacheRawData: false,
      onBeforeSend: function (method, ajaxOptions) {
        method = "GET";
        ajaxOptions.xhrFields = { withCredentials: false };
        ajaxOptions.headers = {
          'Content-Type': 'application/json',
          'Authorization': auth.Authorization,
          'ConfiguracionId': UserSesion.getConfiguracion().idConfiguracionPrograma
        };
      }
    });
  }, [auth]);

  //const tasksData = 


  const onFocusedRowChanged = useCallback(({ selectedRowsData })=>{
    var rowData = selectedRowsData[0];
    if (rowData) {
      //console.log('Select ' + JSON.stringify( rowData));
      selectItem(rowData);
      console.log('leftpanel-focusedrowchanged', rowData)
    }
  }, [selectItem] );
  
  return (
    <TreeList
      id="tree-list"
      dataSource={tasksData}
      keyExpr="itemID"
      parentIdExpr="padreID"
      hasItemsExpr="tieneItems"
      showRowLines={true}
      showBorders={true}
      columnAutoWidth={true}
      wordWrapEnabled={true}
      focusedRowEnabled={false}
      onSelectionChanged={onFocusedRowChanged}
      virtualModeEnabled={false}
      height={height}>

      <StateStoring enabled={true} type="localStorage" storageKey="treeListCodificacionInventario" />
      <RemoteOperations filtering={true} sorting={false} grouping={false} />
      <SearchPanel visible={true} searchVisibleColumnsOnly={true} />
      <Selection mode="single" />
      <Column dataField="nombre" minWidth={350} cellTemplate="itemTemplate" />
      <Column dataField="referencia" cellTemplate={"referenceTemplate"} />
      <Column dataField="Tipo" visible={false} dataType="string" />
      <Column dataField="ExistenciaTotal" visible={false} dataType="number" />
      <Column dataField="TieneImagenes" visible={false} dataType="boolean" />
      <Template name="itemTemplate" render={RenderTreeItem} />
      <Template name="referenceTemplate" render={RenderTreeItemReference} />
    </TreeList>
  )
}

function RenderTreeItemReference(data){
  return(<span style={{fontSize:'11px'}}>{data.data.referencia}</span> );
}
