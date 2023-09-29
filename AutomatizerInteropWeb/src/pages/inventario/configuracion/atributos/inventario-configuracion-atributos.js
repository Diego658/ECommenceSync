import React, { useState } from 'react'
import DataGrid, {
  Column,
  SearchPanel,
  Scrolling,
  Editing,
  Lookup,
  RequiredRule,
  StringLengthRule,
  AsyncRule, StateStoring
} from 'devextreme-react/data-grid';
import { useMemo } from 'react';
import { InventarioDatasourceService } from '../../../../_services/inventario.datasouce.service';
import { InventarioValidationService } from '../../../../_services/inventario.validation.service';
import { InventarioConfiguracionAtributo } from './inventario-configuracion-atributo';
import { useCallback } from 'react';


const tiposAtributo = [
  { name: 'Drop-Down', id: 0 },
  { name: 'Radio-Button', id: 1 },
  { name: 'Color Or Texture', id: 2 }
]

export function InventarioConfiguracionAtributos({ gridHeight }) {
  const [atributo, setAtributo] = useState({});
  const [nuevo, setNuevo] = useState(false);

  const gridDatasource = useMemo(() => {
    return InventarioDatasourceService.dataStoreAtributos();
  }, []);

  const onGridSelectionChanged = useCallback(({ selectedRowsData }) => {
    const data = selectedRowsData[0];
    setAtributo(data ?? {});
  }, []);


  return (
    <div className="splipContainer2">
      <div className="splitContainer-col1">
        <DataGrid
          keyExpr='id'
          dataSource={gridDatasource}
          selection={{ mode: 'single' }}
          showBorders={true}
          focusedRowEnabled={true}
          height={gridHeight}
          hoverStateEnabled={true}
          onSelectionChanged={onGridSelectionChanged}
        >
          <StateStoring enabled={true} type="localStorage" storageKey="inventarioConfiguracionAntributos-grid" />
          <Editing
            mode="row"
            allowAdding={true}
            allowDeleting={true}
            allowUpdating={true}
          />
          <Scrolling mode="virtual" />
          <SearchPanel searchVisibleColumnsOnly={true} visible={true} />
          <Column dataField='id' visible={false} />
          <Column dataField='name' caption='Nombre' >
            <RequiredRule message="Ingrese el nombre." />
            <StringLengthRule max={250} message="La longitud maxima es 250." />
            <AsyncRule
              message="Ya existe el atributo."
              validationCallback={InventarioValidationService.atributosValidationName}
            />
          </Column>
          <Column dataField='publicName' caption='Nombre Publico' >
            <RequiredRule message="Ingrese el nombre publico." />
            <StringLengthRule max={250} message="La longitud maxima es 250." />
          </Column>
          <Column dataField='attributeType' caption='Tipo'  >
            <RequiredRule message="Ingrese el tipo de atributo." />
            <StringLengthRule max={250} message="La longitud maxima es 25." />
            <Lookup dataSource={tiposAtributo} displayExpr='name' valueExpr='id' />
          </Column>
        </DataGrid>
      </div>
      <div className='splitContainer-col2'>
        <InventarioConfiguracionAtributo atributoId={atributo.id} showColor={atributo.attributeType === 2} />
      </div>
    </div>
  );
}

