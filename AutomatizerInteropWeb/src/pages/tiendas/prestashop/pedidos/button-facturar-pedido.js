import React, { useState } from 'react';
import { Button, Popup } from 'devextreme-react';
import FacturarPedidoPrestashop from './facturar-pedido';
import { PrestashopService } from '../../../../_services/stores/prestashop-service';
import notify from 'devextreme/ui/notify';

export default function ButtonFacturarPedidoPrestashop({ id, actualizarEstado }) {
  const [show, setShow] = useState(false);

  const toogleShow = async () => {
    const res = await PrestashopService.tieneSeriesCompletas(id);
    if(res.isOk){
      const tmp = show;
      setShow(!show);
      if(tmp){
        actualizarEstado();
      }
    }
    else{
      notify('Debe ingresar el detale de series completo.', 'error', 2500);
    }
  }

  return (
    <>
      <Button
        id='buttonFacturar'
        onClick={async () => {
          await toogleShow();
          //setfacturarVisible(true);
          //await facturarOrden();
          //setfacturarVisible(false);
        }}
        text='Facturar Orden'
        width={180}
        height={40}
      >

      </Button>
      <Popup
        width={500}
        height={400}
        visible={show}
        title='Facturación'
        onHiding={toogleShow}
      >
        <div>
          {show &&
            <FacturarPedidoPrestashop id={id} />
          }
        </div>
      </Popup>
    </>
  );
}