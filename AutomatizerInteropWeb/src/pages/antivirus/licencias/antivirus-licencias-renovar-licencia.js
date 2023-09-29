import React, { useState } from 'react'
import { Button, Popup } from 'devextreme-react'
import { Popover } from 'devextreme-react/popover';
import { FieldGroup } from '../../../components/fieldGroup/fieldGroup';
import { RadioGroup } from 'devextreme-react';
import { AntivirusLicenciasRenovarLicenciaFacturada } from './antivirus-licencias-renovar/antivirus-licencias-renovar-facturada';
import AntivirusLicenciasRenovarLicenciaFacturar from './antivirus-licencias-renovar/antivirus-licencias-renovar-facturar';

const PopoverAnimationConfig = {
  show: {
    type: 'slide',
    from: {
      top: -100,
      opacity: 0
    },
    to: {
      top: 0,
      opacity: 1
    }
  },
  hide: {
    type: 'pop',
    from: {
      scale: 1,
      opacity: 1
    },
    to: {
      scale: 0.1,
      opacity: 0
    }
  }
};


//const renovarOpciones = [{ ID: 1, Text: 'Renovar (Facturado)' }, { ID: 2, Text: 'Renovar (Facturar)' }];

const TIPO_YA_FACTURADO = 'YAFACTURADO';
const TIPO_FACTURAR = 'FACTURAR';

export const formasRenovacion = [TIPO_YA_FACTURADO, TIPO_FACTURAR];


export function AntivirusLicenciasRenovarLicencia({ idLicencia, clienteCodigo, onUpdated }) {
  const [showTooltip, setShowTooltip] = useState(false);
  const [show, setShow] = useState(false);
  // const [facturas, setFacturas] = useState([]);
  // const [transacciones, settransacciones] = useState([]);
  const [renovado, setRenovado] = useState(false);
  //const [transaccionCodigo, setTransaccionCodigo] = useState('');
  //const [transaccion, setTransaccion] = useState(null);
  const [tipoRenovacion, setTipoRenovacion] = useState(TIPO_YA_FACTURADO);

  const toggleTooltip = () => {
    setShowTooltip(!showTooltip);
  }

  const toogleShow = () => {
    //const localShow = show;
    setShow(!show);
    setShowTooltip(false);
    // if (transacciones.length === 0 && !localShow) {
    //   TransaccionesService.getTransacciones(8, 'FA')
    //     .then(response => {
    //       settransacciones(response.filter(c => c.devolucion === false));
    //     }, error => notify('Erro al cargar las transacciones ' + error))
    // }
  }

  // const onTransaccionChanged = ({ value }) => {
  //   antivirusLicenciasService.getFacturasParaRenovacion(value.codigo, clienteCodigo)
  //     .then(response => {
  //       setFacturas(response);
  //       setTransaccion(null);
  //     })
  //   //setTransaccionCodigo(value.codigo);
  // }

  const onRenovarButtonClick = () => {
    toogleShow();
    if(renovado){
      onUpdated();
    }
    // if (itemData.ID === 1) {
    //   toogleShow();
    // } else {
    //   alert("En desarrollo!!!");
    // }

  }


  // const onRenovarClick = () => {
  //   antivirusLicenciasService.registrarRenovacion(idLicencia, transaccion.Id)
  //     .then(reponse => {
  //       notify('Renovación registrada correctamente', 'success', 2000);
  //       setRenovado(true);
  //       toogleShow();
  //       onUpdated();
  //     }, error => { notify('Error al registrar la renovacion, error ' + error, 'error', 1500) })
  // }

  const onFormaRenovarChanged=({value})=>{
    setTipoRenovacion(value);
  }

  return (
    <>
      <div onMouseEnter={toggleTooltip} onMouseLeave={toggleTooltip}>
        <Button
          id="buttonRenovarLicencia"
          text={(renovado ? "Ya Renovada" : "Renovar")}
          icon="cart"
          disabled={renovado}
          onClick={onRenovarButtonClick}
        >
        </Button>
        {/* <Button id="buttonRenovarLicencia" text="Renovar" icon='cart' type='normal' onClick={toogleShow} /> */}
        <Popover
          target="#buttonRenovarLicencia"
          position="top"
          animation={PopoverAnimationConfig}
          visible={!show && showTooltip}
          closeOnOutsideClick={false}
          width={300}
        >
          Permite registrar el reemplazo de la licencia actual con una nueva licencia.
        </Popover>
        <Popup
          title="Renovar licencia"
          showTitle={true}
          visible={show}
          onHiding={toogleShow}
          width={ tipoRenovacion === TIPO_YA_FACTURADO? 500 : 600 }
          height={ tipoRenovacion === TIPO_YA_FACTURADO? 290 : 650}
        >
          <div className="renovarLicenciaContainer">
            <FieldGroup>
              <RadioGroup items={formasRenovacion} defaultValue={formasRenovacion[0]} layout="horizontal"  onValueChanged={onFormaRenovarChanged} />
            </FieldGroup>
            {show && tipoRenovacion === TIPO_YA_FACTURADO &&
              <>
              <AntivirusLicenciasRenovarLicenciaFacturada idLicencia={idLicencia} clienteCodigo={clienteCodigo}  />
              </>
            }
            {show && tipoRenovacion === TIPO_FACTURAR &&
              <>
              <AntivirusLicenciasRenovarLicenciaFacturar idVenta={idLicencia} >

              </AntivirusLicenciasRenovarLicenciaFacturar>
              </>
            }
          </div>
        </Popup>
      </div>
    </>
  )
}