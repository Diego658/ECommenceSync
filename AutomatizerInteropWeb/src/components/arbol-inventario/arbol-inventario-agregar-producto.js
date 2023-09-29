import React from 'react';
import { FieldGroup } from '../fieldGroup/fieldGroup';
import { Field } from '../field/field';
import { TextBox, Button } from 'devextreme-react';
import { useState } from 'react';
import { useEffect } from 'react';
import { InventarioService } from '../../_services/inventario.service';
import { CssLoadingIndicator } from '../cssLoader/css-loader';
import { LoadIndicator } from 'devextreme-react/load-indicator';
import notify from 'devextreme/ui/notify';
import { RequiredRule, AsyncRule, StringLengthRule } from 'devextreme-react/form';
import { FechHelper } from '../../_helpers/fech-helper';


export default function ArbolInventarioAgregarProducto({ padreId, onCreated, onCanceled }) {
  const [padre, setPadre] = useState(null);
  const [loading, setLoading] = useState(true);
  const [codigo, setCodigo] = useState('');
  const [nombre, setNombre] = useState('');
  const [loadIndicatorVisible, setloadIndicatorVisible] = useState(false);

  const crearProducto = async (data) => {
    setloadIndicatorVisible(true);
    try {
      const isValidCode = await asyncValidationReferencia(data.referencia);
      if (!isValidCode) {
        notify("Ya existe un producto con la misma referencia!!!", 'error', 2500);
        setloadIndicatorVisible(false);
        return;
      }
      const isValidName = await asyncValidationName(data.nombre);
      if (!isValidName) {
        notify("Ya existe un oroducto con el nombre " + data.nombre, 'error', 2500);
        setloadIndicatorVisible(false);
        return;
      }

      const result = await InventarioService.addNewProduct(data.padreId, data);
      if (result.isOk) {
        onCreated(result.categoriaNueva)
      } else {
        notify(result.message, 'error', 2500);
        setloadIndicatorVisible(false);
      }

    } catch (error) {
      notify(error, 'error', 2500);
      setloadIndicatorVisible(false);
    }

  }

  const cargarDatos = async (id) => {
    const p = await InventarioService.getItemDynamic(id);
    setPadre(p);
    setLoading(false);
  }

  useEffect(() => {
    cargarDatos(padreId);
  }, [padreId]);


  if (loading) {
    return <CssLoadingIndicator />
  }

  return (
    <>
      <FieldGroup>
        <Field label='Codigo Padre' size='s2' >
          <span >{padre.Codigo}</span>
        </Field>
        <Field label='Padre' size='s2'>
          <span >{padre.Nombre}</span>
        </Field>
        <Field label='Referencia' size='s2' >
          <TextBox maxLength={24} text={codigo} onValueChanged={(e) => setCodigo(e.value)} />
        </Field>
      </FieldGroup>
      <FieldGroup>
        <Field label='Nombre' size='x3'>
          <TextBox text={nombre} onValueChanged={(e) => setNombre(e.value)} />
        </Field>
      </FieldGroup>
      <FieldGroup>
        <div style={{ display: 'flex', margin: 'auto' }}>
          <div style={{ padding: 5, margin: 'auto' }}>
            <Button
              width={180}
              height={40}
              stylingMode='contained'
              type='normal'
              disabled={!codigo || codigo.length < 1 || !nombre || nombre.length < 1}
              onClick={() => crearProducto({ padreId: padreId, referencia: codigo, nombre: nombre })}
            >
              <LoadIndicator className="button-indicator" visible={loadIndicatorVisible} />
              <span className="dx-button-text">{loadIndicatorVisible ? 'Creando producto...' : 'Crear producto'}</span>
            </Button>
          </div>
          <div style={{ padding: 5, margin: 'auto' }}>
            <Button
              width={180}
              height={40}
              text='Cancelar'
              stylingMode='contained'
              type='danger'
              onClick={onCanceled}
            />
          </div>
        </div>
      </FieldGroup>
    </>
  )
}



function asyncValidationName(name) {
  return FechHelper.post("Inventario", `Inventario/CheckExistProductByName?name=${name}`);
}

function asyncValidationReferencia(referencia) {
  return FechHelper.post("Inventario", `Inventario/CheckExistProductByReference?reference=${referencia}`);
}