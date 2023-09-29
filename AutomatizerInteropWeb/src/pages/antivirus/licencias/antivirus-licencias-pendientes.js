import React, { useState, useCallback } from 'react'
import { LoadIndicator } from 'devextreme-react';
import { useEffect } from 'react';
import { antivirusLicenciasService } from '../../../_services/antivirusLicencias.service';
import notify from 'devextreme/ui/notify';
import DataSource from 'devextreme/data/data_source';
import DataGrid, { Column, FilterRow, StateStoring, Export, ColumnChooser, ColumnFixing } from 'devextreme-react/data-grid';
import RangeSelector, { Scale, Behavior, SliderMarker, MinorTick } from 'devextreme-react/range-selector';

import { AntivirusLicenciaUltimaNotificacionInfo } from './antivirus-licencias-ultimanotificacioninfo';
import './licencias.scss'
import { Popup } from 'devextreme-react/popup';
import { AntivirusLicenciasInfoVentaAntivirus } from './AntivirusLicenciasInfoVentaAntivirus';




export function AntivirusLicenciasPendientes({ tipoAVisualizar }) {
  const [licencias, setLicencias] = useState([]);
  const [licenciasFiltradas, setLicenciasFiltradas] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showVentaInfo, setshowVentaInfo] = useState(false);
  const [ventaSeleccionada, setVentaSeleccionada] = useState({ kardexId: 0 });

  const filtrarFechasVencimiento = (ventas, dateStart, dateEnd) => {
    //console.log("filtrando....");
    const filtradas = [];
    for (let index = 0; index < ventas.length; index++) {
      const elementDate = ventas[index].fechaVencimiento;
      if (elementDate >= dateStart && elementDate <= dateEnd) {
        filtradas.push(ventas[index]);
      }
    }
    return filtradas;
  };


  const cargarLicencias = useCallback(async () => {
    try {
      setLoading(true);
      const info = await antivirusLicenciasService.getVentasPorRenovar(tipoAVisualizar === 3, tipoAVisualizar, false);
      const startDateparts = info.minValue.split('-');
      const endDateparts = info.maxValue.split('-');
      const ventas = await Promise.all(info.ventas.map(async (venta) => {
        const dateparts = venta.fechaVencimiento.split('-');
        return { ...venta, fechaVencimiento: new Date(dateparts[0], dateparts[1] - 1, dateparts[2].substring(0, 2) - 1), factura: venta.trnCod + '-' + venta.trnNum };
      }));
      const start = new Date(startDateparts[0], startDateparts[1] - 1, startDateparts[2].substring(0, 2) - 1);
      const end = new Date(endDateparts[0], endDateparts[1] - 1, endDateparts[2].substring(0, 2) - 1);
      setLicencias({
        licencias: ventas,
        startValue: start,
        endValue: end,
        range: [new Date(), end],
        numeroRegistros: info.numeroRegistros
      });

      setLicenciasFiltradas(filtrarFechasVencimiento(ventas, new Date(), end));
      //                setLicenciasFiltradas(lice)


    } catch (error) {
      notify("Error al recuperar los datos " + error, "error", 2000);
    } finally {
      setLoading(false);
    }

  }, [tipoAVisualizar]);

  useEffect(() => {
    cargarLicencias();
  }, [cargarLicencias]);



  const gridSource = new DataSource({
    store: {
      key: 'kardexId',
      type: 'array',
      data: licenciasFiltradas
    },
    sort: { getter: 'grupoVencimientoNumero', desc: false }
  });



  // const onOcultarClick = ({ row }) => {
  //   //console.log(row);
  // }

  // const onVerClick = ({ row }) => {
  //   //console.log(row);
  //   setVentaSeleccionada(row.data);
  //   setshowVentaInfo(true);
  // }


  const onHiddingpopUp = () => {
    setshowVentaInfo(false);
  }

  // const onNotificarClick = (e) => {

  // }

  // const onRegistrarRenovacionClick = (e) => {

  // }

  const ultimaNotificacionCellRender = ((data) => {
    if (data.data.ultimaNotificacionID > 0) {
      return (
        <>
          <AntivirusLicenciaUltimaNotificacionInfo notificacionId={data.data.ultimaNotificacionID} />
        </>
      );
    }
    else {
      return (
        <div className="cellUltimaNotificacion">
          <i className="dx-icon-message tooltip" >
            <span className="tooltiptext">Nunca Notificado</span>
          </i>
        </div>
      );
    }

  });


  const verLicenciaCellRender = ((data) => {
    return (
      <AntivirusLicenciasInfoVentaAntivirus idLicencia={data.data.kardexId} codigoCliente={data.data.codigoCliente} />
    )
  })



  const onFocusedRowChanged = ({ row }) => {
    //console.log(row);
  }

  const onRangeChanged = ({ value }) => {
    setLicenciasFiltradas(filtrarFechasVencimiento(licencias.licencias, value[0], value[1]));
  }


  const onToolbarPreparing = (e) => {
    e.toolbarOptions.items.unshift({
      location: 'before',
      widget: 'dxButton',
      options: {
        width: 136,
        text: 'Recargar',
        icon: 'refresh',
        onClick: () => cargarLicencias()
      }
    }
    );
  }



  if (loading) {
    return (
      <>
        <LoadIndicator />
      </>
    );
  }



  return (
    <>
      <RangeSelector
        id="range-selector"
        defaultValue={licencias.range}
        onValueChanged={onRangeChanged}
        height="100px"
      >

        <Scale startValue={licencias.startValue} endValue={licencias.endValue} minorTickInterval="week" tickInterval="week" minRange="week" maxRange="year">
          <MinorTick visible={false} />
        </Scale>
        <SliderMarker format="monthAndDay" />
        <Behavior callValueChanged="onMovingComplete" />
      </RangeSelector>
      <DataGrid
        dataSource={gridSource}
        wordWrapEnabled={true}
        focusedRowEnabled={true}
        allowColumnReordering={true}
        allowColumnResizing={true}
        columnAutoWidth={true}
        onFocusedRowChanged={onFocusedRowChanged}
        onToolbarPreparing={onToolbarPreparing}
        showBorders={true}
        width='100%'
      >
        <ColumnChooser enabled={true} />
        <ColumnFixing enabled={true} />
        <FilterRow visible={true} />
        <StateStoring enabled={true} type="localStorage" storageKey="antiviruslicenciaspendientes-grid" />
        <Export enabled={true} fileName="RenovacionesAntivirus" allowExportSelectedData={true} />
        <Column caption="Información cliente"  >
          <Column dataField="kardexId" cellRender={verLicenciaCellRender} caption="Tareas" allowFiltering={false} allowHiding={false} allowExporting={false} width={30} />
          <Column dataField="codigoCliente" caption="Cod. Cli" />
          <Column dataField="identificacionCliente" caption="CI/RUC" />
          <Column dataField="cliente" minWidth={150} />
        </Column>
        <Column caption="Información Venta">
          <Column dataField="producto" minWidth={150} />
          <Column dataField="codigoVenta" caption="Referencia" minWidth={80} />
          <Column dataField="cantidad" minWidth={80} />
          <Column dataField="factura" caption="Factura" />
        </Column>
        <Column caption="Vencimiento / Notificaciones">
          <Column dataField="fechaVencimiento" caption='F. Venc.' dataType="date" minWidth={60} format="dd-MM-yyyy" />
          <Column dataField="fechaUltimaNotificacion" caption='Ult. Not.' dataType="date" minWidth={60} format="dd-MM-yyyy" />
          <Column dataField="numeroNotificaciones" minWidth={40} caption='# Noti.' />
          <Column dataField="ultimaNotificacionID" cellRender={ultimaNotificacionCellRender} caption="Ult. Noti" allowFiltering={false} />
        </Column>
      </DataGrid>
      <Popup
        visible={showVentaInfo}
        onHiding={onHiddingpopUp}
        dragEnabled={false}
        closeOnOutsideClick={true}
        showTitle={true}
        title="Información venta antivirus"
        width={1000}
        height={600}
      >
        <div>
          <AntivirusLicenciasInfoVentaAntivirus idLicencia={ventaSeleccionada.kardexId} />
        </div>
      </Popup>
    </>
  );
}


