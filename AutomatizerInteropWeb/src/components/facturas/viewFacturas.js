import React from 'react';
import './viewFacturas.scss'
import DataGrid, {
  Column,
  Scrolling,
  FilterRow
} from 'devextreme-react/data-grid';
import DataSource from 'devextreme/data/data_source';
//import GridNumerador from '../gridNumerador/gridNumerador'
import { LoadIndicator } from 'devextreme-react/load-indicator';
//import { config } from '../../constants/constants';
import 'devexpress-reporting/dx-webdocumentviewer';
import notify from 'devextreme/ui/notify';
import { facturacionElectronicaService } from '../../_services/facturacionElectronica.service';
import { confirm } from 'devextreme/ui/dialog';
import UserSesion from '../../utils/userSesion';
import { Popup } from 'devextreme-react';
import FacturacionElectronicaReenviarEmail from './reenviarCorreoDocumento';

export default class extends React.Component {

  constructor(props) {
    super(props);
    this.isMailIconVisible = this.isMailIconVisible.bind(this);
    this.mailIconClick = this.mailIconClick.bind(this);
    this.isPdfIconVisible = this.isPdfIconVisible.bind(this);
    this.pdfIconClick = this.pdfIconClick.bind(this);
    this.isReiniciarProcesoIconVisible = this.isReiniciarProcesoIconVisible.bind(this);
    this.reiniciarProcesoIconClick = this.reiniciarProcesoIconClick.bind(this);
    this.isVerRegistroIconVisible = this.isVerRegistroIconVisible.bind(this);
    this.verRegistroIconClick = this.verRegistroIconClick.bind(this);
    this.isXmlIconVisible = this.isXmlIconVisible.bind(this);
    this.xmlIconClick = this.xmlIconClick.bind(this);
  }




  isReiniciarProcesoIconVisible(e) {
    const estadosReiniciables = [0, 2, 4, 70];
    var b = estadosReiniciables.indexOf(e.row.data.estadoSri);
    return b > 0;
  }

  reiniciarProcesoIconClick(e) {
    facturacionElectronicaService.reintentarDocumento(e.row.data.idDocumento)
      .then((data) => {
        notify("Documento puesto en cola para reintentarse, actualice la busqueda", 'info', 2500);
      },
        (error) => {
          this.setState({ retencion: null });
          notify("Error al reinciar documento \nMensaje:" + error, 'error', 2500);
        });

    //console.log("Reiniciar Proceso");
    e.event.preventDefault();
  }


  isMailIconVisible(e) {
    return e.row.data.estado === "Autorizado";
  }

  async mailIconClick(e) {
    console.log("Enviar mail");
    const result = await confirm("<i>Desea reenviar al mail del cliente?</i>", "Ocultar");
    if (result) {
      await facturacionElectronicaService.reenviarEmail(UserSesion.getConfiguracion().idConfiguracionPrograma, e.row.data["idDocumento"], '');
      notify('El documento se puso en cola para reintentarse.', 'success', 2000);

    } else {
      this.props.onReenviarMail(e.row.data["idDocumento"]);
      //this.setState({ showReenviar: true, idDocumento:e.row.data["idDocumento"] });
    }
    e.event.preventDefault();
  }


  isVerRegistroIconVisible(e) {
    return e.row.data.estado === "Autorizado";
  }

  verRegistroIconClick(e) {

    //console.log("Ver Registro");
    this.props.onShowInfo(e.row.data["idDocumento"]);
    e.event.preventDefault();
  }


  isPdfIconVisible(e) {
    return e.row.data.estado === "Autorizado";
  }

  pdfIconClick(e) {
    facturacionElectronicaService.descargarDocumentoPdf(e.row.data["idDocumento"], `Factura-${e.row.data["serieDocumento"]}-${e.row.data["numero"]}.pdf`);
    //this.download(e.row.data["idDocumento"], `Factura - ${e.row.data["serieDocumento"]}-${e.row.data["numero"]}.pdf`,'GetPdf' );
    e.event.preventDefault();
  }


  isXmlIconVisible(e) {
    return e.row.data.estado === "Autorizado";
  }

  xmlIconClick(e) {
    facturacionElectronicaService.descargarDocumentoXml(e.row.data["idDocumento"], `Factura-${e.row.data["serieDocumento"]}-${e.row.data["numero"]}.xml`);
    //this.download(e.row.data["idDocumento"], `Nota Credito-${e.row.data["serieDocumento"]}-${e.row.data["numero"]}.xml`,'GetXml' );
    e.event.preventDefault();
  }



  render() {
    if (this.props.error) {
      return <div>Error: {this.props.error.message}</div>;
    }
    else if (!this.props.isFacturasLoaded) {
      return <LoadIndicator id="large-indicator" height={60} width={60} />;
    }
    else {
      return (
        <>

          <DataGrid
            id="gridContainer"
            dataSource={new DataSource({
              store: {
                key: 'idDocumento',
                type: 'array',
                mode: 'raw',
                data: this.props.facturas
              },
              sort: { getter: 'idDocumento', desc: false }
            })}
            showBorders={true}
            focusedRowEnabled={true}
            defaultFocusedRowIndex={0}
            columnAutoWidth={true}
            columnHidingEnabled={true}
            keyExpr={'idDocumento'}
            height={600}
            className={"grid-block"}
          >
            <Scrolling mode="virtual" />
            <FilterRow visible={true} />
            {/* <Column caption="#" cellRender={GridNumerador} width={50} ></Column> */}
            <Column dataField={'idDocumento'} visible={false} />
            <Column dataField={'serieDocumento'} visible={false} />
            <Column dataField={'fecha'} dataType="date" format="dd-MM-yyyy" caption={'Fecha'} width={140} />
            <Column dataField={'cliente'} caption={'Cliente'} />
            <Column dataField={'transaccion'} caption={'Transacción'} />
            <Column dataField={'numero'} width={90} alignment={'right'} />
            <Column dataField={'estado'} width={120} />
            <Column type="buttons" width={150}
              buttons={[{
                hint: 'Reiniciar Proceso',
                icon: 'pulldown',
                visible: this.isReiniciarProcesoIconVisible,
                onClick: this.reiniciarProcesoIconClick
              },
              {
                hint: 'Reenviar Correo',
                icon: 'message',
                visible: this.isMailIconVisible,
                onClick: this.mailIconClick
              },
              {
                hint: 'Ver Pdf',
                icon: 'pdffile',
                visible: this.isPdfIconVisible,
                onClick: this.pdfIconClick
              },
              {
                hint: 'Ver Xml',
                icon: 'variable',
                visible: this.isXmlIconVisible,
                onClick: this.xmlIconClick
              },
              {
                hint: 'Ver Registro',
                icon: 'info',
                visible: this.isVerRegistroIconVisible,
                onClick: this.verRegistroIconClick
              }

              ]} />
          </DataGrid>
        </>
      );
    }
  }
}

