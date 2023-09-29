import React from 'react';
import { FieldGroup } from '../../../../components/fieldGroup/fieldGroup';
import { Field } from '../../../../components/field/field';
import { TextArea, Button, LoadIndicator } from 'devextreme-react';
import { useState } from 'react';
import { PrestashopService } from '../../../../_services/stores/prestashop-service';
import notify from 'devextreme/ui/notify';

export default function PedidoCancelarPedido({ id }) {
  const [motivo, setMotivo] = useState('');
  const [cancelando, setCancelando] = useState(false);
  const [cancelado, setCancelado] = useState(false);

  const cancelarPedido = async ()=>{
    try {
      setCancelando(true);
      await PrestashopService.cancelarPedido(id, { ordenId : id, motivo: motivo  } )
      notify('Pedido cancelado!!!', 'success', 1500);
      setCancelado(true);
    } catch (error) {
      notify(error, 'error', 3500);
    }
    setCancelando(false);
  }

  return (
    <>
      <FieldGroup>
        <Field label='Motivo' size='x2' height={100} >
          <TextArea disabled={cancelado} value={motivo} onValueChanged={(e) => setMotivo(e.value)} />
        </Field>
      </FieldGroup>
      <div>
        <div>
          <Button
            width={140}
            height={40}
            stylingMode='contained'
            type='danger'
            disabled={!motivo || motivo.length <5 || cancelado}
            onClick={cancelarPedido}
          >
            <LoadIndicator className="button-indicator" visible={cancelando} />
            <span className="dx-button-text">{(cancelando ? 'Cancelando...' : 'Cancelar Pedido')}</span>
          </Button>
        </div>
      </div>
    </>);
}