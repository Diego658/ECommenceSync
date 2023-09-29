import React, { useState } from "react";
import { FieldGroup } from "../../../../components/fieldGroup/fieldGroup";
import { Field } from "../../../../components/field/field";
import { CssLoadingIndicator } from "../../../../components/cssLoader/css-loader";
import { useEffect } from "react";
import { Button, TextBox } from "devextreme-react";
import { ItemsService } from "../../../../_services/inventario/items-service";
import notify from "devextreme/ui/notify";
import List from 'devextreme-react/list';


export default function AnvitirusLicenciasIngresarSeries({ itemId, nombre, codigo, cantidad, bodCod, seleccionadas, actualizar }) {
  const [loading, setloading] = useState(true);
  const [series, setSeries] = useState([]);
  const [seriesFiltradas, setSeriesFiltradas] = useState([]);
  const [seriesSeleccionadas, setSeriesSelecionadas] = useState([]);
  const [busqueda, setBusqueda] = useState('');
  const [serieFiltradaSeleccionada, setserieFiltradaSeleccionada] = useState(null);

  const guardarYCerrar = async () => {
    try {
      actualizar(seriesSeleccionadas);
    } catch (error) {
      notify(error, 'error', 2500);
    }
  }

  const cargarDatos = async (id, sel, bod) => {
    const ser = await ItemsService.getSeriesDisponibles(bod, id)
    if (sel && sel.length > 0) {
      setSeriesSelecionadas(sel);
      const sf = ser.filter(x => {
        return sel.findIndex(z => x.NroSerie === z.NroSerie) === -1;
      });
      setSeries(sf);
      setSeriesFiltradas(sf);
    } else {
      setSeries(ser);
      setSeriesFiltradas(ser);
      setSeriesSelecionadas([]);
    }
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


  const agregarSerieSeleccionada = (s) => {
    const serieAgregar = s ?? serieFiltradaSeleccionada;
    if(!serieAgregar){
      return;
    }
    if (seriesSeleccionadas.length < cantidad) {
      const tmpSeries = series;
      const indx = tmpSeries.indexOf(serieAgregar);
      tmpSeries.splice(indx, 1)
      setSeries(tmpSeries);
      seriesSeleccionadas.push(serieAgregar);
      setSeriesSelecionadas(seriesSeleccionadas.slice(0));
      setserieFiltradaSeleccionada(null);
      setBusqueda("");
      filtrarSeries("");
    }
    else {
      notify('Las series ya estan completas!!!', 'error', 2500);
    }
  }


  const quitarSerieSeleccionada = (serie) => {
    const tmpSeries = series;
    const tmpSele = seriesSeleccionadas;
    const indx = tmpSele.indexOf(serie);
    tmpSele.splice(indx, 1);
    tmpSeries.push(serie);
    setSeries(tmpSeries.slice(0).sort((a, b) => a.NroSerie.localeCompare(b.NroSerie)));
    setSeriesSelecionadas(tmpSele.slice(0));
    setSeriesFiltradas(tmpSeries.slice(0).sort((a, b) => a.NroSerie.localeCompare(b.NroSerie)));
    setBusqueda("");

  }


  useEffect(() => {
    cargarDatos(itemId, seleccionadas, bodCod);
  }, [itemId, seleccionadas, bodCod]);

  if (loading) {
    return <CssLoadingIndicator />
  }

  return (
    <>
      <FieldGroup>
        <Field label="Codigo" size='s1'>
          <span>{codigo}</span>
        </Field>
        <Field label="Descripción" size='x2'>
          <span>{nombre}</span>
        </Field>
        <Field label="Cantidad" size='s1'>
          <span>{cantidad}</span>
        </Field>
      </FieldGroup>
      <FieldGroup>
        <div className='seriesSelector'>
          <div className='seriesSearch'>
            <TextBox value={busqueda} valueChangeEvent='keyup' placeholder='Digite la serie' onKeyUp={(e) => {
              const charCode = (e.event.charCode) ? e.event.charCode : ((e.event.keyCode) ? e.event.keyCode :
                ((e.event.which) ? e.event.which : 0));
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
              itemRender={(props) => {
                return (
                  <div key={props.SecSerie} onDoubleClick={() => {
                    agregarSerieSeleccionada(props);
                  }}
                  >
                    {props.NroSerie}
                  </div>
                )
              }}
            >

            </List>
          </div>
          <div className='seriesSeleccionadas'>
            <List
              items={seriesSeleccionadas}
              displayExpr='NroSerie'
              keyExpr="SecSerie"
              itemRender={(props) => {
                return (
                  <div key={props.SecSerie} onDoubleClick={() => {
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