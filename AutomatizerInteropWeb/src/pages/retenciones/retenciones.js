import React from 'react';
import './retenciones.scss';
import { CustomLoadIndicator } from '../../components';
import { FiltroDocumentos } from '../../components';
import { ViewRetenciones } from '../../components'
import { DocumentosPendientes } from '../../components'
import { config } from '../../constants/constants'
import { Popup } from 'devextreme-react/popup';
import List from 'devextreme-react/list';
import {AlertQuestion} from '../../components';
import notify from 'devextreme/ui/notify';
import UserSesion from '../../utils/userSesion';
import {facturacionElectronicaService} from '../../_services/facturacionElectronica.service';

function ItemTemplate(data) {
  return <div>{data.registro}</div>;
}

export default class extends React.Component {

  constructor(props) {
    super(props);
    this.now = new Date();
    this.state = {
      error: null,
      retenciones: null,
      isretencionesLoaded: false,
      fechaInicio: new Date(this.now.getFullYear(),this.now.getMonth(),this.now.getDate(),0,0,1),
      fechaFin: new Date(this.now.getFullYear(),this.now.getMonth(),this.now.getDate(),23,59,59),
      documentLogInfo: '',
      retencion: null,
      numeroPendientes: 0
    };
    this.onConfiguracionValueChanged = this.onConfiguracionValueChanged.bind(this);
    this.onFechaFinChanged = this.onFechaFinChanged.bind(this);
    this.onFechaInicioChanged = this.onFechaInicioChanged.bind(this);
    this.onCargarClick = this.onCargarClick.bind(this);
    this.showInfo = this.showInfo.bind(this);
    this.hideInfo = this.hideInfo.bind(this);
    this.iniciarProcesoRetencion = this.iniciarProcesoRetencion.bind(this);
    this.cancelarProcesoRetencion = this.cancelarProcesoRetencion.bind(this);
  }


  

  componentDidMount(){
    // this.setState({
    //   fechaInicio: new Date(this.now.getFullYear(),this.now.getMonth(),this.now.getDate()),
    //   fechaFin: new Date(this.now.getFullYear(),this.now.getMonth(),this.now.getDate(),23,59,59)
    // });
    UserSesion.setOnUpdate(this.onConfiguracionValueChanged);
    this.cargarRetenciones();
  }


  componentWillUnmount(){
    UserSesion.removeOnUpdate();
  }

  onConfiguracionValueChanged() {
    this.setState(
      { 
        isretencionesLoaded: false
    });
    this.cargarRetenciones();
  }

  showInfo(idDocumento) {
    fetch(config.url.API_URL + "/api/FacturacionElectronica/GetLogDocumento?idDocumento="+idDocumento)
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
            documentLogInfo:error
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

  iniciarProcesoRetencion(retencion){
    if (this.state.retencion==null) {
      this.setState({retencion: retencion});  
    }
    else
    {
      const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
      facturacionElectronicaService.iniciarProcesoRetencion(configuracionId, this.state.retencion)
      .then((data)=>{
          this.setState({
            retencion: null,
            isretencionesLoaded:false
          });
        notify("Documento puesto en cola.", 'info', 2500);
        this.cargarRetenciones();
      }, error=>{

        this.setState({
          retencion: null
        });
        notify("Error al agregar retención \nMensaje:"+error, 'error', 2500);

      });
    }
  }

  cancelarProcesoRetencion(retencion){
    this.setState({retencion: null});
  }


  

  onFechaInicioChanged(e){
    this.setState(
      { 
        fechaInicio: e.value
      });
  }

  onFechaFinChanged(e){
    this.setState(
      { 
        fechaFin: e.value
    });
  }

  onCargarClick(e){
    this.setState({
      isretencionesLoaded: false
    });
    
    this.cargarRetenciones();
  }

  cargarRetenciones(){
    const {  fechaInicio, fechaFin } = this.state;
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    facturacionElectronicaService.getRetenciones(configuracionId, fechaInicio, fechaFin)
      .then(
        (result) => {
          this.setState({
            isretencionesLoaded: true,
            retenciones: result.documentos,
            numeroPendientes: result.numeroPendientes
          });
        },
        (error) => {
          this.setState({
            isretencionesLoaded: true,
            error,
            numeroPendientes: 0
          });
        }
      )
  }

  

  render(){
    const { error,  fechaInicio, fechaFin, numeroPendientes, isretencionesLoaded } = this.state;
    if (error) {
      return <div>Error: {error.message}</div>;
    } else if (!isretencionesLoaded) {
      return  <CustomLoadIndicator message={"Cargando..."} title={"Retenciones"} />;
    } else {
      return (
      <React.Fragment>
        <h2 className={'content-block'}>Retenciones</h2>
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
              tipoDocumento={`Retenci${numeroPendientes===1?'ón':'ones'}`}
              numeroPendientes={numeroPendientes}
            ></DocumentosPendientes>
            <ViewRetenciones
                error={this.state.error}
                isConfiguracionLoaded={this.state.isConfiguracionLoaded}
                isretencionesLoaded={this.state.isretencionesLoaded}
                retenciones={this.state.retenciones}
                configuracionId={this.state.configuracionId}
                onShowInfo={this.showInfo}
                iniciarProcesoRetencion={(ret)=>this.iniciarProcesoRetencion(ret)}
                >
            </ViewRetenciones>
            <Popup
              visible={this.state.popupVisible}
              onHiding={this.hideInfo}
              dragEnabled={false}
              closeOnOutsideClick={true}
              showTitle={true}
              title="Registro de Operaciones"
              width={800}
              height={600}            >
              <List
                dataSource={this.state.documentLogInfo}
                height={400}
                itemRender={ItemTemplate}
                searchExpr="Name"
                searchEnabled={true}
                searchMode='contains' />
            </Popup>
            <AlertQuestion
              data={this.state.retencion}
              confirmBtnText="Sí, Agregar!"
              confirmBtnText2="Agregando..."
              title="Agregar Retención"
              question="¿Iniciar proceso de retención?"
              onConfirm={this.iniciarProcesoRetencion}
              onCancel={this.cancelarProcesoRetencion}
              message="Una vez se inicie el proceso ya no se pueden cambiar los datos de la retención.">
            </AlertQuestion>
          </div>
        </div>
      </React.Fragment>        
      );
    }
  }
}