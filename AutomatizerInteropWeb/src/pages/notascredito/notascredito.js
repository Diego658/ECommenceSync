import React from 'react';
import './notascredito.scss';
import { CustomLoadIndicator } from '../../components';
import { FiltroDocumentos } from '../../components';
import { ViewNotasCredito } from '../../components'
import { DocumentosPendientes } from '../../components'
import { config } from '../../constants/constants'
import { Popup } from 'devextreme-react/popup';
import List from 'devextreme-react/list';
import UserSesion from '../../utils/userSesion';
import { facturacionElectronicaService } from '../../_services/facturacionElectronica.service';

function ItemTemplate(data) {
  return <div>{data.registro}</div>;
}

export default class extends React.Component {

  constructor(props) {
    super(props);
    this.now = new Date();
    this.state = {
      error: null,
      notasCredito: null,
      isNotasCreditoLoaded: false,
      fechaInicio: new Date(this.now.getFullYear(), this.now.getMonth(), this.now.getDate(), 0, 0, 1),
      fechaFin: new Date(this.now.getFullYear(), this.now.getMonth(), this.now.getDate(), 23, 59, 59),
      documentLogInfo: '',
      numeroPendientes: 0
    };
    this.onConfiguracionValueChanged = this.onConfiguracionValueChanged.bind(this);
    this.onFechaFinChanged = this.onFechaFinChanged.bind(this);
    this.onFechaInicioChanged = this.onFechaInicioChanged.bind(this);
    this.onCargarClick = this.onCargarClick.bind(this);
    this.showInfo = this.showInfo.bind(this);
    this.hideInfo = this.hideInfo.bind(this);
  }


  componentDidMount() {
    UserSesion.setOnUpdate(this.onConfiguracionValueChanged);
    this.cargarNotasCredito();
  }

  componentWillUnmount() {
    UserSesion.removeOnUpdate();
  }

  onConfiguracionValueChanged() {
    this.setState(
      {
        isNotasCreditoLoaded: false
      });
    this.cargarNotasCredito();
  }


  showInfo(idDocumento) {
    fetch(config.url.API_URL + "/api/FacturacionElectronica/GetLogDocumento?idDocumento=" + idDocumento)
      .then(res => res.json())
      .then(
        (result) => {
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
      isNotasCreditoLoaded: false
    });

    this.cargarNotasCredito();
  }

  cargarNotasCredito() {
    const { fechaInicio, fechaFin } = this.state;
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    facturacionElectronicaService.getNotasCredito(configuracionId, fechaInicio, fechaFin)
      .then(
        (result) => {
          this.setState({
            isNotasCreditoLoaded: true,
            notasCredito: result.documentos,
            numeroPendientes: result.numeroPendientes
          });
        },
        (error) => {
          this.setState({
            isNotasCreditoLoaded: true,
            error,
            numeroPendientes: 0
          });
        }
      )
  }

  render() {
    const { error, isNotasCreditoLoaded, fechaInicio, fechaFin, numeroPendientes } = this.state;
    if (error) {
      return <div>Error: {error.message}</div>;
    } else if (!isNotasCreditoLoaded) {
      return <CustomLoadIndicator message={"Cargando..."} title={"Notas de Credito"} />;
    } else {
      return (
        <React.Fragment>
          <h2 className={'content-block'}>Notas de Crédito</h2>
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
                tipoDocumento={`Nota${numeroPendientes === 1 ? '' : 's'} de Crédito`}
                numeroPendientes={numeroPendientes}
              >
              </DocumentosPendientes>
              <ViewNotasCredito
                error={this.state.error}
                isConfiguracionLoaded={this.state.isConfiguracionLoaded}
                isNotasCreditoLoaded={this.state.isNotasCreditoLoaded}
                notasCredito={this.state.notasCredito}
                configuracionId={this.state.configuracionId}
                onShowInfo={this.showInfo}>
              </ViewNotasCredito>
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
            </div>
          </div>
        </React.Fragment>
      );
    }
  }
}