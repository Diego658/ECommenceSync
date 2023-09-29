import './arbol-inventario.scss';
import React, { useState, useEffect, useMemo } from 'react'
import TagBox from 'devextreme-react/tag-box';
import { Button } from 'devextreme-react';
import DataGrid, {
  Column,
  SearchPanel,
  Scrolling,
  Editing,
  RequiredRule,
  NumericRule
} from 'devextreme-react/data-grid';
import { InventarioDatasourceService } from '../../_services/inventario.datasouce.service';
import { SpliContainer } from '../splitContainer/split-container';
import List from 'devextreme-react/list';
import DataSource from 'devextreme/data/data_source';
import { ArbolInventarioAtributoInfo } from './atributo-info';
import { FechHelper } from '../../_helpers/fech-helper';
//import ArrayStore from 'devextreme/data/array_store';


export function ArbolInventarioItemCombinaciones({ itemId, item, height }) {
  const [atributosSeleccionados, setAtributosSeleccionados] = useState([]);
  const [atributos, setAtributos] = useState([]);



  // const tagDatasource = new DataSource({
  //   store: atributos,
  //   group: 'atributoNombre'
  // });



  const listDataSource = useMemo(() => {
    const store = InventarioDatasourceService.dataStoreAtributosWithValuesGrouped();
    const source = new DataSource({
      store: store,
      paginate: false,
      
    });
    return source;
  }, []);

  const gridDatasource = useMemo(() => {
    if (itemId === undefined) {
      return null;
    }
    return InventarioDatasourceService.dataStoreAtributosValues(itemId);
  }, [itemId]);

  useEffect(() => {
    FechHelper.get('Inventario', 'AtributoValores/GetAllAtributesWithValues').then(data => setAtributos(data));
  }, []);

  const leftContent = useMemo(() => {
    return (
      <DataGrid
        keyExpr='id'
        dataSource={gridDatasource}
        selection={{ mode: 'single' }}
        showBorders={true}
        focusedRowEnabled={true}
        width={'100%'}
        height={height - 320}
      >
        <Editing
          mode='row'
          allowDeleting={true}
          allowUpdating={true}
        />
        <Scrolling mode="virtual" />
        <SearchPanel searchVisibleColumnsOnly={true} visible={false} />
        <Column dataField='id' visible={false} />
        <Column dataField='nombreCombinacion' caption='Combinaciones' allowEditing={false} width={350} />
        <Column dataField='impactoPrecio' caption='Impacto en el Precio' allowEditing={true} width={150} dataType='number' >
          <RequiredRule />
          <NumericRule />
        </Column>
        <Column dataField='precioFinal' caption='Precio Final' allowEditing={false} width={150} dataType='number' />
        <Column dataField='predeterminada' caption='Predeerminada' allowEditing={true} width={100} />
      </DataGrid>
    );
  }, [gridDatasource, height])





  const rigthContent = useMemo(() => {
    return (
      <>
        <List
          dataSource={listDataSource}
          height={height - 320}
          grouped={true}
          selectionMode="multiple"
          showSelectionControls={true}
          displayExpr={'fullName'}
          keyExpr={'valorId'}
          collapsibleGroups={true}
          selectedItemKeys={atributosSeleccionados}
          searchExpr="fullName"
          searchEnabled={false}
          
          onSelectionChanged={(eventData) => {
            const seleccionados = atributosSeleccionados;
            for (let index = 0; index < eventData.addedItems.length; index++) {
              const element = eventData.addedItems[index];
              seleccionados.push(element.valorId);
            }
            for (let index = 0; index < eventData.removedItems.length; index++) {
              const element = eventData.removedItems[index];
              let elementIndex = seleccionados.findIndex((value)=> element.valorId===value);
              if(elementIndex>=0){
                seleccionados.splice(elementIndex,1);
              }
            }
            setAtributosSeleccionados(seleccionados.slice(0));
          }}
          itemRender={ArbolInventarioAtributoInfo}
        />
      </>
    );
  }, [height, listDataSource, atributosSeleccionados])




  return (
    <>
      
      <div className='content-row generarCombinaciones'>
        <div >
          <TagBox
            value={atributosSeleccionados}
            dataSource={atributos}
            displayExpr='fullName'
            valueExpr='valorId'
            onValueChanged={(value) => { setAtributosSeleccionados(value.value);  }}
            placeholder='Combina varios atributos, ej.: "Talla: todas", "Color: rojo".'
            grouped={false}

          />
        </div>
        <div>
          <Button
            text={`Generar Combinaciones (${atributosSeleccionados.length})`}
            icon=''
            disabled={atributosSeleccionados && atributosSeleccionados.length<1}
          >
          </Button>
        </div>
      </div>
      <SpliContainer leftWidth={'60%'} rigthWidth={'40%'} leftContent={leftContent} rigthContent={rigthContent} >

      </SpliContainer>
    </>
  )
}


