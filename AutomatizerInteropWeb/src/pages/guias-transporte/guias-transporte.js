import React, { useState } from 'react';
import { GuiasTransporteFiltros } from './guias-transporte-filtros';
import DataGrid, { Summary, TotalItem, Column, StateStoring, Export, ColumnChooser, ColumnFixing } from 'devextreme-react/data-grid';
import { GuiasTransporteService } from '../../_services/guiasTransporte.service';
import GuiasCellVerGuiaTransporte from './guias-cell-ver-guia-transpoprte';

export function GuiasTransporte() {
  //const [loading, setloading] = useState(false);
  const [guias, setGuias] = useState([]);

  const cargarDatos = async (filtros) => {
    try {
      const list = await GuiasTransporteService.getGuias(filtros);
      setGuias(list);
      //setloading(false);
    } catch (error) {

    }

  }



  return (
    <div className='dx-card' style={{ padding: '10px' }}>
      <GuiasTransporteFiltros cargarDatos={cargarDatos} />
      <DataGrid
        wordWrapEnabled={true}
        focusedRowEnabled={true}
        allowColumnReordering={true}
        allowColumnResizing={true}
        showBorders={true}
        keyExpr='id'
        dataSource={guias}
        height={window.innerHeight - 350}
      >
        <ColumnChooser enabled={true} />
        <ColumnFixing enabled={true} />
        <Export enabled={true} fileName={`Guias transporte`} allowExportSelectedData={true} />
        <StateStoring enabled={true} type="localStorage" storageKey="guias-listado-grid" />
        <Column dataField='id' cellRender={verGuiaCellRender}></Column>
        <Column dataField='fecha' dataType='date' ></Column>
        <Column dataField='numeroGuia'></Column>
        <Column dataField='compania'></Column>
        <Column dataField='cliente' ></Column>
        <Column dataField='NumeroFactura'></Column>
        <Column dataField='numeroPiezas'></Column>
        <Column dataField='cobrado'></Column>
        <Column dataField='entregado'></Column>
        <Column dataField='numeroPiezas'></Column>

        <Column dataField='costoEnvio' dataType='currency' ></Column>
        <Summary>
          <TotalItem
            column="numeroGuia"
            summaryType="count" />
          <TotalItem
            column="costoEnvio"
            summaryType="sum"
             />
        </Summary>
      </DataGrid>
    </div>
  );
}

const verGuiaCellRender = ((data) => {
  return (
    <>
      <GuiasCellVerGuiaTransporte id={data.data.id}  />
    </>
  )
})