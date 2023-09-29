import React, { useState } from 'react'
import { FieldGroup } from '../../../../components/fieldGroup/fieldGroup'
import { Field } from '../../../../components/field/field';
import { SelectBox, TextArea, Button, LoadIndicator } from 'devextreme-react';
import { useEffect } from 'react';
import { TransaccionesService } from '../../../../_services/transacciones.service';
import { PrestashopService } from '../../../../_services/stores/prestashop-service';
import notify from 'devextreme/ui/notify';

export default function FacturarPedidoPrestashop({ id, onFacturado }) {
  const [transacciones, setTransacciones] = useState([]);
  const [facturandoVisible, setfacturandoVisible] = useState(false);
  const [observaciones, setObservaciones] = useState('');
  const [transaccion, setTransaccion] = useState(null);
  const [cliente, setCliente] = useState(null);
  const [facturado, setFacturado] = useState(false);
  const [existeCliente, setExisteCliente] = useState(false);
  const [clienteAutomatizer, setClienteAutomatizer] = useState(null);


  const cargarDatos = async (ordenId) => {
    const tmp = await TransaccionesService.getTransacciones(8, "FA");
    const orden = await PrestashopService.getOrden(ordenId);
    const invoiceAddres = await PrestashopService.getAddres(orden.id_address_invoice);
    const ca = await PrestashopService.getAutomatizerCustomer(invoiceAddres.dni);
    setClienteAutomatizer(ca);
    setExisteCliente(ca !== "");
    //setTransaccion(tmp[0].codigo);
    setTransacciones(tmp);
    setObservaciones(`Ref. ${orden.reference}`);
    setCliente(invoiceAddres);
  }

  const crearClienteNuevo = async () => {
    try {
      await PrestashopService.createCustomerFromAddress(cliente.Id);
      const ca = await PrestashopService.getAutomatizerCustomer(cliente.dni);
      setClienteAutomatizer(ca);
      setExisteCliente(ca !== "");
      notify('Cliente creado...', 'success', '1500');
    } catch (error) {
      notify('Error ' + error, 'error', '2500');
    }
  }

  const facturarOrden = async () => {
    try {
      let infoFactura = {};
      infoFactura.ordenId = id;
      infoFactura.clienteId = clienteAutomatizer.Clisec;
      infoFactura.transaccionCodigo = transaccion;
      infoFactura.observaciones = observaciones;

      var res = await PrestashopService.facturarOrden(id, infoFactura);
      if (res.isOk) {
        setFacturado(true);
        notify('Orden facturada correctamente', 'success', 2500);
      } else {
        notify(res.message, 'error', 3500);
      }
    } catch (error) {
      notify(error, 'error', 3500);
    }
  }

  useEffect(() => {
    cargarDatos(id);
  }, [id]);

  return (
    <div className='facturarPedido'>
      <div className='datosFacturacion'>
        <FieldGroup>
          <Field label='Transacción' size='x2'>
            <SelectBox items={transacciones}
              placeholder="Seleccione la Transacción"
              displayExpr='nombre'
              valueExpr='codigo'
              selectedItem={transaccion}
              onValueChanged={({ value }) => setTransaccion(value)}
              showClearButton={true}
              disabled={facturado} />
          </Field>
        </FieldGroup>
        <FieldGroup>
          <Field label='Cliente' size='x2'>
            {cliente &&
              <>
                <span>{`${cliente.dni} - ${cliente.firstname} ${cliente.lastname}`}</span>
                {!existeCliente &&
                  <Button
                    text='Crear'
                    style={{ 'marginLeft': '10px' }}
                    onClick={() => {
                      crearClienteNuevo();
                    }} />
                }
              </>
            }
          </Field>
        </FieldGroup>
        <FieldGroup>
          <Field label='Observaciones' size='x2'>
            <TextArea height={80} disabled={facturado} value={observaciones} onValueChanged={(e) => {
              setObservaciones(e.value);
            }} />
          </Field>
        </FieldGroup>
      </div>
      <div className='botonesFacturar'>
        <Button
          disabled={facturado || !existeCliente || !transaccion}
          id='buttonFacturar'
          onClick={async () => {
            setfacturandoVisible(true);
            await facturarOrden();
            setfacturandoVisible(false);
          }}
          width={180}
          height={40}
        >
          <LoadIndicator className="button-indicator" visible={facturandoVisible} />
          <span className="dx-button-text">{(facturandoVisible ? 'Facturando...' : 'Facturar')}</span>
        </Button>
      </div>
    </div>
  )
}