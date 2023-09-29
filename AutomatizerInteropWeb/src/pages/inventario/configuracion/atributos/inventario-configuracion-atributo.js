import React, { useState, useMemo } from 'react'
import { useEffect } from 'react';
import { useCallback } from 'react';
import { InventarioAtributosService } from '../../../../_services/inventario/atributos-service';
import notify from 'devextreme/ui/notify';
import DataGrid, {
  Column,
  SearchPanel,
  Scrolling,
  Editing,
  Lookup,
  RequiredRule, 
  StringLengthRule,
  AsyncRule} from 'devextreme-react/data-grid';
import { InventarioDatasourceService } from '../../../../_services/inventario.datasouce.service';
import { InventarioValidationService } from '../../../../_services/inventario.validation.service';
import { ColorEditBoxGridCell } from './colorbox-edit-cell';
import { ColorBoxRenderGridCell } from './colorbox-render-cell';


export function InventarioConfiguracionAtributo({  gridHeight , atributoId, showColor }) {
  const [atributo, setAtributo] = useState(null);


  const cargarDatos = useCallback(async (id)=>{
    const datos = await InventarioAtributosService.getAtributo(id);
    return datos;
  }, []);


  const cargarAtributo= useCallback( async (id)=>{
    try {
      const data = await cargarDatos(id);
      setAtributo(data);
    } catch (error) {
      setAtributo(null);
      notify(`Error al cargar atributo, error ${error}`, 'error', 3000);
    }
  }, [cargarDatos]);


  useEffect(()=>{
    if(atributoId === undefined){
      setAtributo(null);
      return;
    }
    cargarAtributo(atributoId);
  }, [atributoId, cargarAtributo])


  const gridDatasource = useMemo(()=>{
    if(atributoId === undefined){
      return null;
    }
    return InventarioDatasourceService.dataStoreAtributosValues(atributoId);
  }, [atributoId])


  

  
  if (atributoId ===  undefined || atributo === null){
    return (<></>);
  }

  return (
    <>
    <DataGrid
          keyExpr='id'
          dataSource={gridDatasource}
          selection={{ mode: 'single' }}
          showBorders={true}
          focusedRowEnabled={true}
          height={gridHeight}
        >
          <Editing
            mode="row"
            allowAdding={true}
            allowDeleting={true}
            allowUpdating={true}
          />
          <Scrolling mode="virtual" />
          <SearchPanel searchVisibleColumnsOnly={true} visible={true} />
          <Column dataField='id' visible={false} />
          <Column dataField='valor' caption='Valor' >
            <RequiredRule message="Ingrese el valor." />
            <StringLengthRule max={150} message="La longitud maxima es 150." />
            <AsyncRule
              message="Ya existe el atributo."
              validationCallback={(params)=>InventarioValidationService.atributoValorValidationName(params, atributoId)}
            />
          </Column>
          <Column dataField='orden' caption='Orden' width={100} />
          <Column dataField='color' visible={showColor} width={200} editCellRender={ColorEditBoxGridCell} cellRender={ColorBoxRenderGridCell}  allowSorting={false} >
            {showColor  &&
              <RequiredRule/>
            }
          </Column>

        </DataGrid>
    </>
  );
}



