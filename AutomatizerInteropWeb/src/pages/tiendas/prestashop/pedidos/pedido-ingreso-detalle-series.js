import React, { useState } from "react";
import { FieldGroup } from "../../../../components/fieldGroup/fieldGroup";
import { Field } from "../../../../components/field/field";
import { CssLoadingIndicator } from "../../../../components/cssLoader/css-loader";
import { useEffect } from "react";
import { PrestashopService } from "../../../../_services/stores/prestashop-service";
import {  Button, TextBox } from "devextreme-react";
import { ItemsService } from "../../../../_services/inventario/items-service";
import notify from "devextreme/ui/notify";
import List from 'devextreme-react/list';


export default function PedidoIngresoDetalleSeries({ idDetalle, actualizarSeries }) {
  const [loading, setloading] = useState(true);
  const [detalle, setDetalle] = useState({});
  const [series, setSeries] = useState([]);
  const [seriesFiltradas, setSeriesFiltradas] = useState([]);
  const [seriesSeleccionadas, setSeriesSelecionadas] = useState([]);
  const [seriesActuales, setSeriesActuales] = useState(null);
  const [busqueda, setBusqueda] = useState('');
  const [serieFiltradaSeleccionada, setserieFiltradaSeleccionada] = useState(null);
  const guardarYCerrar = async () => {
    try {
      await PrestashopService.saveOrderDetailSeriesAutomatizer(idDetalle, seriesSeleccionadas.map(s => s.SecSerie));
      actualizarSeries( seriesActuales);
    } catch (error) {
      notify(error, 'error', 2500);
    }
  }

  const cargarDatos = async (id) => {
    const det = await PrestashopService.getOrdenDetalle(id);
    const ser = await ItemsService.getSeriesDisponibles(det.bodCodAutomatizer, det.idItemAutomatizer)
    
    if (det.Series) {
      const ss = det.Series;//.split(';').map(s => ser.find(x => x.NroSerie === s));
      // for (let index = 0; index < ss.length; index++) {
      //   const element = ss[index];
      //   const tmpIndx = ser.indexOf( element);
      //   ser.splice(tmpIndx, 1);
      // }
      const sa =  det.Series.map(s=> s.NroSerie);
      setSeriesActuales( sa.join(';') );
      setSeriesSelecionadas(ss);
      setSeries(ser);
      setSeriesFiltradas(ser);
    }
    else {
      setSeries(ser);
      setSeriesFiltradas(ser);
      setSeriesSelecionadas([]);
      setSeriesActuales("");
    }
    //const se = await Service
    setDetalle(det);
    setloading(false);
  }

  const filtrarSeries = (filtro) => {
    const tmpList = series.filter(s => s.NroSerie.includes(filtro.toUpperCase()));
    setSeriesFiltradas(tmpList);
    if (tmpList.length > 0) {
      setserieFiltradaSeleccionada(tmpList[0]);
    }
    else {
      setserieFiltradaSeleccionada(null);
    }
  }


  const agregarSerieSeleccionada = () => {
    if (serieFiltradaSeleccionada && seriesSeleccionadas.length < detalle.product_quantity) {
      const tmpSeries = series;
      const indx = tmpSeries.indexOf(serieFiltradaSeleccionada);
      tmpSeries.splice(indx, 1)
      setSeries(tmpSeries);
      seriesSeleccionadas.push(serieFiltradaSeleccionada);
      setSeriesSelecionadas(seriesSeleccionadas.slice(0));
      setserieFiltradaSeleccionada(null);
      setBusqueda("");
      filtrarSeries("");
      const tmp = seriesSeleccionadas.map(sec => sec.NroSerie);
      setSeriesActuales(tmp.join(';'));
    }
  }


  const quitarSerieSeleccionada=(serie)=>{
    const tmpSeries = series;
    const tmpSele = seriesSeleccionadas;
    const indx = tmpSele.indexOf(serie);
    tmpSele.splice(indx, 1);
    tmpSeries.push(serie);
    setSeries(tmpSeries.slice(0).sort( (a,b)=> a.NroSerie.localeCompare(b.NroSerie) ) );
    setSeriesSelecionadas(tmpSele.slice(0));
    setSeriesFiltradas(tmpSeries.slice(0).sort( (a,b)=> a.NroSerie.localeCompare(b.NroSerie) ) );
    setBusqueda("");
    
    setSeriesActuales(tmpSele.join(';'));

  }

  // const onValueChanged = (e) => {
  //   if (e.value.length > detalle.product_quantity) {
  //     const newValue = e.value.slice(0, detalle.product_quantity);
  //     e.component.option('value', newValue);
  //     const tmp = newValue.map(sec => series.find(x => x.SecSerie === sec).NroSerie);
  //     setSeriesActuales(tmp.join(';'));
  //     setSeriesSelecionadas(newValue);
  //   }
  //   else {
  //     const tmp = e.value.map(sec => series.find(x => x.SecSerie === sec).NroSerie);
  //     setSeriesActuales(tmp.join(';'));
  //     setSeriesSelecionadas(e.value);
  //   }

  // }

  useEffect(() => {
    cargarDatos(idDetalle);
  }, [idDetalle]);

  if (loading) {
    return <CssLoadingIndicator />
  }

  return (
    <>
      <FieldGroup>
        <Field label="Codigo" size='s1'>
          <span>{detalle.product_reference}</span>
        </Field>
        <Field label="Descripción" size='x2'>
          <span>{detalle.product_name}</span>
        </Field>
        <Field label="Cantidad" size='s1'>
          <span>{detalle.product_quantity}</span>
        </Field>
      </FieldGroup>
      <FieldGroup>
        <div className='seriesSelector'>
          <div className='seriesSearch'>
            <TextBox value={busqueda} valueChangeEvent='keyup' placeholder='Digite la serie' onKeyUp={(e) => {
              const charCode = (e.event.charCode) ? e.event.charCode : ((e.event.keyCode) ? e.event.keyCode :
                ((e.event.which) ? e.event.which : 0));
              //console.log(charCode);
              if (charCode === 13) {
                agregarSerieSeleccionada();
              }

            }} onValueChanged={(e) => {
              setBusqueda(e.value.toUpperCase());
              filtrarSeries(e.value);
            }} />
          </div>
          <div className='listadoSeries'>
            <List
              items={seriesFiltradas}
              keyExpr="SecSerie"
              displayExpr='NroSerie'
              selectedItem={serieFiltradaSeleccionada}
            >

            </List>
          </div>
          <div className='seriesSeleccionadas'>
            <List
              items={seriesSeleccionadas}
              displayExpr='NroSerie'
              keyExpr="SecSerie"
              itemRender={(props)=>{
                return(
                  <div  key={props.SecSerie} onDoubleClick={()=>{
                    quitarSerieSeleccionada(props);
                    }} >
                    {props.NroSerie}
                  </div>
                )
              }}
              
            >

            </List>
          </div>
        </div>

      </FieldGroup>
      {/* <FieldGroup>
        <TagBox  searchTimeout={250} width='100%' items={series} onValueChanged={onValueChanged}
          defaultValue={seriesSelecionadas} hideSelectedItems={true} showDataBeforeSearch={true}
          searchEnabled={true} displayExpr='NroSerie' valueExpr='SecSerie' showSelectionControls={false} />
      </FieldGroup> */}
      <div className="commandBar">
        <Button
          text='Aceptar'
          stylingMode='contained'
          type='success'
          height={40}
          width={120}
          onClick={guardarYCerrar} />

      </div>
    </>
  );
}