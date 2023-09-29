import React, { useState, useMemo } from 'react';
import { ButtonGroup } from 'devextreme-react';
import { useEffect } from 'react';
import './pedidos.scss'
import { PrestashopService } from '../../../../_services/stores/prestashop-service';
import notify from 'devextreme/ui/notify';
import DataGrid, { Column, FilterRow, StateStoring, Export, ColumnChooser, ColumnFixing, Pager, Paging, SearchPanel, HeaderFilter, RemoteOperations } from 'devextreme-react/data-grid';
import DataSource from 'devextreme/data/data_source';
import { useCallback } from 'react';
import CellVerPedidoPrestashop from './cell-ver-pedido';
import AspNetData from 'devextreme-aspnet-data-nojquery';
import { config } from '../../../../constants/constants';
import { authHeader } from '../../../../_helpers';
import UserSesion from '../../../../utils/userSesion';
const EstadoSeleccionadosKey = 'pedidos-estados-seleccionados';

export default function PedidosPrestashop() {
  //const [loading, setLoading] = useState(true);
  //const [loadingOrders, setLoadingOrders] = useState(false);
  // const [estados, setEstados] = useState(["Pago aceptado", "Enviado", "Entregado", "Cancelado", "En espera de pago por transferencia bancaria"]);
  // const [estadosSeleccionados, setEstadosSeleccionados] = useState([]);
  // const [ordenes, setOrdenes] = useState([]);


  // const cargarEstados = async () => {
  //   const e = await PrestashopService.getEstadosOrdenes(2020);
  //   setEstados(e);
  // }

  // const cargarOrdenes = useCallback(async () => {
  //   try {
  //     //setLoadingOrders(true);
  //     const o = await PrestashopService
  //       .getOrdenes(2020, estadosSeleccionados.join(','));
  //     setOrdenes(o);
  //   } catch (error) {
  //     notify(error, 'error', 2500);
  //   }
  //   //setLoadingOrders(false);
  // }, [estadosSeleccionados]);

  // useEffect(() => {
  //   // cargarEstados().then(() => {
  //   //   const tmp = localStorage.getItem(EstadoSeleccionadosKey)
  //   //   console.log(tmp);
  //   //   if (tmp) {
  //   //     const estadosTmp = JSON.parse(tmp);
  //   //     setEstadosSeleccionados(estadosTmp);
  //   //     PrestashopService
  //   //       .getOrdenes(2020, estadosTmp.join(','))
  //   //       .then(o => {
  //   //         setOrdenes(o);
  //   //       }, error => {
  //   //         setOrdenes([]);
  //   //         notify(error, 'error', 2500);
  //   //       });

  //   //   }
  //   // });
  // }, []);


  // const onToolbarPreparing = (e) => {
  //   e.toolbarOptions.items.unshift({
  //     location: 'before',
  //     widget: 'dxButton',
  //     options: {
  //       width: 136,
  //       text: 'Recargar',
  //       icon: 'refresh',
  //       onClick: () => cargarOrdenes()
  //     }
  //   }
  //   );
  // }

  // const onSelectedStatusChanged = async (e) => {
  //   const tmp = estadosSeleccionados;
  //   const { addedItems, removedItems } = e;
  //   for (let index = 0; index < addedItems.length; index++) {
  //     const element = addedItems[index];
  //     tmp.push(element.id);
  //   }

  //   for (let index = 0; index < removedItems.length; index++) {
  //     const element = removedItems[index];
  //     const pos = tmp.indexOf(element.id);
  //     tmp.splice(pos, 1);
  //   }

  //   localStorage.setItem(EstadoSeleccionadosKey, JSON.stringify(tmp));
  //   await cargarOrdenes();

  // }

  const auth = useMemo(() => authHeader(), []);

  const ordersData = useMemo(() => {
    return AspNetData.createStore({
      key: 'id',
      loadUrl: `${config.url.API_URL}/api/ProductsSearch/PrestashopOrders`,
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
    });
  }, [auth]);

  const verOrdenCellRender = ((data) => {
    return (
      <>
        <CellVerPedidoPrestashop idPedido={data.data.id} estado={data.data.current_state} />
      </>
    )
  });

  const estadoCellRender = ((data) => {
    return (
      <div style={{ backgroundColor: data.data.color }} className='estadoPedido' ><span>{data.data.status}</span></div>
    )
  });


  return (
    <>
      <div className='content-block'>
        <div className='dx-card'>

          <DataGrid
            dataSource={ordersData}
            wordWrapEnabled={true}
            focusedRowEnabled={true}
            allowColumnReordering={true}
            allowColumnResizing={true}
            columnAutoWidth={false}
            showBorders={true}
          >
            <RemoteOperations paging={true} filtering={true} sorting={true} grouping={true} />
            <StateStoring enabled={true} type="localStorage" storageKey="pedidos-grid" />
            <ColumnChooser enabled={true} />
            <ColumnFixing enabled={true} />
            <Pager showInfo={true} visible={true} showPageSizeSelector={true} />
            <Paging defaultPageSize={10} />
            <SearchPanel visible={true} />
            <HeaderFilter visible={true} />
            <Pager
              showPageSizeSelector={true}
              allowedPageSizes={[5, 10, 20]}
              showInfo={true} />
            <Export enabled={true} fileName={`Ordenes Prestashop`} allowExportSelectedData={true} />
            <Column caption='Orden' width={100} allowResizing={false} dataField='id' allowExporting={false} allowFiltering={false} cellRender={verOrdenCellRender} >

            </Column>
            <Column caption='Información cliente'>
              <Column caption='Cliente' dataField='cliente'  allowSearch={true} />
              <Column caption='Email' dataField='email'  allowSearch={false} />
            </Column>
            <Column caption='Información Orden'>
              <Column caption='Referencia' dataField='reference'  allowSearch={true} />
              <Column
                caption='Fecha'
                dataField='dateCreated'
                dataType='date'
                alignment='right'
                format='dd/MM/yyyy'
                allowFiltering={true}
                allowSearch={false} />
              <Column caption='Válida' dataField='valid' dataType='boolean'  allowSearch={false} />
              <Column caption='Estado' dataField='status' allowFiltering={true} allowSearch={true} cellRender={estadoCellRender} />
            </Column>
            <Column caption='Información Pago'>
              <Column caption='Metodo' dataField='payment' />
              <Column caption='Valor' dataField='totalPaid' allowFiltering={false}  allowSearch={false} />
            </Column>
            <Column caption='Información Envio'>
              <Column caption='Metodo' dataField='carrierName' />
              <Column caption='Peso (Kg)' dataField='weight' allowFiltering={false}  allowSearch={false} />
              <Column caption='Valor' dataField='totalWrappingTaxIncl' allowFiltering={false} allowSearch={false} />
            </Column>
          </DataGrid>
        </div>
      </div>
    </>);
}


function calculateCellValue(data) {
  return [data.firstname, data.lastName].join(' ');
}