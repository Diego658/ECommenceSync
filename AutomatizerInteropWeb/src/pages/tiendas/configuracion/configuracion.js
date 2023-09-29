import React from 'react';
import { ConfiguracionServicio } from './configuracion-servicio'
//import Form, { TabbedItem, Tab } from 'devextreme-react/form';
import { ConfiguracionCategoriasSync } from './configuracion-categorias-sync';
//import { Tab, Tabs, TabList, TabPanel } from 'react-tabs';
import './configuracion.scss'
import Form, { Tab, TabbedItem } from 'devextreme-react/form';
//import { Tabs } from '../../../components/Tabs/Tabs';
//import { ArbolInventarioCategoria } from '../../../components/arbol-inventario/arbol-inventario-categoria';

export function ConfiguracionTiendas(props) {
  return (
    <React.Fragment>
      <div className={'content-block'}>
        <div className={'dx-card'}>
          <Form>
            <TabbedItem>
              <Tab title="Configuración de categorías a sincronizar">
                <ConfiguracionCategoriasSync />
              </Tab>
              <Tab title="Configuración del servicio de sincronización">
                <ConfiguracionServicio />
              </Tab>
            </TabbedItem>
          </Form>
        </div>
      </div>
    </React.Fragment>
  );
}


