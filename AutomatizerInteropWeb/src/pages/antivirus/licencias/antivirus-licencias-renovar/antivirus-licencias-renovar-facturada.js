import React, { useState, useEffect } from 'react'
import { Button, SelectBox, TextArea } from 'devextreme-react'
import { antivirusLicenciasService } from '../../../../_services/antivirusLicencias.service';
import notify from 'devextreme/ui/notify';
import { TransaccionesService } from '../../../../_services/transacciones.service';
import { SpliContainer } from '../../../../components/splitContainer/split-container';
import { FieldGroup } from '../../../../components/fieldGroup/fieldGroup';
import { Field } from '../../../../components/field/field';

export function AntivirusLicenciasRenovarLicenciaFacturada({ idLicencia, clienteCodigo }) {
  const [facturas, setFacturas] = useState([]);
  const [transacciones, settransacciones] = useState([]);
  const [transaccion, setTransaccion] = useState(null);
  const [renovado, setRenovado] = useState(false);
  const [observaciones, setObservaciones] = useState("");

  const onTransaccionChanged = ({ value }) => {
    antivirusLicenciasService.getFacturasParaRenovacion(value.codigo, clienteCodigo)
      .then(response => {
        setFacturas(response);
        setTransaccion(null);
      })
    //setTransaccionCodigo(value.codigo);
  }

  const onRenovarClick = () => {
    antivirusLicenciasService.registrarRenovacion(idLicencia, transaccion.Id, observaciones)
      .then(reponse => {
        notify('Renovación registrada correctamente', 'success', 2000);
        setRenovado(true);
      }, error => { notify('Error al registrar la renovacion, error ' + error, 'error', 1500) })
  }

  useEffect(() => {
    TransaccionesService.getTransacciones(8, 'FA')
      .then(response => {
        settransacciones(response.filter(c => c.devolucion === false));
      }, error => notify('Erro al cargar las transacciones ' + error))
  }, [idLicencia])



  return (
    <>
      <SpliContainer
        leftWidth='40%'
        rigthWidth='60%'
        leftContent={
          <>
            <FieldGroup>
              <Field size="x3" label="Transacción">
                <SelectBox items={transacciones} displayExpr="nombre" onValueChanged={onTransaccionChanged} />
              </Field>
              <Field size="x3" label="Número">
                <SelectBox items={facturas} displayExpr='Numero' value={transaccion} onValueChanged={(value) => setTransaccion(value.value)} />
              </Field>
            </FieldGroup>
          </>}
        rigthContent={
          <>
            <Field label="Observaciones" >
              <TextArea wordWrapEnabled={true} height={80} value={observaciones} onValueChanged={({value}) => setObservaciones(value)}></TextArea>
            </Field>
          </>}
      >

      </SpliContainer>
      <FieldGroup>
        <Field>
          <Button icon='cart' text='Renovar' type='normal' disabled={transaccion === null || renovado} onClick={onRenovarClick} />
        </Field>
      </FieldGroup>
    </>
  );
}