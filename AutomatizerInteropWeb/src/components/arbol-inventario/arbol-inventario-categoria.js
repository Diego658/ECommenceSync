import './arbol-inventario.scss'
import React, { useState, useEffect, useCallback } from 'react';
import { InventarioService } from '../../_services/inventario.service'
import { LoadIndicator } from 'devextreme-react/load-indicator';
import Form, {
  Tab, TabbedItem
} from 'devextreme-react/form';
import 'devextreme-react/text-area';
import { TextBox } from 'devextreme-react';
import { CustomHtmlEditor } from '../html-editor/html-Editor'
import { Button } from 'devextreme-react';
import notify from 'devextreme/ui/notify'
import { ArbolInventarioCategoriaImagenes } from './arbol-inventario-categoria-imagenes'
import { HtmlEditorWithPopUp } from '../html-editor/html-editor-with-popup';
import { FieldGroup } from '../fieldGroup/fieldGroup';
import { Field } from '../field/field';
import { CustomSunHtmlEditor } from '../sun-editor/customHtmEditor';


export function ArbolInventarioCategoria({ itemId, height }) {
  const [categoria, setCategoria] = useState(null);
  const [cambiosPendientes, setcambiosPendientes] = useState(false);
  const [descripcionWeb, setDescripcionWeb] = useState('');
  const [descripcionCortaWeb, setDescripcionCortaWeb] = useState('');

  const categoriaFunction = useCallback(async (itemID) => {
    try {
      setDescripcionWeb('');
      setDescripcionCortaWeb('');
      setcambiosPendientes(false);
      const data = await InventarioService.getItemDynamic(itemID);
      setCategoria(data);
      setDescripcionWeb(data["Description-Html"])
      setDescripcionCortaWeb(data['Description-Short-Html'])
    } catch (error) {
      notify('Erro al cargar la categoria, error: ' + error, 'error', 2500);
    }
  }, [])

  const onDescripcionPaginaWebChanged = useCallback((content) => {
    setcambiosPendientes(descripcionWeb !== content);
    //setCategoria({ ...categoria, 'Description-Html': content });
    setDescripcionWeb(content);

  }, [ descripcionWeb])

  useEffect(() => {
    categoriaFunction(itemId);
  }, [itemId, categoriaFunction]);


  if (categoria == null) {
    return (
      <React.Fragment>
        <LoadIndicator id="small-indicator" height={20} width={20} />
      </React.Fragment>
    );
  }
  console.log('categoria: ', categoria);
  return (
    <div className='arbolInventarioContainer'  >
      <div className='arbolInventarioContent'>
        <FieldGroup>
          <Field label='Código'>
            <TextBox value={categoria.Codigo} />
          </Field>
          <Field label='Nombre' size='x2'>
            <TextBox value={categoria.Nombre} onValueChanged={(value) => {
              setCategoria({ ...categoria, Nombre: value.value });
              setcambiosPendientes(true);
            }} />
          </Field>
        </FieldGroup>
        <Form>
          <TabbedItem>
            <Tab title="Descripción Pagina Web">
              <CustomSunHtmlEditor showToolbar={true} initialContents={descripcionWeb} updateFunction={onDescripcionPaginaWebChanged} >

              </CustomSunHtmlEditor>
            </Tab>
            <Tab title="Descripción corta Pagina Web">
              <CustomSunHtmlEditor showToolbar={true} initialContents={descripcionCortaWeb} updateFunction={content => {
                if (itemId === categoria.ItemID) {
                  setcambiosPendientes(true);
                  setDescripcionCortaWeb(content);
                  //categoria['Description-Short-Html'] = content;
                }

                //setCategoria({ categoria, 'Description-Short-Html': content });
              }} >
              </CustomSunHtmlEditor>
            </Tab>
            <Tab title="Imagenes Pagina Web">
              <div id="imagenesCategoria">
                <ArbolInventarioCategoriaImagenes itemID={categoria.ItemID} height={height} />
              </div>
            </Tab>
          </TabbedItem>
        </Form>
        <div className='button-row arbolInventario-commandContainer'  >
          <div className='button-row-button'>
            <Button icon='save' text='Guardar' disabled={!cambiosPendientes} onClick={async () => {
              notify("Guardando...", 'info', 1000);
              categoria['Description-Html'] = descripcionWeb;
              categoria['Description-Short-Html'] = descripcionCortaWeb;
              await InventarioService.actualizarCategoria(categoria);
              notify("Guardado");
            }} />
          </div>
        </div>
      </div>
    </div>

  );
}