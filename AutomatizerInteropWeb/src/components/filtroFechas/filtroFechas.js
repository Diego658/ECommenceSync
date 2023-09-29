import React from 'react';
import { DateBox } from 'devextreme-react';


export default class extends React.Component {
  render() {
    return (
      <React.Fragment>
        <div className="dx-field">
          <div className="dx-field-label">Fechas a buscar</div>
          <div className="dx-field-value">
            <DateBox value={this.props.fechaInicio}
              type="datetime" />
            <DateBox value={this.props.fechaFin}
              type="datetime" />
          </div>
        </div>
      </React.Fragment>
    )
  }
}