import React from 'react';
import Toolbar, { Item } from 'devextreme-react/toolbar';
import Button from 'devextreme-react/button';
import UserPanel from '../user-panel/user-panel';
import ConfiguracionPanel from '../configuracion-panel/configuracion-panel';
import './header.scss';
import { Template } from 'devextreme-react/core/template';
import RenderButtonBusquedaItems from './render-button-busqueda';


export default ({ menuToggleEnabled, title, toggleMenu, userMenuItems, configuraciones, configuracion, onConfiguracionChanged }) => (
  <header className={'header-component'}>
    <Toolbar className={'header-toolbar'}>
      <Item
        visible={menuToggleEnabled}
        location={'before'}
        widget={'dxButton'}
        cssClass={'menu-button'}
      >
        <Button icon="menu" stylingMode="text" onClick={toggleMenu} />
      </Item>
      <Item
        location={'before'}
        cssClass={'header-title'}
        text={title}
        visible={!!title}
      />

      <Item
        location={'after'}
        locateInMenu={'auto'}
      >
        <RenderButtonBusquedaItems />
      </Item>

      <Item
        location={'after'}
        locateInMenu={'auto'}
        menuItemTemplate={'configuracionesPanelTemplate'}
      >
        <Button

          width={200}
          height={'100%'}
          stylingMode={'text'}
        >
          <ConfiguracionPanel configuraciones={configuraciones} configuracion={configuracion} onConfiguracionChanged={onConfiguracionChanged} />
        </Button>
      </Item>


      <Item
        location={'after'}
        locateInMenu={'auto'}
        menuItemTemplate={'userPanelTemplate'}
      >
        <Button
          className={'user-button authorization'}
          width={200}
          height={'100%'}
          stylingMode={'text'}
        >
          <UserPanel menuItems={userMenuItems} menuMode={'context'} />
        </Button>
      </Item>

      <Template name={'userPanelTemplate'}>
        <UserPanel menuItems={userMenuItems} menuMode={'list'} />
      </Template>
      <Template name={'configuracionesPanelTemplate'}>
        <ConfiguracionPanel configuraciones={configuraciones} />
      </Template>
      <Template name={'buttonBusquedaTemplate'}>
        <RenderButtonBusquedaItems />
      </Template>
    </Toolbar>
  </header>
);

