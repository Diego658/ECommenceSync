import React from 'react';
import './notificaciones.scss'
import DataGrid, { Scrolling, Column, GroupPanel, SearchPanel, Paging } from 'devextreme-react/data-grid';
import DataSource from 'devextreme/data/data_source';
import UserSesion from '../../../utils/userSesion';
import ClienteCell from './clienteCell';
import NumeroNotificacionesCell from './numeroNotificacionesCell'
import FechaVencimientoCell from './fechaVencimientoCell';
import { antivirusLicenciasService } from '../../../_services/antivirusLicencias.service'
import { CustomLoadIndicator } from '../../../components';

export default class extends React.Component {

  constructor(props) {
    super(props);
    this.state = {
      ventas: [],
      cargando: false,
      vencimientosFuturos: false,
      popUpComentarioVisible: false,
      popUpComentarioTarget: null
    };
    this.onToolbarPreparing = this.onToolbarPreparing.bind(this);
    this.gridRef = React.createRef();
  }


  componentDidMount() {
    this.cargarVentas();
  }

  shouldComponentUpdate(nextProps, nextState) {
    if (this.state.ventas === nextState.ventas && this.state.cargando === nextState.cargando) {
      return false;
    } else {
      return true;
    }
  }



  async cargarVentas() {
    this.setState({
      cargando: true
    });

    await new Promise(r => setTimeout(r, 2000));

    const { vencimientosFuturos } = this.state;
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;

    antivirusLicenciasService.getVentasPorRenovar(configuracionId, vencimientosFuturos)
      .then(
        (result) => {
          this.setState({
            cargando: false,
            ventas: result

          });
        },
        (error) => {
          this.setState({
            cargando: false,
            error,
            ventas: null
          });
        }
      )


  }



  render() {
    //console.log('render App');
    const { cargando } = this.state;
    if (cargando) {
      return (
        <CustomLoadIndicator message={"Cargando..."} title={"Antivirus ventas"} />
      );
    }
    return (
      <DataGrid
        id="gridContainer"
        dataSource={new DataSource({
          store: {
            key: 'kardexId',
            type: 'array',
            data: this.state.ventas
          },
          sort: { getter: 'grupoVencimientoNumero', desc: false }
        })}
        showBorders={true}
        focusedRowEnabled={true}
        defaultFocusedRowIndex={0}
        columnAutoWidth={false}
        columnHidingEnabled={true}
        onRowDblClick={this.onRowDblClick}
        ref={this.gridRef}
        onToolbarPreparing={this.onToolbarPreparing}
        height={600}
        className={"grid-block"}
      >
        <SearchPanel visible={true} />
        <Scrolling mode="virtual" />
        <Paging pageSize={20} enabled={true} />
        <GroupPanel visible={false} />
        <Column dataField="grupoVencimiento" allowSearch={false} caption="Vencimiento" width={150} visible={false} />
        <Column dataField="cliente" allowSearch={true} cellRender={ClienteCell} width={300} />
        <Column dataField="producto" allowSearch={false} />
        <Column dataField="numeroNotificaciones" caption="# Notif." width={90} cellRender={NumeroNotificacionesCell} />
        <Column dataField="fechaVencimiento" caption="Vence" allowSearch={false} width={120} dataType="date" format="dd-MM-yyyy" cellRender={FechaVencimientoCell} />
        <Column dataField="fechaUltimaNotificacion" caption="Ult. Notif." allowSearch={false} width={120} dataType="date" format="dd-MM-yyyy" />
      </DataGrid>
    );
  }


  onToolbarPreparing(e) {
    e.toolbarOptions.items.unshift({
      location: 'after',
      widget: 'dxButton',
      options: {
        icon: 'refresh',
        hint: 'Actualizar',
        onClick: () => { this.cargarVentas(); }
      }
    }, {
      location: 'after',
      widget: 'dxButton',
      options: {
        icon: 'preferences',
        hint: 'Opciones',
        onClick: () => { this.cargarVentas(); }
      }
    });
  }

  get grid() {
    return this.gridRef.current.instance;
  }

  calculateRowCellValue = (data) => {
    var rowIndex = this.grid.getRowIndexByKey(data.kardexId);
    return rowIndex + 1;
  }


  onCellClick = (e) => {
    if (e.column.dataField === "cliente") {
      if (e.data.observacionesNotificacion !== null) {
        this.setState({
          popUpComentarioVisible: true,
          popUpComentarioTarget: `#comment${e.data.kardexId}`
        });
      }
      else {
        this.setState({
          popUpComentarioVisible: false,
          popUpComentarioTarget: null
        });
      }

    }
  }

  onRowDblClick = (e) => {
    this.props.mostrarVenta(e.data);
  }

  onCellPrepared = (e) => {

  }

}


