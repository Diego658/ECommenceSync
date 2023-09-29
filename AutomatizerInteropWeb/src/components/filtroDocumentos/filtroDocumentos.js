import React from 'react';
import './filtroDocumentos.scss';
import Box, {
  Item
} from 'devextreme-react/box';
import { DateBox } from 'devextreme-react';
import { Button } from 'devextreme-react';

export default class extends React.Component {

  render() {
    return <Box
      direction="row"
      width="100%"
      height={75}>
      <Item ratio={2}>
        <div className="dx-field">
          <div className="dx-field-label">Inicio</div>
          <div className="dx-field-value">
            <DateBox defaultValue={this.props.fechaInicio}
              type="datetime"
              useMaskBehavior={true}
              displayFormat="dd/MM/yyyy HH:mm:ss"
              onValueChanged={this.props.onFechaInicioChanged} />
          </div>
        </div>
      </Item>
      <Item ratio={2}>
        <div className="dx-field">
          <div className="dx-field-label">Fin</div>
          <div className="dx-field-value">
            <DateBox defaultValue={this.props.fechaFin}
              type="datetime"
              useMaskBehavior={true}
              displayFormat="dd/MM/yyyy HH:mm:ss"
              onValueChanged={this.props.onFechaFinChanged} />
          </div>
        </div>
      </Item>
      <Item ratio={1}>
        <div className="dx-field">
          <div className="dx-field-label"></div>
          <div className="dx-field-value">
            <Button
              text="Cargar"
              type="normal"
              stylingMode="contained"
              disabled={this.props.configuracionId === 0}
              onClick={this.props.onCargarClick}
            />
          </div>

        </div>

      </Item>
    </Box>
  }
}