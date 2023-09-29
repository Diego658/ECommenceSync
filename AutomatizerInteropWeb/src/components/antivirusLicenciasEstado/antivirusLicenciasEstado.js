import React from 'react';
import './antivirusLicenciasEstado.scss'
import Chart, {
    ArgumentAxis,
    Legend,
    Series,
    ValueAxis,
    Label,
    Export,
    Tick,
    Title,
  } from 'devextreme-react/chart';
  //import { config } from '../../constants/constants'
  import UserSesion from '../../utils/userSesion';
  import DataSource from 'devextreme/data/data_source';
  import { CustomLoadIndicator } from '../../components';
  import {antivirusLicenciasService} from '../../_services/antivirusLicencias.service'

  export default class  extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            error: null,
            estado: null,
            isLoaded: false
        };
    }


    componentDidMount(){
        this.cargarDatos();
    }

    
    actualizar(){
        this.setState({
            isLoaded:false,
            estado:null
        });
        this.cargarDatos();
    }


    cargarDatos(){
        const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
        antivirusLicenciasService.getEstadoLicencias(configuracionId)
          .then(
            (result) => {
              this.setState({
                isLoaded: true,
                estado: result
              });
            },
           
            (error) => {
              this.setState({
                isLoaded: true,
                error
              });
            }
          )
    }

    render(){
        const configuracionId = this.props.configuracionId;
        var {estado, isLoaded, error} = this.state;
        
        if(configuracionId === 0 || !isLoaded){
            return  <CustomLoadIndicator message={"Cargando..."} title={"Estado licencias antivirus"} />;
        }
        if(error){
            return <div>Error: {error.message}</div>;
        }
        return(
            <React.Fragment>
                <Chart
                    rotated={true}
                    id="chartEstadoLicenciasAntivirus"
                    dataSource={new DataSource({
                    store: {
                        type: 'array',
                        mode: 'raw',
                        data: estado.estados
                    },
                    })}
                >
                    <Title
                        text="Estado licencias antivirus"
                        subtitle="(Licencias por renovar)"
                    />
                    <ArgumentAxis>
                        <Label  />
                    </ArgumentAxis>
                    <ValueAxis>
                        <Tick visible={false} />
                        <Label visible={false} />
                    </ValueAxis>
                    <Series
                        valueField="cantidad"
                        argumentField="grupo"
                        type="bar"
                        color="#79cac4"
                    >
                        <Label visible={true} backgroundColor="#c18e92" />
                    </Series>
                    <Legend visible={false} />
                    <Export enabled={false} />
                </Chart>
            </React.Fragment>
        );
    }
}
