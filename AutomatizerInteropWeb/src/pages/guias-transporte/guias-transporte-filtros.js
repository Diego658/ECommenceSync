import './guias-transporte.scss'
import React, { useState } from 'react';
import { GroupPanel } from '../../components/panel/GroupPanel';
import { Field } from '../../components/field/field';
import { DateBox, SelectBox, TextBox, Button, NumberBox, Popup } from 'devextreme-react';
import { useEffect } from 'react';
import { GuiasTransporteService } from '../../_services/guiasTransporte.service';
import { max } from 'moment';
import GuiasTransporteAgregarGuia from './guias-transporte-nuevaguia';

export function GuiasTransporteFiltros({ cargarDatos }) {
  //const [loading, setloading] = useState(true);
  const [fechaInicio, setFechaInicio] = useState(new Date());
  const [fechaFin, setFechaFin] = useState(new Date());
  const [usuarios, setUsuarios] = useState(null);
  const [usuario, setUsuario] = useState(null);
  const [cliente, setCliente] = useState(null);
  const [compania, setCompania] = useState(null);
  const [companias, setCompanias] = useState(null);
  const [numeroPiezas, setNumeroPiezas] = useState(0);
  const [numeroGuia, setNumeroGuia] = useState('');
  const [filtros, setFiltros] = useState({});
  const [showNuevo, setShowNuevo] = useState(false);

  const cargarCompanias = async () => {
    var list = await GuiasTransporteService.getCompaniasTransporte();
    setCompanias(list);
  }


  const cargarUsuarios = async () => {
    var list = await GuiasTransporteService.getUsuariosGuias();
    setUsuarios(list);
  }


  useEffect(() => {
    const now = new Date();
    const inicio = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0);
    const fin = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 23, 59, 59);
    const f = { fechaInicio: inicio.toJSON(), fechaFin: fin.toJSON() };
    setFechaInicio(inicio);
    setFechaFin(fin);
    setFiltros(f);
    cargarCompanias();
    cargarUsuarios();
  }, []);

  return (
    <div  >
      <GroupPanel header='Criterios de búsqueda'>
        <div className='filtrosContainer'>
          <div className='filtroRangofechas'>
            <GroupPanel header='Rango de Fechas'>
              <Field label='Desde'>
                <DateBox value={fechaInicio} pickerType='native' type='datetime' onValueChanged={(e) => {
                  setFechaInicio(e.value);
                  if (e.value) {
                    setFiltros({ ...filtros, fechaInicio: e.value.toJSON() });
                  }

                }} />
              </Field>
              <Field label='Hasta'>
                <DateBox value={fechaFin} pickerType='native' type='datetime' onValueChanged={(e) => {
                  setFechaFin(e.value);
                  if (e.value) {
                    setFiltros({ ...filtros, fechaFin: e.value.toJSON() });
                  }

                }} />
              </Field>
            </GroupPanel>
          </div>
          <div className='filtroUsuarios'>
            <GroupPanel header='Usuario'>
              <SelectBox
                searchEnabled={true}
                searchMode='contains'
                showDataBeforeSearch={true}
                selectedItem={usuario}
                items={usuarios} width={250} showClearButton={true} onValueChanged={(e) => {
                  setUsuario(e.value);
                  const tmp = (e.value != null ? e.value : '');
                  setFiltros({ ...filtros, usuario: tmp });
                }} />
            </GroupPanel>
          </div>
          <div className='filtroClientes'>
            <GroupPanel header='Cliente'>
              <SelectBox showClearButton={true} width={250} />
            </GroupPanel>
          </div>
          <div className='filtroNroPiezas'>
            <GroupPanel header='# Piezas'>
              <NumberBox min={0} max={250} showClearButton={true} value={numeroPiezas} onValueChanged={(e) => {
                setNumeroPiezas(e.value);
                setFiltros({ ...filtros, numeroPiezas: (e.value ? e.value : 0) });

              }} />
            </GroupPanel>
          </div>
          <div className='filtroNroGuia'>
            <GroupPanel header='# Guia'>
              <TextBox showClearButton={true} value={numeroGuia} onValueChanged={(e) => {
                setNumeroGuia(e.value);
                setFiltros({ ...filtros, numeroGuia: (e.value ? e.value : '') });
              }} />
            </GroupPanel>
          </div>
          <div className='filtroCompaniaTransporte'>
            <GroupPanel header='Compañias de Transporte'>
              <SelectBox
                items={companias}
                searchEnabled={true}
                searchMode='contains'
                showDataBeforeSearch={true}
                displayExpr='nombre'
                valueExpr='companiaID'
                showClearButton={true}
                width={250}
                onValueChanged={(e) => {
                  setCompania(e.value);
                  setFiltros({ ...filtros, companiaId: (e.value ? e.value : 0) });
                }}
              />
            </GroupPanel>
          </div>
          <div className='filtrosButtonBar'>
            <div className='filtrosButton'>
              <Button text='Nueva Guía' onClick={(e) => setShowNuevo(true)} />
              <Popup
                title='Agregar Guía'
                showTitle={true}
                visible={showNuevo}
                onHiding={() => setShowNuevo(false)}
                closeOnOutsideClick={false}
                width='900px'
                height='700px'
              >
                <div id='agregraGuia'>
                  {showNuevo &&
                    <GuiasTransporteAgregarGuia/>
                  }
                </div>
              </Popup>
            </div>
            <div className='filtrosButton'>
              <Button text='Cargar Datos' onClick={() => cargarDatos(filtros)} />
            </div>
            <div className='filtrosButton'>
              <Button text='Etiquetas Paquetes' />
            </div>
          </div>
        </div>
      </GroupPanel>
    </div>
  );
}