import React, { useMemo } from 'react';
import { FieldGroup } from '../fieldGroup/fieldGroup';
import { Field } from '../field/field';
import { TextBox, CheckBox, SelectBox, DataGrid, ScrollView, Popup, Button, ContextMenu } from 'devextreme-react';
import { useState } from 'react';
import { useEffect } from 'react';
import { InventarioService } from '../../_services/inventario.service';
import { Column, StateStoring, Paging, Pager, Export } from 'devextreme-react/data-grid';
import { authHeader } from '../../_helpers';
import { config } from '../../constants/constants';
import UserSesion from '../../utils/userSesion';
import AspNetData from 'devextreme-aspnet-data-nojquery';
import { ArbolInventarioItem } from '../arbol-inventario/arbol-inventario-item';
import notify from 'devextreme/ui/notify';
import BusquedaItemsNombreItemRender from './busqueda-items-imagenes-popover';

export default function BusquedaItems() {
  const [precionConIva, setprecionConIva] = useState(false);
  const [soloConSaldo, setSoloConSaldo] = useState(true);
  const [todasProveniencias, setTodasProveniencias] = useState(true);
  const [proveniencias, setProveniencias] = useState([]);
  const [proveniencia, setProveniencia] = useState(0);
  const [textoBusqueda, setTextoBusqueda] = useState('');
  const [busqueda, setBusqueda] = useState({});
  const [item, setItem] = useState(null);

  const cargarDatos = async () => {
    const p = await InventarioService.getProcedencias();
    setProveniencias(p);
  }

  const auth = useMemo(() => authHeader(), []);

  const itemsData = useMemo(() => {
    return AspNetData.createStore({
      key: 'itemId',
      loadUrl: `${config.url.API_URL}/api/ProductsSearch/SearchItemsWithFulltext`,
      loadMode: 'processed',
      cacheRawData: false,
      onBeforeSend: function (method, ajaxOptions) {
        method = "POST";
        ajaxOptions.xhrFields = { withCredentials: false };
        ajaxOptions.headers = {
          'Content-Type': 'application/json',
          'Authorization': auth.Authorization,
          'ConfiguracionId': UserSesion.getConfiguracion().idConfiguracionPrograma
        };
        ajaxOptions.data = busqueda;
      }
    });
  }, [auth.Authorization, busqueda]);

  useEffect(() => {
    cargarDatos();
  }, [])

  const linkWebRender = ((data) => {
    if (data.data.linkWeb === "") {
      return <></>
    };

    return (
      <>
        <a href={data.data.linkWeb} target="_blank" rel="noopener noreferrer"   >
          <Button
            stylingMode='text'
            icon='link'
          />
        </a>
      </>
    );

  });


  

  const nombreItemRender = ((data) => {
    return (
      <>
        <BusquedaItemsNombreItemRender item={data.data} precioConIva={precionConIva} />
      </>
    )
  });


  return (
    <>
      <FieldGroup>
        <Field size='x15'>
          <TextBox
            value={textoBusqueda}
            onValueChanged={(e) => setTextoBusqueda(e.value)} valueChangeEvent='keyup'
            onKeyUp={(e) => {
              const charCode = (e.event.charCode) ? e.event.charCode : ((e.event.keyCode) ? e.event.keyCode :
                ((e.event.which) ? e.event.which : 0));
              if (charCode === 13) {
                const searchData = {
                  top: 50,
                  precioConIva: precionConIva,
                  proveniencias: (todasProveniencias ? proveniencias.map(p => p.codigo).join(',') : proveniencia),
                  soloConStock: soloConSaldo,
                  searchQuery: textoBusqueda
                };
                setBusqueda(searchData);
              }
            }} />
        </Field>
        <Field>
          <CheckBox
            text='Precio con Iva'
            value={precionConIva}
            onValueChanged={(e) => {
              setprecionConIva(e.value);
              const searchData = {
                top: 50,
                precioConIva: e.value,
                proveniencias: (todasProveniencias ? proveniencias.map(p => p.codigo).join(',') : proveniencia),
                soloConStock: soloConSaldo,
                searchQuery: textoBusqueda
              };
              setBusqueda(searchData);
            }}
          />
        </Field>
        <Field>
          <CheckBox
            text='Con Saldo'
            value={soloConSaldo}
            onValueChanged={(e) => {
              setSoloConSaldo(e.value);
              const searchData = {
                top: 50,
                precioConIva: precionConIva,
                proveniencias: (todasProveniencias ? proveniencias.map(p => p.codigo).join(',') : proveniencia),
                soloConStock: e.value,
                searchQuery: textoBusqueda
              };
              setBusqueda(searchData);
            }} />
        </Field>
        <Field>
          <CheckBox
            text='Cualquier proveniencia'
            value={todasProveniencias}
            onValueChanged={(e) => setTodasProveniencias(e.value)} />
        </Field>
        {!todasProveniencias &&
          <Field>
            <SelectBox
              items={proveniencias}
              displayExpr='nombre'
              valueExpr='codigo'
              onValueChanged={(e) => setProveniencia(e.value)} />
          </Field>
        }
      </FieldGroup>

      <DataGrid
        showBorders={true}
        allowColumnReordering={true}
        allowColumnResizing={true}
        showRowLines={true}
        dataSource={itemsData}
        focusedRowEnabled={true}
        hoverStateEnabled={true}
        onRowDblClick={(e) => {
          setItem(e.data);
        }}
        height={window.innerHeight - 300}
      >
        <Export allowExportSelectedData={true} fileName='busquedaItems' />
        <Paging enabled={false} />
        <StateStoring enabled={true} type="localStorage" storageKey="gridBusquedaitems" />
        <Pager showInfo={true} visible={true} />
        <Column dataField='codigo' />
        <Column dataField='referencia' />
        <Column dataField='nombre' cellRender={nombreItemRender} />
        <Column dataField='marca' />
        <Column dataField='modelo' />
        <Column dataField='nroParte' />
        <Column dataField='codigosProveedor' />
        <Column dataField='tipoParte' />
        <Column dataField='b1' caption='B1' />
        <Column dataField='eventual' />
        <Column dataField='mayorista' />
        <Column dataField='empPublica' />
        <Column dataField='linkWeb' cellRender={linkWebRender} />
      </DataGrid>

      <Popup
        title='Detalle Item'
        width='80%'
        height='80%'
        visible={item}
        closeOnOutsideClick={true}
        showCloseButton={true}
        onHiding={() => setItem(null)}
      >
        <div>
          {item &&
            <ArbolInventarioItem itemId={item.itemId} height={window.innerHeight - 300} />
          }
        </div>
      </Popup>
    </>
  )
}

