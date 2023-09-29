import React, { useMemo, useRef } from 'react';
import { FieldGroup } from '../../../../components/fieldGroup/fieldGroup';
import { Field } from '../../../../components/field/field';
import { useState } from 'react';
import { useEffect } from 'react';
import { CssLoadingIndicator } from '../../../../components/cssLoader/css-loader';
import { antivirusLicenciasService } from '../../../../_services/antivirusLicencias.service';
import notify from 'devextreme/ui/notify';
import { SelectBox, NumberBox, DropDownBox, TextArea, Button, LoadIndicator, Popup } from 'devextreme-react';
import { Lookup, DropDownOptions } from 'devextreme-react/lookup';
import { TransaccionesService } from '../../../../_services/transacciones.service';
import { config } from '../../../../constants/constants';
import AspNetData from 'devextreme-aspnet-data-nojquery';
import { authHeader } from '../../../../_helpers';
import UserSesion from '../../../../utils/userSesion';
import DataGrid, { SearchPanel, Column, Selection, Scrolling, Paging, FilterRow } from 'devextreme-react/data-grid';
import { PreciosService } from '../../../../_services/facturacion/precios.service';
import { Label } from 'devextreme-react/bar-gauge';
import { InventarioService } from '../../../../_services/inventario.service';
import AnvitirusLicenciasIngresarSeries from './antivirus-licencias-ingreso-series';
import '../licencias.scss';
export default function AntivirusLicenciasRenovarLicenciaFacturar({ idVenta }) {
  const [loading, setLoading] = useState(true);
  const [infoVenta, setInfoVenta] = useState(null);
  const [facturando, setfacturando] = useState(false);
  const [facturado, setfacturado] = useState(false);
  const [transacciones, settransacciones] = useState([]);
  const [transaccion, setTransaccion] = useState(null);
  const [antivirus, setAntivirus] = useState(null);
  const [producto, setProducto] = useState(null);
  const [series, setSeries] = useState([]);
  const [tieneIva, setTieneIva] = useState(false);
  const [cantidad, setCantidad] = useState(0);
  const [precio, setPrecio] = useState(0);
  const [efectivo, setEfectivo] = useState(0);
  const [credito, setCredito] = useState(0);
  const [diasCredito, setdiasCredito] = useState(1);
  const [observaciones, setObservaciones] = useState('');
  const auth = useMemo(() => authHeader(), []);
  const grid = useRef(null);
  const dropDown = useRef(null);
  const [showIngresarSeries, setShowIngresarSeries] = useState(false);
  const [bodega, setBodega] = useState('B1');

  const cargarDatos = async (id) => {
    try {
      const info = await antivirusLicenciasService.getInfoVentaAntivirus(id, true);
      const trn = await TransaccionesService.getTransacciones(8, 'FA');
      settransacciones(trn);
      setInfoVenta(info);
      setCantidad(info.ventaAntivirus.cantidad);
      setAntivirus([info.ventaAntivirus.antivirusId]);
      setLoading(false);
    } catch (error) {
      notify(error, 'error', 3500);
    }
  };

  const dataGrid_onSelectionChanged = (e) => {
    setAntivirus(e.selectedRowKeys);
  }

  const syncDataGridSelection = (e) => {
    setAntivirus(e.value);
  }

  const gridBox_displayExpr = (item) => {
    return item && `${item.referencia} - ${item.nombre} <${item.bodega}>`;
  }

  const moveArrows = (e) => {
    var selKey = grid.current.instance.getSelectedRowKeys();
    if (selKey.length) {
      var currentKey = selKey[0];
      var index = grid.current.instance.getRowIndexByKey(currentKey);
      if (e.event.keyCode === 38) {
        index--;
        if (index >= 0) {
          grid.current.instance.selectRowsByIndexes([index]);
        }
      }
      else if (e.event.keyCode === 40) {
        index++;
        grid.current.instance.selectRowsByIndexes([index]);
        // if (index >= this._count) {
        //   grid.current.instance.selectRowsByIndexes([0]);
        // } else {
        //   grid.current.instance.selectRowsByIndexes([index]);
        // }
      } else if (e.event.keyCode === 13) {
        dropDown.current.instance.close();
      }
    }
    else {
      grid.current.instance.selectRowsByIndexes([0]);
    }
    if (e.event.keyCode === 38 || e.event.keyCode === 40 || e.event.keyCode === 13) {
      e.event.preventDefault();
      e.event.stopPropagation();
    }
  }

  const onGridEditorPreparing = (e) => {
    if (e.parentType === "searchPanel") {
      e.editorOptions.onKeyDown = moveArrows;
    }
  }



  const itemsSource = useMemo(() => {
    return AspNetData.createStore({
      key: 'antivirusId',
      loadUrl: `${config.url.API_URL}/api/ProductsSearch/BusquedaAntivirus`,
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


  const dataGridRender = () => {
    return (
      <DataGrid
        ref={grid}
        dataSource={itemsSource}
        hoverStateEnabled={true}
        selectedRowKeys={antivirus}
        onSelectionChanged={dataGrid_onSelectionChanged}
        onEditorPreparing={onGridEditorPreparing}
        onKeyDown={(e) => {
          console.log(e);
        }}
        height="100%"
      >
        <Selection mode="single" />
        <Scrolling mode="infinite" />
        <Paging enabled={true} pageSize={10} />
        <FilterRow visible={false} />
        <SearchPanel visible={true} />
        <Column dataField='referencia' width={100} allowSearch={true} />
        <Column dataField='nombre' width={430} allowSearch={true} />
        <Column dataField='bodega' width={80} allowSearch={false} />
        <Column dataField='saldo' width={80} allowSearch={false} />
      </DataGrid>
    );
  }

  const cargarPrecio = async (av, iv) => {
    if (av && av.length > 0) {
      const prod = await InventarioService.getItemDynamic(av[0]);
      const prec = await PreciosService.getPrecioItem(iv.cliente.tipoClienteId, av[0])
      setPrecio(prec.precio)
      setTieneIva(prec.calculaIva);
      setProducto(prod);
      setSeries([]);
    }
  }


  const facturar = async () => {
    const ttotalFp = round(efectivo + credito, 2);
    const total = round(((cantidad * precio) + (tieneIva ? (cantidad * precio) * 0.12 : 0)), 2);
    if (total !== ttotalFp) {
      notify('Revise las formas de pago!!!', 'error', 2500);
      return;
    }

    if (producto.TieneSeries && series.length < cantidad) {
      notify('Faltan ingresar series', 'error', 2500);
      return;
    }

    const data = {
      ventaId: idVenta,
      transaccionCodigo: transaccion,
      productoId: antivirus[0],
      cantidad: cantidad,
      precio: precio,
      efectivo: efectivo,
      credito: credito,
      diasCredito: diasCredito,
      observaciones: observaciones,
      series: series.map(s => s.SecSerie),
      bodegaCodigo: bodega
    };
    try {
      setfacturando(true);
      const renov = await antivirusLicenciasService.renovarLicencia(idVenta, data);
      if (renov.isOk) {
        notify('Licencia renovada ' + renov.id, 'success', 3500);
        setfacturado(true);
      }
      else {
        notify(renov.error, 'error', 3500);
      }
    } catch (error) {
      notify(error, 'error', 3500);
    }
    setfacturando(false);
  }

  useEffect(() => {
    cargarDatos(idVenta);
  }, [idVenta])


  useEffect(() => {
    cargarPrecio(antivirus, infoVenta)
  }, [infoVenta, antivirus])

  if (loading) {
    return <CssLoadingIndicator />
  }

  return (
    <div>
      <FieldGroup>
        <Field label='Cliente' size='x2'>
          <span>{infoVenta.cliente.nombreCompletos}</span>
        </Field>
        <Field label='CI/RUC' size='x1'>
          <span>{infoVenta.cliente.identificacion}</span>
        </Field>
      </FieldGroup>
      <FieldGroup>
        <Field label='Transacción' size='x3'>
          <SelectBox items={transacciones}
            placeholder="Seleccione la Transacción"
            displayExpr='nombre'
            valueExpr='codigo'
            selectedItem={transaccion}
            onValueChanged={({ value }) => {
              setTransaccion(value);
              const trn = transacciones.filter(x => x.codigo === value);
              if (trn.length > 0) {
                setdiasCredito(trn[0].diasCredito);
              }
            }}
            showClearButton={true}
            disabled={facturado} />
        </Field>
      </FieldGroup>
      <FieldGroup>
        <Field label='Producto' size='x3'>
          <DropDownBox
            ref={dropDown}
            value={antivirus}
            valueExpr='antivirusId'
            deferRendering={false}
            displayExpr={gridBox_displayExpr}
            dataSource={itemsSource}
            showClearButton={true}
            onValueChanged={syncDataGridSelection}
            contentRender={dataGridRender}
            dropDownOptions={{ width: 720, height: 400 }}
          >
          </DropDownBox>
        </Field>
      </FieldGroup>
      <FieldGroup>
        <Field label='Cantidad' size='s1'>
          <NumberBox showSpinButtons={true} value={cantidad} onValueChanged={(e) => setCantidad(e.value)} />
        </Field>
        <Field label='Precio' size='s1'>
          <NumberBox value={precio} onValueChanged={(e) => setPrecio(e.value)} />
        </Field>
        <Field label='Subtotal' size='s1'>
          <span >{(cantidad * precio).toFixed(2)}</span>
        </Field>
        <Field label='Impuesto' size='s1'>
          <span>{(tieneIva ? (cantidad * precio) * 0.12 : 0).toFixed(2)}</span>
        </Field>
        <Field label='Total' size='s1'>
          <span>{((cantidad * precio) + (tieneIva ? (cantidad * precio) * 0.12 : 0)).toFixed(2)}</span>
        </Field>
      </FieldGroup>
      {producto && producto.TieneSeries &&
        <FieldGroup>
          <Field label='Series' height={50 + (5 * series.length)} size='x3' >
            <div id='detalleSeries' className='antivirusLicenciasDetalleSeries' >
              {series.map(item => <span className='antivirusLicenciasDetalleSeries-serie' onClick={() => setShowIngresarSeries(true)} key={'serie' + item.SecSerie} >{item.NroSerie}</span>)}
              {series.length === 0 &&
                <Button text='Editar' onClick={() => setShowIngresarSeries(true)} />
              }
            </div>
            <Popup
              title="Detalle de Series"
              showTitle={true}
              visible={showIngresarSeries}
              onHiding={() => setShowIngresarSeries(false)}
              width={500}
              height={550}
            >
              <div id='ingresoSeriesAntivirus'>
                {showIngresarSeries &&
                  <AnvitirusLicenciasIngresarSeries
                    itemId={producto.ItemID}
                    nombre={producto.Nombre}
                    codigo={producto.Referencia}
                    cantidad={cantidad}
                    bodCod={bodega}
                    seleccionadas={series}
                    actualizar={(s) => {
                      console.log(s);
                      setShowIngresarSeries(false);
                      setSeries(s);
                    }} />
                }
              </div>
            </Popup>
          </Field>
        </FieldGroup>
      }
      <FieldGroup>
        <Field label='Observaciones' height={80} size='x3'>
          <TextArea value={observaciones} onValueChanged={(e) => setObservaciones(e.value)} maxLength={800} />
        </Field>
      </FieldGroup>
      <FieldGroup>
        <Field min={0} label='Efectivo' size='s1'>
          <NumberBox value={efectivo} onValueChanged={(e) => {
            const ef = e.value;
            //const total = ((cantidad * precio) + (tieneIva ? (cantidad * precio) * 0.12 : 0));
            setEfectivo(ef);
            //setCredito(total - ef);
          }} />
          <Button icon='check' hint='Todo efectivo' onClick={(e) => {
            const total = round(((cantidad * precio) + (tieneIva ? (cantidad * precio) * 0.12 : 0)), 2)
            setEfectivo(total);
            setCredito(0);
          }} />
        </Field>
        <Field label='Crédito' size='s1'>
          <NumberBox min={0} value={credito} onValueChanged={(e) => {
            const cr = e.value;
            //const total = ((cantidad * precio) + (tieneIva ? (cantidad * precio) * 0.12 : 0));
            setCredito(cr);
            //setEfectivo(Math.fround( total - cr) );
          }} />
          <Button icon='check' hint='Todo crédito' onClick={(e) => {
            const total = round(((cantidad * precio) + (tieneIva ? (cantidad * precio) * 0.12 : 0)), 2)
            setEfectivo(0);
            setCredito(total);
          }} />
        </Field>
        {credito > 0 &&
          < Field label='# días' size='s1'>
            <NumberBox max={250} min={1} value={diasCredito} showSpinButtons={true} />
          </Field>
        }
      </FieldGroup>
      <div style={{ display: 'flex' }}>
        <div style={{ margin: 'auto' }}>
          <Button
            width={160}
            height={40}
            stylingMode='contained'
            onClick={facturar}
            disabled={!transaccion || !antivirus || antivirus.Length === 0 || cantidad <= 0 || precio <= 0 || facturado}
          >
            <LoadIndicator className="button-indicator" visible={facturando} />
            <span className="dx-button-text">{(facturando ? 'Facturando...' : 'Facturar')}</span>
          </Button>
        </div>
      </div>
    </div >
  )
}


function round(value, decimals) {
  return Number(Math.round(value + 'e' + decimals) + 'e-' + decimals);
}