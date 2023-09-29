import React from 'react';
import './notificaciones.scss'
import Form, { Item, SimpleItem, TabbedItem, Tab } from 'devextreme-react/form';
//import Moment from 'react-moment';
import {GridContactos} from '../../../components/cliente/gridContactos'
import { Button } from 'devextreme-react';
import notify from 'devextreme/ui/notify';


export default class extends React.Component {
    componentDidMount(){
        
    }
    render (){
        const {codigoCliente} = this.props.venta;
        
        return(
            <React.Fragment>
                {/* <div className="long-title"><h3>Personal details</h3></div> */}
                <Form
                    colCount={2}
                    id="formVentaAntivirus"
                    formData={this.props.venta}
                >
                <Item itemType="group" caption="Información Venta"  >
                    <SimpleItem dataField="transaccionVenta" label={{text: "Transaccion"}} editorOptions={{disabled:true}} ></SimpleItem>
                    <SimpleItem dataField="trnNum" label={{text: "Numero"}} editorOptions={{disabled:true}} ></SimpleItem>
                    <SimpleItem dataField="fechaFactura" label={{text: "Fecha"}} editorOptions={{disabled:true}} ></SimpleItem>
                </Item>
                <Item itemType="group" caption="Información Licencia">
                    <SimpleItem dataField="producto" label={{text: "Producto"}} editorOptions={{disabled:true}} editorType="dxTextArea" ></SimpleItem>
                    <Item itemType="group"  >
                        <SimpleItem dataField="periodoLicencia" label={{text: "Periodo"}} editorOptions={{disabled:true}} ></SimpleItem>
                        <SimpleItem dataField="fechaVencimiento" label={{text: "Vence"}}  editorOptions={{disabled:true}} editorType="dxDateBox" ></SimpleItem>
                    </Item>
                    
                </Item>
                
                <TabbedItem  caption="Cliente" colSpan="2" tabPanelOptions={{deferRendering: false}}   >
                    <Tab title="Informacion Cliente">
                        <SimpleItem dataField="cliente" label={{text: "Nombre"}} editorOptions={{disabled:true}} ></SimpleItem>
                        <SimpleItem dataField="identificacionCliente" label={{text: "Cedula / Ruc"}} editorOptions={{disabled:true}} ></SimpleItem>
                        <SimpleItem dataField="emailCliente" label={{text: "E-Mail"}} editorOptions={{disabled:false}} ></SimpleItem>
                    </Tab>
                    <Tab title="Contactos">
                        <GridContactos codigoCliente={codigoCliente}></GridContactos>
                    </Tab>
                    <Tab title="Observaciones">
                        <SimpleItem dataField="ObservacionesNotificacion" label={{visible: false}} editorOptions={{disabled:true}} editorType="dxTextArea" ></SimpleItem>
                    </Tab>
                    <Tab  title="Historial Notificacioneas">

                    </Tab>
                </TabbedItem>
                <Item itemType="group" horizontalAlignment="rigth">
                    <Button
                        width={140}
                        text="Enviar Email"
                        type="default"
                        stylingMode="contained"
                        onClick={()=>{notify("Se pudo en cola de notificacioon.")}}
                    />
                    <Button
                        width={180}
                        text="Registrar Notificacion"
                        type="default"
                        stylingMode="contained"
                        onClick={this.onClick}
                    />
                </Item>
                </Form>
            </React.Fragment>
        )
    }    
}