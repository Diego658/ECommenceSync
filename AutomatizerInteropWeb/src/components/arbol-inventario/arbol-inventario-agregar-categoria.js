import React from 'react';
import { FieldGroup } from '../fieldGroup/fieldGroup';
import { Field } from '../field/field';
import { TextBox, Button, Switch } from 'devextreme-react';
import { useState } from 'react';
import { useEffect } from 'react';
import { InventarioService } from '../../_services/inventario.service';
import { CssLoadingIndicator } from '../cssLoader/css-loader';
import { LoadIndicator } from 'devextreme-react/load-indicator';
import notify from 'devextreme/ui/notify';
import { RequiredRule, AsyncRule, StringLengthRule } from 'devextreme-react/form';
import { FechHelper } from '../../_helpers/fech-helper';

export default function ArbolInventarioAgregarCategoria({ padreId, onCreated, onCanceled }) {
  const [padre, setPadre] = useState(null);
  const [loading, setLoading] = useState(true);
  const [codigo, setCodigo] = useState('');
  const [nombre, setNombre] = useState('');
  const [loadIndicatorVisible, setloadIndicatorVisible] = useState(false);
  const [agregarPrestashop, setAgregarPrestashop] = useState(false);

  const crearCategoria = async (data) => {
    setloadIndicatorVisible(true);
    try {
      const isValidCode = await asyncValidation(data.codigo, data.padreId);
      if (!isValidCode) {
        notify("Ya existe un subcategoria con el código " + data.codigo, 'error', 2500);
        setloadIndicatorVisible(false);
        return;
      }
      const isValidName = await asyncValidationName(data.nombre, data.padreId);
      if (!isValidName) {
        notify("Ya existe un subcategoria con el nombre " + data.nombre, 'error', 2500);
        setloadIndicatorVisible(false);
        return;
      }

      const result = await InventarioService.addNewCategory(data.padreId, data);
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
        <Field label='Padre' size='x1'>
          <span >{padre.Nombre}</span>
        </Field>
        {false &&
          <Field label='Agregar Prestashop' size='s2'>
            <Switch value={agregarPrestashop} onValueChanged={(e) => setAgregarPrestashop(e.value)} />
          </Field>
        }

      </FieldGroup>
      <FieldGroup>
        <Field label='Código' size='s2' >
          <div style={{ display: 'flex' }}>
            <TextBox text={padre.Codigo} width={80} disabled={true} />
            <TextBox readOnly={false} text={codigo} maxLength={3} width={30} onValueChanged={(e) => setCodigo(e.value)} >
              <RequiredRule message="El codigo es necesario" />
              <StringLengthRule min={3} max={3} />
              <AsyncRule
                message="El código ya existe"
                validationCallback={(p) => asyncValidation(p, padreId)} />
            </TextBox>
          </div>
        </Field>
        <Field label='Nombre' size='x15'>
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
              disabled={!codigo || codigo.length < 3 || !nombre || nombre.length < 1}
              onClick={() => crearCategoria({ padreId: padreId, codigo: codigo, nombre: nombre })}
            >
              <LoadIndicator className="button-indicator" visible={loadIndicatorVisible} />
              <span className="dx-button-text">{loadIndicatorVisible ? 'Creando categoría...' : 'Crear categoría'}</span>
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



function asyncValidation(code, padreId) {
  return FechHelper.post("Inventario", `Inventario/CheckExistSubcategoryCode?parentId=${padreId}&code=${code}`);
}

function asyncValidationName(name, padreId) {
  return FechHelper.post("Inventario", `Inventario/CheckExistSubcategoryName?parentId=${padreId}&name=${name}`);
}