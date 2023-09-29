import React, { useState, useEffect } from 'react'
import DataGrid, {
  Column,
  SearchPanel,
  Scrolling,
  FilterRow,
  FilterPanel,
  StateStoring
} from 'devextreme-react/data-grid';
import { CustomLoadIndicator } from '../../../../components';
import { InventarioService } from '../../../../_services/inventario.service';
import './productos-sin-imagen.scss'
import { ArbolInventarioItem } from '../../../../components/arbol-inventario/arbol-inventario-item'
import { ScrollView } from 'devextreme-react';
import { SpeedDialAction } from 'devextreme-react/speed-dial-action';
import { useCallback } from 'react';


export function ProductosSinImagen() {
  const [productos, setProductos] = useState([]);
  const [productoSeleccionado, setproductoSeleccionado] = useState(null);
  const [loading, setLoading] = useState(true);
  const [gridHeight] = useState(window.innerHeight - 250);

  const onGridSelectionChanged = ({ selectedRowsData }) => {
    const data = selectedRowsData[0];
    setproductoSeleccionado(data);
  }

  const cargarDatos = useCallback( () => {
    setLoading(true);
    InventarioService.getProductosSinImagen().then(response => {
      setProductos(response);
      setproductoSeleccionado(null);
      setLoading(false);
    }, error => {

    })
  }, [])


  useEffect(() => {
    cargarDatos();
  }, [cargarDatos])


  if (loading) {
    return (
      <React.Fragment>
        <CustomLoadIndicator message={"Cargando..."} title={"Productos sin imagen..."} />
      </React.Fragment>
    );
  }

  return (
    <>
      <SpeedDialAction
        icon="refresh"
        index={1}
        visible={true}
        onClick={() => {
          cargarDatos();
        }} />

      <h3 className={'content-block'}>Productos sin imagen</h3>
      <div className={'content-block'}>
        <div className={'dx-card responsive-paddings'}>
          <div className="gridContainer">
            <div className="listadoProductos">
              <ScrollView
                height="100%"
              >
                <DataGrid
                  dataSource={productos}
                  keyExpr="itemID"
                  allowColumnReordering={false}
                  allowColumnResizing={true}
                  showBorders={true}
                  rowAlternationEnabled={false}
                  selection={{ mode: 'single' }}
                  onSelectionChanged={onGridSelectionChanged}
                  focusedRowEnabled={true}
                  height={gridHeight}
                >
                  <Scrolling mode="virtual" />
                  <SearchPanel />
                  <FilterRow visible={true} />
                  <FilterPanel visible={true} />
                  <StateStoring enabled={true} type="localStorage" storageKey="productos-sin-imagen-grid" />
                  <Column dataField="nombre" />
                  <Column dataField="referencia" width={150} />
                  <Column dataField="existenciaTotal" dataType='number' width={50} />
                </DataGrid>
              </ScrollView>
            </div>

            <div className="detalleProducto">
              <ArbolInventarioItem itemId={productoSeleccionado === null ? null : productoSeleccionado.itemID} ></ArbolInventarioItem>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}