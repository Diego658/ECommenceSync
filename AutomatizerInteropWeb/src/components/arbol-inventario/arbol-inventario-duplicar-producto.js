import React, { useEffect, useState } from 'react';
import { FieldGroup } from '../fieldGroup/fieldGroup';
import { Field } from '../field/field';
import { CssLoadingIndicator } from '../cssLoader/css-loader';
import { InventarioService } from '../../_services/inventario.service';
import { TextBox, LoadIndicator, Button } from 'devextreme-react';
import { FechHelper } from '../../_helpers/fech-helper';
import notify from 'devextreme/ui/notify';

export default function ArbolInventarioDuplicarProducto({ origenId, onCreated, onCanceled }) {
  const [origen, setOrigen] = useState(null);
  const [loading, setLoading] = useState(true);
  const [codigo, setCodigo] = useState('');
  const [nombre, setNombre] = useState('');
  const [loadIndicatorVisible, setloadIndicatorVisible] = useState(false);


  const cargarDatos = async (id) => {
    const p = await InventarioService.getItemDynamic(id);
    setOrigen(p);
    setLoading(false);
  }


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

      const result = await InventarioService.copyProduct(data.origenId, data);
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

  useEffect(() => {
    cargarDatos(origenId);
  }, [origenId]);


  if (loading) {
    return <CssLoadingIndicator />
  }

  return (
    <>
      <FieldGroup>
        <Field label='Codigo Origen' size='x4' >
          <span >{origen.Codigo}</span>
        </Field>
        <Field label='Referencia Origen' size='x4'>
          <span >{origen.Referencia}</span>
        </Field>
        <Field label='Nombre Origen' size='x4'>
          <span >{origen.Nombre}</span>
        </Field>
      </FieldGroup>
      <FieldGroup>
        <Field label='Referencia' size='s2' >
          <TextBox maxLength={24} text={codigo} onValueChanged={(e) => setCodigo(e.value)} />
        </Field>
        <Field label='Nombre' size='x4'>
          <TextBox width={450} text={nombre} onValueChanged={(e) => setNombre(e.value)} />
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
              onClick={() => crearProducto({ origenId: origenId, referencia: codigo, nombre: nombre })}
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
  );
}



function asyncValidationName(name) {
  return FechHelper.post("Inventario", `Inventario/CheckExistProductByName?name=${name}`);
}

function asyncValidationReferencia(referencia) {
  return FechHelper.post("Inventario", `Inventario/CheckExistProductByReference?reference=${referencia}`);
}