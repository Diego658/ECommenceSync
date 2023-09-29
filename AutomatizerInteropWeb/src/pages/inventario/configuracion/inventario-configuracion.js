import React, { useState } from 'react'
import { Form } from "devextreme-react";
import { TabbedItem, Tab } from 'devextreme-react/form';
import { InventarioConfiguracionMarcas } from './marcas/inventario-configuracion-marcas';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { InventarioConfiguracionAtributos } from './atributos/inventario-configuracion-atributos';
import { ArbolInventarioItem } from '../../../components/arbol-inventario/arbol-inventario-item';
import { config } from '../../../constants/constants';

const customTabRender = ({ title, icon }) => {
  return (

    <div className="customTabHeader">
      <FontAwesomeIcon icon={icon} className="tabHeaderIcon" />
      <span className="tabHeaderText">{title}</span>
    </div>

  )
}

export function InventarioConfiguracion() {
  const [gridHeight] = useState(window.innerHeight - 200);

  return (
    <React.Fragment>
      <div className={'content-block'} style={{ height: gridHeight }} >
        <div className={'dx-card'}>
          <Form>
            <TabbedItem>
              {process.env.NODE_ENV === 'development' &&
                <Tab title="Desarrollo" tabRender={customTabRender} icon={['fas', 'bookmark']}>
                  <ArbolInventarioItem itemId={1548} height={gridHeight} />
                </Tab>
              }

              <Tab title="Marcas" tabRender={customTabRender} icon={['fas', 'bookmark']}>
                <InventarioConfiguracionMarcas gridHeight={gridHeight} />
              </Tab>
              <Tab title="Bodegas" tabRender={customTabRender} icon={['fas', 'warehouse']}  >
                <p>Proximamente....</p>
              </Tab>
              <Tab title="Atributos" tabRender={customTabRender} gridHeight={gridHeight} icon={['fas', 'grip-horizontal']}  >
                <InventarioConfiguracionAtributos gridHeight={gridHeight} />
              </Tab>
            </TabbedItem>
          </Form>
        </div>
      </div>
    </React.Fragment>
  );
}