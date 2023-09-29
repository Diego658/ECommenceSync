import React from 'react';
import './configuracion-panel.scss';
import SelectBox from 'devextreme-react/select-box';
import ConfiguracionInfo from './configuracion-info';

export default class ConfiguracionPanel extends React.Component {
  render() {
    const { configuraciones, configuracion, onConfiguracionChanged } = this.props;
    return (
      <div className={'configuracion-panel'}>
        <div className={'configuracion-info'}>
          <SelectBox
            dataSource={configuraciones}
            placeholder="Seleccionar empresa..."
            displayExpr="nombreConfiguracion"
            itemRender={ConfiguracionInfo}
            value={configuracion}
            onValueChanged={onConfiguracionChanged}
          />
        </div>
      </div>
    );
  }
}