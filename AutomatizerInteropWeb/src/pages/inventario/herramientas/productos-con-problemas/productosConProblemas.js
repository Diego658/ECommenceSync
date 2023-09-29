import React, { useMemo } from 'react';
import { FieldGroup } from '../../../../components/fieldGroup/fieldGroup';
import { Field } from '../../../../components/field/field';
import { CheckBox, Button, SelectBox, ScrollView, Popup } from 'devextreme-react';
import FilterBuilder, { } from 'devextreme-react/filter-builder';
import { useState } from 'react';
import DataGrid, { SearchPanel, ColumnChooser, Export, StateStoring, Pager, Column } from 'devextreme-react/data-grid';
import { useEffect } from 'react';
import ls from 'local-storage';
import DataSource from 'devextreme/data/data_source';
import AspNetData from 'devextreme-aspnet-data-nojquery';
import { config } from '../../../../constants/constants';
import { authHeader } from '../../../../_helpers';
import UserSesion from '../../../../utils/userSesion';
import { sizes, subscribe, unsubscribe } from '../../../../utils/media-query';
import './productosConProblemas.scss'
import { ArbolInventarioItem } from '../../../../components/arbol-inventario/arbol-inventario-item';


function renderCellIdProduct(data){
  return(
    <ProductRender id={data.data.itemId} />
  )
}

function ProductRender({id}) {
  const [show, setShow] = useState(false);
  return (
    <div id={'verProducto' + id}>
      <span  
      style={{ cursor: 'pointer' }} onClick={()=> setShow(true)} >{id}</span>
      <Popup
        width='80%'
        height='90%'
        visible={show}
        title='Ficha del producto'
        onHiding={()=> setShow(false)}
      >
        <div>
          {show &&
            <ArbolInventarioItem itemId={id} height={window.innerHeight -200} />
          }
        </div>
      </Popup>
    </div>

  )
}


const fields = [
  {
    dataField: 'codigo',
    width: 100,
    dataType: 'string'
  }, {
    dataField: 'referencia',
    width: 100,
    dataType: 'string'
  }, {
    dataField: 'nombre',
    width: 300,
    dataType: 'string'
  }, {
    caption: 'Peso',
    dataField: 'peso',
    dataType: 'number',
    width: 80
  }, {
    dataField: 'marca',
    width: 100,
    dataType: 'string'
  }, {
    dataField: 'bodegas',
    width: 100,
    dataType: 'string'
  }, {
    dataField: 'saldo',
    caption: 'Saldo',
    dataType: 'number',
    width: 80
  }, {
    dataField: 'numeroImagenes',
    caption: '# Imagenes',
    dataType: 'number',
    width: 80
  }, {
    dataField: 'preciosEstablecidos',
    width: 100,
    dataType: 'string'
  }, {
    dataField: 'padre',
    width: 100,
    dataType: 'string'
  }, {
    dataField: 'prestashop',
    width: 100,
    dataType: 'boolean'
  }, {
    dataField: 'fechaCreacion',
    width: 100,
    dataType: 'date'
  },
  {
    dataField: 'esNuevo',
    caption: 'Nuevo',
    width: 100,
    dataType: 'boolean'
  }

];

const defaultFilter = [
  ['saldo', '>', 0],
  'and',
  [
    ['EsNuevo', '=', 'true']
  ]
];

const filtroProductosNuevos = [
  ['esNuevo', '=', true]
];

const filtroProductosSinPeso = [
  ['peso', '=', 0],
  'and',
  [
    ['saldo', '>', 0]
  ]
];

const filtrosPredefinidos = [
  {
    name: 'Productos Nuevos',
    filter: filtroProductosNuevos
  },
  {
    name: 'Productos Sin Peso',
    filter: filtroProductosSinPeso
  }
];


export default function ProductosConProblemas() {
  const [filtro, setFiltro] = useState('');
  const [gridFilterValue, setGridFilterValue] = useState('');
  const auth = useMemo(() => authHeader(), []);

  const getScreenSizeClass = () => {
    const screenSizes = sizes();
    return Object.keys(screenSizes).filter(cl => screenSizes[cl]).join(' ');
  }

  const dataSource = new DataSource({
    store: AspNetData.createStore({
      key: 'itemId',
      loadUrl: `${config.url.API_URL}/api/ProductsSearch/ProductosConProblemas`,
      loadMode: 'processed',
      cacheRawData: false,
      onBeforeSend: function (method, ajaxOptions) {
        method = "GET";
        ajaxOptions.xhrFields = { withCredentials: false };
        ajaxOptions.headers = {
          'Content-Type': 'application/json',
          'Authorization': auth.Authorization,
          'ConfiguracionId': UserSesion.getConfiguracion().idConfiguracionPrograma
        };
      }
    })
  });


  useEffect(() => {
    const f = ls.get('filtroProductosConProblemas') || filtroProductosNuevos;
    setFiltro(f);
    setGridFilterValue(f);
  }, [])


  return (
    <div style={{ margin: '10px' }} className='dx-card' >
      <FieldGroup>
        <Field size='x3' height={120}>
          <div className='filter-container'>
            <div>
              <ScrollView
                height={120}
              >
                <FilterBuilder fields={fields}

                  width={600}
                  value={filtro}
                  defaultValue={filtroProductosNuevos}
                  onValueChanged={(e) => {
                    setFiltro(e.value);
                    if(e.value){
                      ls.set('filtroProductosConProblemas', e.value);
                    }
                  }} />
              </ScrollView>

            </div>
            <div style={{ marginLeft: '20px' }}>
              <FieldGroup>
                <Field label='Filtros Predefinidos'>
                  <SelectBox
                    items={filtrosPredefinidos}
                    displayExpr='name'
                    onValueChanged={(e) => {
                      if (e.value) {
                        setFiltro(e.value.filter);
                        setGridFilterValue(e.value.filter);
                      }
                    }}
                  />
                </Field>
              </FieldGroup>
              <FieldGroup>
                <Field>
                  <Button
                    text="Aplicar filtro"
                    type="default"
                    onClick={() => { setGridFilterValue(filtro) }} />
                </Field>
              </FieldGroup>


            </div>
          </div>
        </Field>
      </FieldGroup>
      <FieldGroup>
        <Field size='x4' height={window.innerHeight - 100}>
          <DataGrid
            dataSource={dataSource}
            filterValue={gridFilterValue}
            showBorders={true}
            height={window.innerHeight - 200}
            width={window.innerWidth - (getScreenSizeClass().includes('large') ? 300 : 100)}
            showRowLines={true}
            hoverStateEnabled={true}
            focusedRowEnabled={true}
          >
            <ColumnChooser enabled={true} />
            <SearchPanel visible={true} />
            <Pager showInfo={true} />
            <Export enabled={true}
              fileName={`Prodcutos con problemas`}
              allowExportSelectedData={true} />
            <StateStoring enabled={true} type="localStorage" storageKey="productos-con-problemas-grid" />
            <Column
              caption='ID'
              width={50}
              dataField='itemId'
              dataType='number'
              cellRender={renderCellIdProduct}
            >
            </Column>
            {fields.map(f => <Column key={'column' + f.dataField} dataField={f.dataField} width={f.width} dataType={f.dataType} caption={f.caption} />)

            }
          </DataGrid>
        </Field>

      </FieldGroup>
    </div>
  )
}