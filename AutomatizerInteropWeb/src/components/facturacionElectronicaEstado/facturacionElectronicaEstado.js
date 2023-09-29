import React from 'react';
import './facturacionElectronicaEstado.scss';
//import { List } from 'devextreme-react';
//import EstadoInfo from './estadoInfo';
//import { config } from '../../constants/constants'
import UserSesion from '../../utils/userSesion';
//import ConfiguracionInfo from '../configuracion-panel/configuracion-info';
import { CustomLoadIndicator } from '../../components';
import { facturacionElectronicaService } from '../../_services/facturacionElectronica.service'
export default class extends React.Component {

  constructor(props) {
    super(props);
    this.state = {
      error: null,
      isLoaded: false,
      estado: null
    };

  }

  actualizar() {
    this.setState({
      isLoaded: false,
      estado: null
    });
    this.cargarDatos();
  }

  componentDidMount() {
    this.cargarDatos();
  }

  cargarDatos() {
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    facturacionElectronicaService.GetNumeroPendientes(configuracionId)
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

  render() {

    var { estado, isLoaded, error } = this.state;

    if (!isLoaded) {
      return <CustomLoadIndicator message={"Cargando..."} title={"Estado facturacion electronica"} />;
    }
    if (error) {
      return <div>Error: {error.message}</div>;
    }
    return (
      <React.Fragment>
        <div className="configuracionName">{estado.configuracionName}</div>
        <table className="tablaDocumentos">
          <thead>
            <tr>
              <th>Tipo Documento</th>
              <th className="header-valor">Pendientes</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td>Facturas</td>
              <td className="number">{estado.factura}</td>
            </tr>
            <tr>
              <td>Notas de Crédito</td>
              <td className="number">{estado.notaCredito}</td>
            </tr>
            <tr>
              <td>Notas de Débito</td>
              <td className="number">{0}</td>
            </tr>
            <tr>
              <td>Retenciones</td>
              <td className="number">{estado.retencion}</td>
            </tr>
          </tbody>
        </table>
      </React.Fragment>
    );
  }
}
