import React from 'react';
import { InventarioService } from '../../_services/inventario.service';
import { TreeList, Column } from 'devextreme-react/tree-list';

export function InventarioTreeList(props){

    const dataSource = {
        load: function(loadOptions) {
          //console.log(loadOptions);
          let parentIdsParam = loadOptions.parentIds;
          return InventarioService.getSubitems('',1, null)
            .then(response => response)
            .catch((error) => { throw error; });
        }
    };

    return(
        <>
            <TreeList
                dataSource={dataSource}
                showBorders={true}
                columnAutoWidth={false}
                wordWrapEnabled={true}
                keyExpr="codigo"
                parentIdExpr="codigoPadre"
                hasItemsExpr="hasItems"
                rootValue=""
            >
                <Column dataField="codigo" width={80} />
                <Column dataField="nombre" width={100} />
                <Column  dataField="referencia" />
            </TreeList>
        </>
    );
}