import React from 'react';
import './home.scss';

import ResponsiveBox, {
  Row,
  Col,
  Item,
  Location
} from 'devextreme-react/responsive-box';
import { FacturacionElectronicaEstado } from '../../components'
import { AntivirusLicenciasEstado } from '../../components'
import { AntivirusLicenciasVsPosiblesRenovaciones } from '../../components'
import UserSesion from '../../utils/userSesion';


function screen(width) {
  return (width < 700) ? 'sm' : 'lg';
}

export default class extends React.Component {

  estadoLicenciasAntivirusRef = React.createRef();
  estadoFacturacionElectronicaRef = React.createRef();
  antivirusLicenciasVsPosiblesRenovacionesRef = React.createRef();

  constructor(props) {
    super(props);
    this.onConfiguracionValueChanged = this.onConfiguracionValueChanged.bind(this);
  }


  componentDidMount() {
    UserSesion.setOnUpdate(this.onConfiguracionValueChanged);
    this.onConfiguracionValueChanged();
  }

  componentWillUnmount() {
    UserSesion.removeOnUpdate();
  }

  onConfiguracionValueChanged() {
    if (this.estadoLicenciasAntivirusRef.current != null) {
      this.estadoLicenciasAntivirusRef.current.actualizar();
    }

    if (this.estadoFacturacionElectronicaRef.current != null) {
      this.estadoFacturacionElectronicaRef.current.actualizar();
    }

    if (this.antivirusLicenciasVsPosiblesRenovacionesRef.current != null) {
      this.antivirusLicenciasVsPosiblesRenovacionesRef.current.actualizar();
    }
  }



  render() {
    return (
      <React.Fragment>
        <h2 className={'content-block'}>Estado</h2>
        <div className={'content-block'}>
          <div className={'dx-card responsive-paddings'}>
            <ResponsiveBox
              singleColumnScreen="sm"
              screenByWidth={screen}
            >
              <Row ratio={1}></Row>
              <Row ratio={2} screen="xs"></Row>
              <Row ratio={2}></Row>
              <Row ratio={1}></Row>

              <Col ratio={1}></Col>
              <Col ratio={2} screen="lg"></Col>
              <Col ratio={2} screen="lg"></Col>
              <Col ratio={1}></Col>


              <Item className="homeWidget">
                <Location
                  row={0}
                  col={0}
                  colspan={1}
                  screen="lg"
                >
                </Location>
                <Location
                  row={1}
                  col={0}
                  colspan={2}
                  screen="sm"
                >
                </Location>
                <div >
                  <div className="titleWidget" >Estado Facturación Electronica</div>
                  <FacturacionElectronicaEstado ref={this.estadoFacturacionElectronicaRef} />
                </div>

              </Item>

              <Item className="homeWidget">
                <Location
                  row={1}
                  col={2}
                  colspan={2}
                  screen="lg"
                >
                </Location>
                <Location
                  row={0}
                  col={0}
                  colspan={4}
                  screen="sm"
                >
                </Location>
                <AntivirusLicenciasEstado ref={this.estadoLicenciasAntivirusRef} />
                {/* <div>
                <div className="titleWidget" >Estado licencias antivirus</div>
                
              </div> */}

              </Item>

              <Item>
                <Location
                  row={1}
                  col={0}
                  colspan={2}
                  screen="lg"
                >
                </Location>
                <Location
                  row={1}
                  col={0}
                  colspan={2}
                  screen="sm"
                >
                </Location>
                <AntivirusLicenciasVsPosiblesRenovaciones ref={this.antivirusLicenciasVsPosiblesRenovacionesRef} />

              </Item>
              <Item>
                <Location
                  row={3}
                  col={1}
                  colspan={1}
                  screen="lg"
                >
                </Location>
                <Location
                  row={2}
                  col={0}
                  colspan={2}
                  screen="sm"
                >
                </Location>
                <div className="tittle2">
                  <div className="titleWidget" >Prestashop</div>
                </div>

              </Item>
            </ResponsiveBox>
          </div>
        </div>
      </React.Fragment>
    )
  }
}