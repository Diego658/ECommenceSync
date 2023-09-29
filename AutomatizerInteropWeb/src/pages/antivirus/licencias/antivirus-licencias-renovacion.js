import React from 'react'
import Form, { Tab, TabbedItem } from 'devextreme-react/form';
import { AntivirusLicenciasPendientes } from './antivirus-licencias-pendientes';
import { AntivirusLicenciasRenovaciones } from './antivirus-licencias-renovaciones';


export function AntivirusLicenciasRenovacion() {


  return (
    <>
      <div className={'content-block'}>
        <div className={'dx-card'}>
          <Form>
            <TabbedItem>
              <Tab title="Renovaciones">
                <div className="contenidoTab">
                  <AntivirusLicenciasRenovaciones />
                </div>
              </Tab>
              <Tab title="Pendientes (Por caducar)">
                <div className="contenidoTab">
                  <AntivirusLicenciasPendientes tipoAVisualizar={0} />
                </div>
              </Tab>
              <Tab title="Pendientes (Confirmados)">
                <div className="contenidoTab">
                  <AntivirusLicenciasPendientes tipoAVisualizar={1} />
                </div>
              </Tab>
              <Tab title="Pendientes (No localizados)">
                <div className="contenidoTab">
                  <AntivirusLicenciasPendientes tipoAVisualizar={2} />
                </div>
              </Tab>
            </TabbedItem>
          </Form>
        </div>
      </div>
    </>
  );
}