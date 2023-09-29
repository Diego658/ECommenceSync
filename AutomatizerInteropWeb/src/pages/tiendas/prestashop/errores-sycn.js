import React, { useState } from 'react';
import { SpliContainer } from '../../../components/splitContainer/split-container';
import { DataGrid } from 'devextreme-react';
import { useCallback } from 'react';
import { PrestashopService } from '../../../_services/stores/prestashop-service';
import DataSource from 'devextreme/data/data_source';

export default function PrestashopErroresSincronizacion(props) {
  const [loading, setLoading] = useState(true);
  const [listado, setListado] = useState(null);

  const cargarErrores = useCallback(async ()=>{
    setLoading(true);
    try {
      const errores = await PrestashopService.getErroresSync();
      setListado(errores);
    } catch (error) {
      
    }
    setLoading(false);
  }, []);

  const gridSource = new DataSource({
    store: {
      type: 'array',
      data: listado
    },
    sort: { getter: 'grupoVencimientoNumero ', desc: false }
  });

  return (
    <>
      <SpliContainer
        leftContent={
          <>
            <DataGrid

            >

            </DataGrid>
          </>}
      >

      </SpliContainer>
    </>
  )
}