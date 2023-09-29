import React, { useState } from 'react'
import DataSource from 'devextreme/data/data_source';

import DataGrid, {
  Column
} from 'devextreme-react/data-grid';
import { useEffect } from 'react';
import { BodegaService } from '../../_services/bodega.service';
import notify from 'devextreme/ui/notify';

export function ArbolInventarioItemBodegas({ itemId, height }) {
  const [detalleBodegas, setdetalleBodegas] = useState([]);


  const dataSource = new DataSource({
    store: {
      key: 'codigoBodega',
      type: 'array',
      mode: 'raw',
      data: detalleBodegas
    }
  });

  useEffect(() => {
    if (itemId === 0) {
      setdetalleBodegas([]);
    }

    BodegaService.getItemDetalleBodegas(itemId).then((response) => {
      setdetalleBodegas(response);

    },
      error => {
        setdetalleBodegas([]);
        notify("Error al cargar el detalle de bodegas!!!", 'error', 1500);

      })
  }, [itemId])

  return (
    <>
      <div className='content-row'>
        <DataGrid
          dataSource={dataSource}
          height={height}
          width='95%'
        >
          <Column dataField="seleccionada" type="boolean" width={40} caption="" allowEditing={true} />
          <Column dataField="nombreBodega" width={300} allowEditing={false} />
          <Column dataField="existencia" width={80} allowEditing={false} />
          <Column dataField="existenciaMaxima" width={100} allowEditing={true} dataType="number" caption="Máxima" />
          <Column dataField="existenciaMinima" width={100} allowEditing={true} dataType="number" caption="Mínima" />
          <Column dataField="costoActual" width={100} allowEditing={false} caption="Costo" />
        </DataGrid>
      </div>

    </>
  );
}