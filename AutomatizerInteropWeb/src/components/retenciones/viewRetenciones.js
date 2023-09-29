import React from 'react';
import './viewRetenciones.scss'
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
//import { DownloadHelper } from '../../utils/downloadHelper'

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
    this.isIniciarProcesoIconVisible = this.isIniciarProcesoIconVisible.bind(this);
    this.iniciarProcesoIconClick = this.iniciarProcesoIconClick.bind(this);
  }


  isVerRegistroIconVisible(e) {
    return true;
  }

  verRegistroIconClick(e) {

    //console.log("Ver Registro");
    this.props.onShowInfo(e.row.data["idDocumento"]);
    e.event.preventDefault();
  }


  isIniciarProcesoIconVisible(e) {
    return e.row.data.estado === "NoMigrado";
  }

  iniciarProcesoIconClick(e) {
    this.props.iniciarProcesoRetencion(e.row.data);
    e.event.preventDefault();
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
    e.event.preventDefault();
  }


  isMailIconVisible(e) {
    return e.row.data.estado === "Autorizado";
  }

  mailIconClick(e) {
    e.event.preventDefault();
  }

  isPdfIconVisible(e) {
    return e.row.data.estado === "Autorizado";
  }

  pdfIconClick(e) {
    facturacionElectronicaService.descargarDocumentoPdf(e.row.data["idDocumento"], `Retencion-${e.row.data["serieDocumento"]}-${e.row.data["numero"]}.pdf`);
    //this.download(e.row.data["idDocumento"], `Retencion - ${e.row.data["serieDocumento"]}-${e.row.data["numero"]}.pdf`,'GetPdf' );
    e.event.preventDefault();
  }


  isXmlIconVisible(e) {
    return e.row.data.estado === "Autorizado";
  }

  xmlIconClick(e) {
    facturacionElectronicaService.descargarDocumentoXml(e.row.data["idDocumento"], `Retencion-${e.row.data["serieDocumento"]}-${e.row.data["numero"]}.xml`);
    //this.download(e.row.data["idDocumento"], `Retencion - ${e.row.data["serieDocumento"]}-${e.row.data["numero"]}.xml`,'GetXml' );
    e.event.preventDefault();
  }

  render() {
    if (!this.props.isretencionesLoaded) {
      return <LoadIndicator id="large-indicator" height={60} width={60} />;
    }
    else {
      return (
        <DataGrid
          id="gridContainer"
          dataSource={new DataSource({
            store: {
              key: 'idDocumento',
              type: 'array',
              mode: 'raw',
              data: this.props.retenciones
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
          <Scrolling mode="virtual" rowRenderingMode="virtual" />
          <FilterRow visible={true} />
          {/* <Column caption="#" cellRender={GridNumerador} width={50} ></Column> */}
          <Column dataField={'idDocumento'} visible={false} />
          <Column dataField={'serie'} visible={false} />
          <Column dataField={'fecha'} dataType="date" format="dd-MM-yyyy" caption={'Fecha'} width={140} />
          <Column dataField={'proveedor'} caption={'Proveedor'} />
          <Column dataField={'transaccionCompra'} caption={'Transacción'} />
          <Column dataField={'numeroCompra'} caption="# Compra" width={110} alignment={'right'} />
          <Column dataField={'numeroFacturaCompra'} caption="# Factura" width={160} alignment={'right'} />
          <Column dataField={'numero'} caption="# Retención" width={160} alignment={'right'} />
          <Column dataField={'estado'} width={120} />
          <Column type="buttons" width={150}
            buttons={[{
              hint: 'Iniciar Proceso',
              icon: 'plus',
              visible: this.isIniciarProcesoIconVisible,
              onClick: this.iniciarProcesoIconClick
            }, {
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
      );
    }
  }
}

