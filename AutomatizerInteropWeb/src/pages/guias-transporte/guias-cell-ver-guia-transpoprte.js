import React, { useState } from 'react';
import { Button, Popup } from 'devextreme-react';
import GuiasGuiaTransporte from './guias-guiatransporte';


export default function GuiasCellVerGuiaTransporte({ id }) {
  const [show, setShow] = useState(false);

  const toogleShow = ()=>{
    setShow(!show);
  }
  return (
    <>
      <Button stylingMode='text' icon='search' onClick={toogleShow} />
      <Popup
        title='Ver Guía'
        showTitle={true}
        visible={show}
        onHiding={toogleShow}
        closeOnOutsideClick={true}
        width='950px'
        height='500px'
      >
        <div id={'verGuia' + id}>
          {show &&
            <GuiasGuiaTransporte id={id} />
          }
        </div>
      </Popup>
    </>
  );
}