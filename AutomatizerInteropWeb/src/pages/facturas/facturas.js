import React from 'react';
import './facturas.scss';
import { FiltroDocumentos } from '../../components';
import { ViewFacturas } from '../../components'
//import { FiltroFechas } from '../../components'
import 'devextreme/data/odata/store';
import { config } from '../../constants/constants';
import { CustomLoadIndicator } from '../../components';
import { Popup } from 'devextreme-react/popup';
import List from 'devextreme-react/list';
import { DocumentosPendientes } from '../../components';
import UserSesion from '../../utils/userSesion';
import { facturacionElectronicaService } from '../../_services/facturacionElectronica.service';
import FacturacionElectronicaReenviarEmail from '../../components/facturas/reenviarCorreoDocumento';

function ItemTemplate(data) {
  return <div>{data.registro}</div>;
}


export default class extends React.Component {
  constructor(props) {
    super(props);
    this.now = new Date();
    this.state = {
      error: null,
      facturas: null,
      isFacturasLoaded: false,
      fechaInicio: new Date(this.now.getFullYear(), this.now.getMonth(), this.now.getDate(), 0, 0, 1),
      fechaFin: new Date(this.now.getFullYear(), this.now.getMonth(), this.now.getDate(), 23, 59, 59),
      documentLogInfo: '',
      numeroPendientes: 0,
      idDocumento: 0,
      showReenviar: false
    };
    this.onConfiguracionValueChanged = this.onConfiguracionValueChanged.bind(this);
    this.onFechaFinChanged = this.onFechaFinChanged.bind(this);
    this.onFechaInicioChanged = this.onFechaInicioChanged.bind(this);
    this.onCargarClick = this.onCargarClick.bind(this);
    this.showInfo = this.showInfo.bind(this);
    this.hideInfo = this.hideInfo.bind(this);
    this.reenviarEmail = this.reenviarEmail.bind(this);
  }

  reenviarEmail(idDocumento) {
    this.setState({
      idDocumento: idDocumento,
      showReenviar: true
    });
  }


  showInfo(idDocumento) {
    var info = '';
    fetch(config.url.API_URL + "/api/FacturacionElectronica/GetLogDocumento?idDocumento=" + idDocumento)
      .then(res => res.json())
      .then(
        (result) => {

          result.forEach(l => {
            info = info + `FechaInicio: ${l.fechaInicio} \nFecha Fin: ${l.fechaFin} \nRegistro: ${l.registro} \n${"-".repeat(100)} \n`;
          });

          this.setState({
            popupVisible: true,
            documentLogInfo: result
          });
        },

        (error) => {
          this.setState({
            popupVisible: true,
            documentLogInfo: error
          });
        }
      )


  }



  hideInfo() {

    this.setState({
      currentDocument: 0,
      popupVisible: false
    });
  }

  componentDidMount() {
    UserSesion.setOnUpdate(this.onConfiguracionValueChanged);
    this.cargarFacturas();
  }

  componentWillUnmount() {
    UserSesion.removeOnUpdate();
  }

  onConfiguracionValueChanged() {
    this.setState(
      {
        isFacturasLoaded: false
      });
    this.cargarFacturas();
  }

  onFechaInicioChanged(e) {
    this.setState(
      {
        fechaInicio: e.value
      });
  }

  onFechaFinChanged(e) {
    this.setState(
      {
        fechaFin: e.value
      });
  }

  onCargarClick(e) {
    this.setState({
      isFacturasLoaded: false
    });

    this.cargarFacturas();
  }


  cargarFacturas() {
    const { fechaInicio, fechaFin } = this.state;
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    facturacionElectronicaService.getFacturas(configuracionId, fechaInicio, fechaFin)
      .then(
        (result) => {
          this.setState({
            isFacturasLoaded: true,
            facturas: result.documentos,
            numeroPendientes: result.numeroPendientes
          });
        },
        (error) => {
          this.setState({
            isFacturasLoaded: true,
            error,
            numeroPendientes: 0
          });
        }
      )
  }


  render() {
    const { error, isFacturasLoaded, fechaInicio, fechaFin, numeroPendientes } = this.state;
    if (error) {
      return <div>Error: {error.message}</div>;
    } else if (!isFacturasLoaded) {
      return <CustomLoadIndicator message={"Cargando..."} title={"Facturas"} />;
    } else {
      return (
        <React.Fragment>
          <h2 className={'content-block'}>Facturas</h2>
          <div className={'content-block'}>
            <div className={'dx-card responsive-paddings'}>
              <FiltroDocumentos
                fechaInicio={fechaInicio}
                onFechaInicioChanged={this.onFechaInicioChanged}
                fechaFin={fechaFin}
                onFechaFinChanged={this.onFechaFinChanged}
                onCargarClick={this.onCargarClick}
              >
              </FiltroDocumentos>
              <DocumentosPendientes
                tipoDocumento={`Factura${numeroPendientes === 1 ? '' : 's'}`}
                numeroPendientes={numeroPendientes}
              ></DocumentosPendientes>
              <ViewFacturas
                error={this.state.error}
                isConfiguracionLoaded={this.state.isConfiguracionLoaded}
                isFacturasLoaded={this.state.isFacturasLoaded}
                facturas={this.state.facturas}
                configuracionId={this.state.configuracionId}
                onShowInfo={this.showInfo}
                onReenviarMail={this.reenviarEmail}>
              </ViewFacturas>
              <Popup
                visible={this.state.popupVisible}
                onHiding={this.hideInfo}
                dragEnabled={false}
                closeOnOutsideClick={true}
                showTitle={true}
                title="Registro de Operaciones"
                width={800}
                height={600}
              >
                <List
                  dataSource={this.state.documentLogInfo}
                  height={400}
                  itemRender={ItemTemplate}
                  searchExpr="Name"
                  searchEnabled={true}
                  searchMode='contains' />
              </Popup>
              <Popup
                title='Reenviar Documento'
                visible={this.state.showReenviar}
                onHiding={() => this.setState({ showReenviar: false })}
                width={400}
                height={200}
              >
                <div id={`reenviarEmail`}>
                  {this.state.showReenviar &&
                    <FacturacionElectronicaReenviarEmail idDocumento={this.state.idDocumento} />
                  }
                </div>
              </Popup>
            </div>
          </div>
        </React.Fragment>
      );
    }
  }

}