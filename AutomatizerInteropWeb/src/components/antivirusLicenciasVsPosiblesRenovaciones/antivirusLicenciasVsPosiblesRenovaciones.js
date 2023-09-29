import React from 'react';

import Chart, {
    Legend,
    Series,
    Export,
    Title,
    Tooltip,
    CommonSeriesSettings
  } from 'devextreme-react/chart';
  //import { config } from '../../constants/constants'
  import UserSesion from '../../utils/userSesion';
  import DataSource from 'devextreme/data/data_source';
  import { CustomLoadIndicator } from '../../components';
  import {antivirusLicenciasService} from '../../_services/antivirusLicencias.service'

  export default class  extends React.Component {

    customizeTooltip(arg) {
        return {
          text: `${arg.percentText} cantidad: ${arg.valueText}`
        };
      }

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
        antivirusLicenciasService.getExistenciasVsRenovaciones(configuracionId)
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
                    id="chartLicenciasVsRenovaciones"
                    dataSource={new DataSource({
                    store: {
                        type: 'array',
                        mode: 'raw',
                        data: estado
                    },
                    })}
                >
                    <Title
                        text="Licencias VS Renovaciones"
                        subtitle="(Existencias vs Licencias por renovar)"
                    />
                    <CommonSeriesSettings argumentField="nombre" type="fullstackedbar" />
                    <Series valueField="existencia" name="Existencia" />
                    <Series valueField="pendienteRenovar" name="Pendiente Renovar" />
                    <Legend verticalAlignment="top"
                        horizontalAlignment="center"
                        itemTextPosition="right"
                    />
                    <Export enabled={false} />
                    <Tooltip
                        enabled={true}
                        customizeTooltip={this.customizeTooltip}
                    />
                </Chart>
            </React.Fragment>
        );
    }
}
