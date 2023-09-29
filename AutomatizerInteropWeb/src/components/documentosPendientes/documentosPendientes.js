import React from 'react';
import './documentosPendientes.scss'

export default class extends React.Component {
  render() {
    if (this.props.numeroPendientes === 0) {
      return null;
    }
    return (
      <React.Fragment>
        <div className="documentosPendientes">
          <div className="documentosPendientes-text" >
            <i className="dx-icon-warning"></i>
            <span >{`Tiene ${this.props.numeroPendientes} ${this.props.tipoDocumento} pendiente${this.props.numeroPendientes === 1 ? '' : 's'}`}</span>
          </div>
        </div>
      </React.Fragment>
    );
  }
}