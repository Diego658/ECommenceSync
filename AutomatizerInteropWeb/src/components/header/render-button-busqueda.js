import React from 'react';
import { Button, Popup } from 'devextreme-react';
import { useState } from 'react';
import BusquedaItems from '../busqueda-items/busqueda-items';

export default function RenderButtonBusquedaItems() {
  const [show, setShow] = useState(false);

  return (
    <>
      <Button
        text="Busqueda Items"
        stylingMode='contained'
        icon='find'
        type='normal'
        onClick={() => setShow(true)} />
      <Popup
        title='Busqueda de items'
        width='90%'
        height='90%'
        visible={show}
        onHiding={() => setShow(false)}
      >
        <div>
          <BusquedaItems />
        </div>
      </Popup>
    </>
  )
}