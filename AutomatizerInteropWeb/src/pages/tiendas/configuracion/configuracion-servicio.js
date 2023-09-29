import React, { useState } from 'react'
import './configuracion.scss'
import { Button } from 'devextreme-react';
import { LoadIndicator } from 'devextreme-react/load-indicator';
import { InventarioService } from '../../../_services/inventario.service';


export function ConfiguracionServicio(props) {
  const [migrado, setMigrado] = useState(false);
  const [migrando, setMigrando] = useState(false);
  return (
    <React.Fragment>
      <div>
        <Button
          width={300}
          type='default'
          stylingMode='contained'

          onClick={async () => {
            if (migrando) return;
            if (migrado) {
              alert('Ya migrado!!!');
              return;
            }

            setMigrando(true);
            await InventarioService.migrarImagenesCategoriasAutomatizerToBlob();
            setMigrado(true);
            setMigrando(false);
          }}
        >
          <LoadIndicator className="button-indicator" visible={migrando} />
          <span className="dx-button-text">{(migrando ? 'Migrando...' : 'Migrar imagenes categorias a Blob')}</span>
        </Button>
      </div>
      <div>
        <Button
          width={300}
          type='default'
          stylingMode='contained'

          onClick={async () => {
            if (migrando) return;
            if (migrado) {
              alert('Ya migrado!!!');
              return;
            }

            setMigrando(true);
            await InventarioService.migrarImagenesItemsAutomatizerToBlob();
            setMigrado(true);
            setMigrando(false);
          }}
        >
          <LoadIndicator className="button-indicator" visible={migrando} />
          <span className="dx-button-text">{(migrando ? 'Migrando...' : 'Migrar imagenes items a Blob')}</span>
        </Button>
      </div>

    </React.Fragment>
  );
}