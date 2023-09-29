import React from 'react';
import { ConfiguraPlantillasEmail } from './configuraPlantillasEmail'
import Form, { Tab, TabbedItem } from 'devextreme-react/form';
import { ConfigurarNotificaciones } from './configuracionNotificaciones';

export function AntivirusConfiguracion() {
  return (
    <React.Fragment>
      <div className={'dx-card'} style={{height: '100%'}} >
        <Form
          height='100%'
        >
          <TabbedItem>
            <Tab title="Configuración">
              <div >
                <ConfigurarNotificaciones />
              </div>
            </Tab>
            <Tab title="Plantillas">
              <div style={{height:window.innerHeight -200}} >
                <ConfiguraPlantillasEmail />
              </div>
            </Tab>
          </TabbedItem>
        </Form>
      </div>
    </React.Fragment>
  );
}