import React, { useState } from 'react';
import {  Popup } from 'devextreme-react';
import PedidoPrestashop from './pedido';
import { DropDownButton } from 'devextreme-react';
import PedidoGenerarEtiquetas from './pedido-genera-etiquetas';
import PedidoRegistrarTracking from './pedido-registrar-tracking';
import notify from 'devextreme/ui/notify';
import GuiasGuiaTransporte from '../../../guias-transporte/guias-guiatransporte';
import { PrestashopService } from '../../../../_services/stores/prestashop-service';

export default function CellVerPedidoPrestashop({ idPedido, estado }) {
  const [show, setShow] = useState(false);
  const [showPrintLabels, setShowPrintLabels] = useState(false);
  const [showRegistrarGuia, setShowRegistrarGuia] = useState(false);
  const [showGuia, setshowGuia] = useState(false);
  const [idGuia, setIdGuia] = useState(0);

  const toogleShow = () => {
    setShow(!show);
  }

  const toogleShowPrintLabels = () => {
    if (estado !== 3) {
      notify('Solo se puede generar etiquetas en pedidos en preparación', 'error', 2500);
    }
    else {
      setShowPrintLabels(!showPrintLabels);
    }

  }


  const toogleShowRegistrarGuias = async () => {
    if (estado === 3) {
      setShowRegistrarGuia(!showRegistrarGuia);
      
    }
    else if(estado ===4 || estado ===5){
      const guia = await PrestashopService.getGuiaTransporteOrden(idPedido);
      setIdGuia(guia.id);
      setshowGuia(!showGuia);
    }
    else {
      notify('Solo se puede establecer / ver el número de seguimiento en pedidos en preparación, enviados o entregados', 'error', 2500);
    }

  }

  const onMenuItemClick = async (e) => {
    const buttonId = e.itemData.id;
    if (buttonId === 1) {
      toogleShowPrintLabels();
    }
    else if (buttonId === 4) {
      await toogleShowRegistrarGuias();
    }

  }


  const buttonSettings = [
    { id: 1, name: 'Etiquetas', icon: 'background' },
    { id: 4, name: 'Guía', icon: 'variable', }
  ];

  return (
    <>

      <DropDownButton
        splitButton={true}
        useSelectMode={false}
        text="Ver"
        icon="arrowright"
        items={buttonSettings}
        displayExpr="name"
        keyExpr="id"
        onButtonClick={toogleShow}
        onItemClick={onMenuItemClick}
      />


      <Popup
        title="Información Orden"
        showTitle={false}
        visible={show}
        onHiding={toogleShow}
        width='100%'
        height='100%'
      >
        <div className='popUpPedidoContainer' >
          {show &&
            <PedidoPrestashop id={idPedido} toogleZoom={toogleShow} />
          }
        </div>
      </Popup>
      <Popup
        title='Etiquetas'
        showTitle={true}
        visible={showPrintLabels}
        onHiding={toogleShowPrintLabels}
        closeOnOutsideClick={true}
        width='180px'
        height='200px'
      >
        <div id={'generarEtiquetas' + idPedido}>
          {showPrintLabels &&
            < PedidoGenerarEtiquetas idPedido={idPedido} />
          }
        </div>

      </Popup>
      <Popup
        title='Registrar Guía'
        showTitle={true}
        visible={showRegistrarGuia}
        onHiding={toogleShowRegistrarGuias}
        closeOnOutsideClick={true}
        width='900px'
        height='400px'
      >
        <div id={'registrarGuia' + idPedido}>
          {showRegistrarGuia &&
            <PedidoRegistrarTracking idPedido={idPedido} ></PedidoRegistrarTracking>
          }
        </div>
      </Popup>
      <Popup
        title='Ver Guía'
        showTitle={true}
        visible={showGuia && idGuia>0}
        onHiding={toogleShowRegistrarGuias}
        closeOnOutsideClick={true}
        width='950px'
        height='450px'
      >
        <div id={'registrarGuia' + idPedido}>
          {showGuia && idGuia>0 &&
            <GuiasGuiaTransporte id={idGuia} />
          }
        </div>
      </Popup>
    </>
  )
}