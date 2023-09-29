import React from 'react'
import { SelectBox, LoadIndicator, NumberBox, TagBox, Button, TextBox } from 'devextreme-react'

import { useState } from 'react';
import { useEffect } from 'react';
import { InventarioService } from '../../_services/inventario.service';


import { Field } from '../field/field';
import { FieldGroup } from '../fieldGroup/fieldGroup';

import { CustomHtmlEditor } from '../html-editor/html-Editor';
import { PrestashopService } from '../../_services/stores/prestashop-service';




export function ArbolInventarioItemInformacion({ item, updateDataFunction, updateHtmlEditorData }) {
  const [unidades, setUnidades] = useState(null);
  const [marcas, setMarcas] = useState(null);
  const [colores, setColores] = useState(null);
  const [tallas, setTallas] = useState(null);
  const [procedencias, setProcedencias] = useState(null);
  const [origenes, setOrigenes] = useState(null);
  const [loading, setLoading] = useState(true);
  const [tags, setTags] = useState(null);

  // const update =(field, data)=>{
  //     updateDataFunction({...item, [field]:data.value })
  // }


  useEffect(() => {
    const cargarDatosSelects = async () => {
      // if (unidades != null) {
      //   return;
      // }
      //console.log(props);
      const u = await InventarioService.getUnidades();
      const m = await InventarioService.getMarcas();
      const c = await InventarioService.getColores();
      const t = await InventarioService.getTallas();
      const p = await InventarioService.getProcedencias();
      const o = await InventarioService.getOrigenes();
      //const l = await InventarioService.Ge
      //console.log(u);
      setUnidades(u);
      setMarcas(m);
      setColores(c);
      setTallas(t);
      setProcedencias(p);
      setOrigenes(o);
      setLoading(false);
    };
    cargarDatosSelects();
  }, [])

  useEffect(() => {
    console.log('setTags (Item changed)')
    if (item == null || item === undefined) {
      setTags([]);
    }
    else {
      setTags(item.Tags.split(','));
      
    }

  }, [item])



  if (loading) {
    return (
      <React.Fragment>
        <LoadIndicator id="small-indicator" height={20} width={20} />
      </React.Fragment>
    );
  }

  return (
    <div>
      <div className='content-row'>
        <FieldGroup>
          <Field label='Unidad' size='s2'>
            <SelectBox
              displayExpr="nombre" valueExpr="codigo" searchEnabled={true} searchMode={'contains'} items={unidades} value={item.UnidadID} onValueChanged={(value) => updateDataFunction({ ...item, UnidadID: value.value }, 'UnidadID')} />
          </Field>

          <Field label='Marca' size='s2'>
            <SelectBox displayExpr="nombre" valueExpr="codigo" searchEnabled={true} searchMode={'contains'} items={marcas} value={item.MarcaID} onValueChanged={(value) => updateDataFunction({ ...item, MarcaID: value.value }, 'MarcaID')} />
          </Field>

          <Field label='Talla' size='s2'>
            <SelectBox displayExpr="nombre" valueExpr="codigo" searchEnabled={true} searchMode={'contains'} items={tallas} value={item.IdTalla} onValueChanged={(value) => updateDataFunction({ ...item, IdTalla: value.value }, 'IdTalla')} />
          </Field>

          <Field label='Procedencia' size='s2'>
            <SelectBox displayExpr="nombre" valueExpr="codigo" searchEnabled={true} searchMode={'contains'} items={procedencias} value={item.ProvCod} onValueChanged={(value) => updateDataFunction({ ...item, ProvCod: value.value }, 'ProvCod')} />
          </Field>

          <Field label='Origen Repuesto' size='s2'>
            <SelectBox displayExpr="nombre" valueExpr="codigo" searchEnabled={true} searchMode={'contains'} items={origenes} value={item.Original} onValueChanged={(value) => updateDataFunction({ ...item, Original: value.value }, 'Original')} />
          </Field>

          <Field label='Color' size='s2'>
            <SelectBox displayExpr="nombre" valueExpr="codigo" searchEnabled={true} searchMode={'contains'} items={colores} value={item.IdColor} onValueChanged={(value) => updateDataFunction({ ...item, IdColor: value.value }, 'IdColor')} />
          </Field>

          {/* <Field label='Línea Prod.' size='s2'>
            <SelectBox displayExpr="nombre" valueExpr="codigo" searchEnabled={true} searchMode={'contains'} items={lineasProducion} value={item.IdColor} onValueChanged={(value) => updateDataFunction({ ...item, IdColor: value.value }, 'IdColor')} />
          </Field> */}

        </FieldGroup>
        <FieldGroup>
          <Field label='Modelo'>
            <TextBox items={origenes} value={item.Modelo} onValueChanged={(value) => updateDataFunction({ ...item, Modelo: value.value }, 'Modelo')} />
          </Field>

          <Field label='# Parte'>
            <TextBox value={item.NroParte} onValueChanged={(value) => updateDataFunction({ ...item, NroParte: value.value }, 'NroParte')} />
          </Field>

          <Field label='Codigos proveedor'>
            <TextBox value={item.CodigosProveedor} onValueChanged={(value) => updateDataFunction({ ...item, CodigosProveedor: value.value }, 'CodigosProveedor')} />
          </Field>
          <Field label='Peso' size='s1'>
            <NumberBox min={0} max={100} value={item.Peso} onValueChanged={(value) => updateDataFunction({ ...item, Peso: value.value }, 'Peso')} />
          </Field>

          <Field label='Contiene' size='s1'>
            <NumberBox showSpinButtons={true} min={0} max={1000} value={item.Contiene} onValueChanged={(value) => { updateDataFunction({ ...item, Contiene: value.value }, 'Contiene') }} />
          </Field>
        </FieldGroup>
        <FieldGroup>
          <Field label='Etiquetas' size='x4' height={90}>
            <div>
              <TagBox acceptCustomValue={true} searchEnabled={false} value={tags} onValueChanged={(value) => {
                //setTags(value.value);
                updateDataFunction({ ...item, Tags: value.value.join(',') }, 'Tags')
              }} />
              <Button
                text='Generar Etiquetas'
                onClick={async (e) => {
                  var data = item.Nombre;
                  var tags = await PrestashopService.getTagsForText(data);
                  updateDataFunction({ ...item, Tags: tags.map(t => t.value).join(',') }, 'Tags');
                }}
              >

              </Button>
            </div>

          </Field>
        </FieldGroup>
      </div>
      <div className='content-row'>
        <CustomHtmlEditor markup={item.Observaciones} height='100%' useToolBar={true} updateMarkupFunction={(value) => updateDataFunction({ ...item, Observaciones: value }, 'Observaciones')} >

        </CustomHtmlEditor>
      </div>
      {/* <div className='htmlContent'>
        <div dangerouslySetInnerHTML={{ __html: item.Observaciones }} />
      </div> */}


    </div>
  );
}