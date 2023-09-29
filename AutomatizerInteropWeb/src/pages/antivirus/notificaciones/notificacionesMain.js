import React from 'react';
import Notificaciones from './notificaciones'
import InformacionVentaAntivirus from './informacionVentaAntivirus'
import { Popup } from 'devextreme-react/popup';

export default class extends React.Component {


  constructor(props) {
    super(props);
    this.state = {
      ventaSeleccionada: {},
      popupVisible: false
    };
    this.mostrarVenta = this.mostrarVenta.bind(this);
    this.ocultarVenta = this.ocultarVenta.bind(this);
  }



  mostrarVenta(venta) {
    this.setState({
      ventaSeleccionada: venta,
      popupVisible: true
    });
  }

  ocultarVenta() {
    this.setState({
      ventaSeleccionada: {},
      popupVisible: false
    });
  }

  render() {
    const { ventaSeleccionada } = this.state;
    return (
      <React.Fragment>
        <h2 className={'content-block'}>Antivirus ventas</h2>
        <div className={'content-block'}>
          <div className={'dx-card responsive-paddings'}>
            <Notificaciones mostrarVenta={this.mostrarVenta}></Notificaciones>
            <Popup
              visible={this.state.popupVisible}
              onHiding={this.ocultarVenta}
              dragEnabled={false}
              closeOnOutsideClick={true}
              showTitle={true}
              title={this.state.ventaSeleccionada.producto}
              width={800}
              height={600}
            >
              <InformacionVentaAntivirus venta={ventaSeleccionada} />
            </Popup>
          </div>

        </div>
      </React.Fragment>
    );
  }


}