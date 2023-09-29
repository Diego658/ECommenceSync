import React, { useState, useEffect } from 'react'
import { TextBox, Button, ScrollView } from 'devextreme-react';
import { InventarioService } from '../../_services/inventario.service';
import './arbol-inventario-item.scss'
import Form, { Tab, TabbedItem } from 'devextreme-react/form';
import { ArbolInventarioItemInformacion } from './arbol-inventario-item-infomacion';
import { ArbolInventarioItemPaginaWeb } from './arbol-inventario-item-paginaweb';
import { ArbolInventarioItemImagenes } from './arbol-inventario-item-imagenes';
import notify from 'devextreme/ui/notify';
import { ArbolInventarioItemBodegas } from './arbol-inventario-item-bodega';
import { ArbolInventarioItemCombinaciones } from './arbol-inventario-item-combinaciones';
//import Barcode from 'react-barcode';
import { FieldGroup } from '../fieldGroup/fieldGroup';
import { Field } from '../field/field';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { useCallback } from 'react';
import ArbolInventarioItemPoliticas from './arbol-inventario-item-politicas';


export function ArbolInventarioItem({ itemId, height }) {
  //const [producto, setProducto] = useState(null);
  //const [cols, setCols] = useState(null);
  //const[treeData, setTreeData] = useState(null);
  //const [cambiosPendientes, setcambiosPendientes] = useState(false);
  const [estado, setEstado] = useState({ producto: null });
  const { producto } = estado;
  const updateData = useCallback((newItem, col) => {
    //console.log(newItem);
    //setProducto(newItem);
    //setcambiosPendientes(true);
    //const colsUpdated = {...cols};
    //colsUpdated[col] = newItem[col];
    //setCols(colsUpdated);
    const cols = estado.cols;
    cols[col] = newItem[col];
    setEstado({ producto: newItem, cols: { ...cols }, cambiosPendientes: true });
    console.log('updateData called, col ' + col)

  }, [estado]);


  const updateHtmlEditorData = (col, data) => {
    //console.log('Editor ' +data);
    //console.log('Estado' + estado.producto.Observaciones);
    // if (estado.producto[col] !== data) {
    //   const colsUpdated = estado.cols;
    //   colsUpdated[col] = data;
    //   setEstado({ producto: estado.producto, cols: colsUpdated, cambiosPendientes : true});
    //   //setcambiosPendientes(true);
    //   //setCols({ ...colsUpdated });
    // }

  };

  const onSaveButtonClick = useCallback(() => {
    notify('Guardando...', 'info', 1500);
    InventarioService.saveItem(estado.cols, itemId).then(response => {
      setEstado({ producto: estado.producto, cols: {}, cambiosPendientes: false });
      //setCols({});
      //setcambiosPendientes(false);
      notify('Guardado', 'success', 1500);
    }, error => {
      notify('Error al guardar el item...', 'error', 1500);
    })

  }, [itemId, estado]);

  const onRefreshButtonClick = () => {

  }

  const onDeleteButtonClick = () => {

  }


  const onReferenceUpdated = useCallback((e) => {
    updateData({ ...producto, Referencia: e.value }, 'Referencia')
  },    [producto, updateData])


  useEffect(() => {
    if (itemId == null || itemId === undefined) {
      return;
    }
    InventarioService.getItemDynamic(itemId).then(data => {
      //setProducto(data);
      //setCols({});
      //setcambiosPendientes(false);
      //console.log('Cargado ' + data.Observaciones);
      setEstado({ producto: data, cols: {}, cambiosPendientes: false });
    }, error => {
      notify('Error al cargar el item...' + error, 'error', 2500);
      setEstado({ producto: null, cols: {}, cambiosPendientes: false });
      //setCols({});
      //setcambiosPendientes(false);
    });

  }, [itemId]);

  //console.log(itemId);


  //console.log('Render ' + (producto ?? {}).Observaciones );
  if (itemId === null || producto === null) {
    return (
      <>
        <div className='infoContainer'>
          <FontAwesomeIcon icon='info-circle' className='fa-4x infoicon' />
          <p className='infoText'>
            Seleccione un elemento del árbol izquierdo para visualizar.
          </p>
        </div>
      </>);
  }



  return (
    <div className='arbolInventarioContainer'  >
      <div className='arbolInventarioContent' style={{ height: height }}>
        <FieldGroup>
          <FieldGroup>
            <Field label='Código'>
              <TextBox value={producto.Codigo} />
            </Field>
            <Field label='Referencia'>
              <TextBox value={producto.Referencia} onValueChanged={onReferenceUpdated} />
            </Field>
            <Field label='Máscara'>
              <TextBox value={producto.Mascara} />
            </Field>
          </FieldGroup>

        </FieldGroup>
        <FieldGroup>
          <Field label='Nombre' size=' field-fullwidth'>
            <TextBox value={producto.Nombre} onValueChanged={(value) => updateData({ ...producto, Nombre: value.value }, 'Nombre')} />
          </Field>
        </FieldGroup>
        <Form
        >
          <TabbedItem >
            {producto.HasOptions &&
              <Tab title="Combinaciones" >
                <div className="tabContainer">
                  <ScrollView height={height - 200}>
                    <ArbolInventarioItemCombinaciones updateDataFunction={updateData} itemId={itemId} item={producto} height={height} />
                  </ScrollView>
                </div>
              </Tab>
            }
            <Tab title="Informacion"  >
              <ScrollView height={height - 200}>
                <div className="tabContainer">
                  <ArbolInventarioItemInformacion updateDataFunction={updateData} updateHtmlEditorData={updateHtmlEditorData} itemId={itemId} item={producto} />
                </div>
              </ScrollView>
            </Tab>
            <Tab title="Bodegas">
              <ScrollView height={height - 200}>
                <div className="tabContainer">

                  <ArbolInventarioItemBodegas itemId={itemId} />

                </div>
              </ScrollView>
            </Tab>
            <Tab title="Cuentas Contables">
              <p>Proximamente....</p>
            </Tab>
            <Tab title="Politicas">
              <ScrollView height={height - 200}>
                <div className="tabContainer">
                  <ArbolInventarioItemPoliticas updateDataFunction={updateData} updateHtmlEditorData={updateHtmlEditorData} itemId={itemId} item={producto} />
                </div>
              </ScrollView>
            </Tab>
            <Tab title="Historial">
              <p>Proximamente....</p>
            </Tab>
            <Tab title="Pagina Web">
              <ScrollView height={height - 200}  >
                <div className="tabContainer" >

                  <ArbolInventarioItemPaginaWeb item={producto} updateDataFunction={updateData} height={height} updateHtmlEditorData={updateHtmlEditorData} />

                </div>
              </ScrollView>
            </Tab>
            <Tab title="Imagenes">
              <ScrollView height={height - 200}>
                <div className="tabContainer">
                  <ArbolInventarioItemImagenes itemId={itemId} />
                </div>
              </ScrollView>

            </Tab>
          </TabbedItem>
        </Form>
      </div>
      <div className='button-row arbolInventario-commandContainer'  >
        <div className='button-row-button'>
          <Button text="Guardar" icon="save" type='success' onClick={onSaveButtonClick} disabled={!estado.cambiosPendientes} />
        </div>
        <div className='button-row-button'>
          <Button text="Actualizar" icon="refresh" type='success' onClick={onRefreshButtonClick} disabled={false} />
        </div>
        <div className='button-row-button'>
          <Button text="Eliminar" icon="trash" type='danger' onClick={onDeleteButtonClick} disabled={true} />
        </div>
      </div>
    </div>
  )

}