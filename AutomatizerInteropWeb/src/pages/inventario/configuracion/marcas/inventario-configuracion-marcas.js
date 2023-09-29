import React, { useState, useEffect } from 'react'
import '../../inventario.scss'
import DataGrid, {
  Column,
  SearchPanel,
  Scrolling,
} from 'devextreme-react/data-grid';
import { InventarioService } from '../../../../_services/inventario.service';
import { InventarioConfiguracionMarca } from './inventario-configuracion-marca';

export function InventarioConfiguracionMarcas({gridHeight}) {
  const [marcas, setMarcas] = useState([]);
  const [marca, setMarca] = useState(null);
  const cargarDatos = () => {
    InventarioService.getMarcas().then(response => {
      setMarcas(response);
      setMarca(null);
    }, error => {

    })
  };

  useEffect(() => {
    cargarDatos();
  }, []);


  const onGridSelectionChanged = ({ selectedRowsData }) => {
    const data = selectedRowsData[0];
    setMarca(data);
  }

  return (
    <div className="splipContainer">
      <div className="splitContainer-col1">
        <DataGrid
          dataSource={marcas}
          keyExpr='codigo'
          height={gridHeight}
          selection={{ mode: 'single' }}
          onSelectionChanged={onGridSelectionChanged}
          showBorders={true}
          focusedRowEnabled={true}
        >
          <Scrolling mode="virtual" />
          <SearchPanel searchVisibleColumnsOnly={true} visible={true} />
          <Column dataField='codigo' allowSearch={false} visible={false} />
          <Column dataField='nombre' />
        </DataGrid>
      </div>
      <div className="splitContainer-col2">
        {marca &&
          <InventarioConfiguracionMarca marcaId={marca.id} />
        }
      </div>
    </div>
  )
}