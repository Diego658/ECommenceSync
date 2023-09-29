import React, { useCallback } from 'react'
import { useState } from 'react';
import { LoadIndicator, TextBox, Button, Popup } from 'devextreme-react';
import { antivirusLicenciasService } from '../../../_services/antivirusLicencias.service';
import DataGrid, { Column, SearchPanel, RequiredRule, HeaderFilter, Editing, Popup as GridPopup, Form, Lookup, Export } from 'devextreme-react/data-grid';
import { createStore } from 'devextreme-aspnet-data-nojquery';
import { config } from '../../../constants/constants'
import UserSesion from '../../../utils/userSesion';
import { authHeader } from '../../../_helpers';
import { Item } from 'devextreme-react/form';
import {
  Validator,
  EmailRule
} from 'devextreme-react/validator';
import { ClientesService } from '../../../_services/clientes.service';
import notify from 'devextreme/ui/notify';
import { AntivirusLicenciasRegistrarNotificacionManual } from './antivirus-licencias-registrar-notificacion';
import { AntivirusLicenciasVerFacturaAsociadaALicencia } from './AntivirusLicenciasVerFacturaAsociada';
import { AntivirusLicenciasRenovarLicencia } from './antivirus-licencias-renovar-licencia';
import { useMemo } from 'react';
import { confirm } from 'devextreme/ui/dialog';



const tiposContactos = [
  { 'ID': 1, 'Nombre': 'Teléfono' },
  { 'ID': 2, 'Nombre': 'Fax' },
  { 'ID': 3, 'Nombre': 'Metro' },
  { 'ID': 4, 'Nombre': 'Celular' },
  { 'ID': 5, 'Nombre': 'Recuperacion Cartera' },
  { 'ID': 6, 'Nombre': 'Retenciones' }

];

export function AntivirusLicenciasInfoVentaAntivirus({ idLicencia, codigoCliente }) {
  const [loading, setLoading] = useState(true);
  const [show, setShow] = useState(false);
  const [factura, setFactura] = useState(null);
  const [cliente, setCliente] = useState(null);
  const [ventaAntivirus, setVentaAntivirus] = useState(null);
  const [email, setEmail] = useState('');
  const [emailValid, setEmailValid] = useState(false);
  const [oculta, setOculta] = useState(false);

  const cargarDatos = useCallback(() => {
    setLoading(true);
    antivirusLicenciasService.getInfoVentaAntivirus(idLicencia, true)
      .then(info => {
        setFactura(info.factura);
        setCliente(info.cliente);
        setVentaAntivirus(info.ventaAntivirus);
        setEmail(info.cliente.email);
        setLoading(false);
      }, error => {
        setLoading(false);
      });
  }, [idLicencia])

  // useEffect(() => {
  //     if (idLicencia === undefined || idLicencia === 0) {
  //         return;
  //     }
  //     cargarDatos();
  // }, [cargarDatos, idLicencia]);

  const toogleShow = () => {
    const localShow = show;
    setShow(!show);
    if (!localShow) {
      cargarDatos();
    }
  }

  const onUpdate = useCallback(() => {
    cargarDatos();
  }, [cargarDatos]);





  const contactosDataSource = useMemo(() => {
    const auth = authHeader();
    const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
    return createStore({
      key: 'contacto',
      loadUrl: `${config.url.API_URL}/api/Clientes/GetContactosCliente?configuracionId=${configuracionId}&clienteCodigo=${codigoCliente}`,
      insertUrl: `${config.url.API_URL}/api/Clientes/AddContactoCliente?configuracionId=${configuracionId}&clienteCodigo=${codigoCliente}`,
      updateUrl: `${config.url.API_URL}/api/Clientes/UpdateContactoCliente?configuracionId=${configuracionId}&clienteCodigo=${codigoCliente}`,
      deleteUrl: `${config.url.API_URL}/api/Clientes/DeleteContactoCliente?configuracionId=${configuracionId}&clienteCodigo=${codigoCliente}`,
      onBeforeSend: (method, ajaxOptions) => {
        ajaxOptions.xhrFields = { withCredentials: false };
        ajaxOptions.headers = {
          'Content-Type': 'application/json',
          'Authorization': auth.Authorization,
        };
      }
    });
  }, [codigoCliente]);



  const onEmailSave = () => {
    ClientesService.updateEmail(cliente.codigo, email)
      .then(response => {
        notify('Correo actualizado correctamente.', 'success', 1500);
      }, error => {
        notify('Error al actualizar correo. ' + error, 'error', 1500);
      })
  }

  const ocultarVenta = async () => {
    let result = confirm("<i>¿Ocultar licencia?</i>", "Ocultar");
    result.then(async (dialogResult) => {
      if (dialogResult) {
        const res = await antivirusLicenciasService
          .ocultarLicencia(idLicencia, { Motivo: 'Ocultar Licencia' });
        if (res.isOk) {
          notify('Licencia oculta, actualice el listado.', 'info', 2500);
          setOculta(true);
        }
        else {
          notify(res.message, 'error', 3500);
        }

      }
    });
  }


  return (
    <>
      <Button icon='search' type='normal' onClick={toogleShow} />
      <Popup
        title="Información Licencia"
        showTitle={true}
        visible={show}
        onHiding={toogleShow}
        width={1000}
        height={600}
      >
        <div>



          {loading &&
            <LoadIndicator />
          }
          {!loading &&
            <div className={'content-block'}>
              <div className="infoVentaContaniner">
                <div className="infoFactura">
                  <div className="fieldGroupTittle">Información Factura</div>
                  <div className="fieldGroup">
                    <div className="fieldContainer">
                      <div className="fieldTitle">Transacción</div>
                      <div className="fieldValue">
                        <span>{factura.transaccionCodigo}</span>
                      </div>
                    </div>
                    <div className="fieldContainer">
                      <div className="fieldTitle">Número</div>
                      <div className="fieldValue">
                        <span>{factura.numero}</span>
                      </div>
                    </div>
                    <div className="fieldContainer">
                      <div className="fieldTitle">Serie</div>
                      <div className="fieldValue">
                        <span>{factura.numeroSerie}</span>
                      </div>
                    </div>
                  </div>
                  <div className="fieldGroup">
                    <div className="fieldContainer">
                      <div className="fieldTitle">Autorización</div>
                      <div className="fieldValue">
                        <span>{factura.numeroAutorizacion}</span>
                      </div>
                    </div>
                  </div>
                  <div className="fieldGroup">
                    <div className="fieldContainer">
                      <div className="fieldTitle">Observaciones</div>
                      <div className="fieldValue" style={{ 'height': '100px', 'overflowWrap': 'break-word' }}>
                        <span>{factura.observaciones}</span>
                      </div>
                    </div>
                  </div>

                </div>
                <div className="infoLicencia">
                  <div className="fieldGroupTittle">Información Licencia</div>
                  <div className="fieldGroup">
                    <div className="fieldContainer">
                      <div className="fieldTitle">Producto</div>
                      <div className="fieldValue">
                        <span>{ventaAntivirus.producto}</span>
                      </div>
                    </div>
                  </div>
                  <div className="fieldGroup">
                    <div className="fieldContainer">
                      <div className="fieldTitle">Referencia</div>
                      <div className="fieldValue">
                        <span>{ventaAntivirus.codigoVenta}</span>
                      </div>
                    </div>
                    <div className="fieldContainer">
                      <div className="fieldTitle">Cantidad</div>
                      <div className="fieldValue">
                        <span>{ventaAntivirus.cantidad}</span>
                      </div>
                    </div>
                    <div className="fieldContainer">
                      <div className="fieldTitle">Precio</div>
                      <div className="fieldValue">
                        <span>{ventaAntivirus.precio}</span>
                      </div>
                    </div>
                  </div>
                  <div className="fieldGroup">
                    <div className="fieldContainer">
                      <div className="fieldTitle">Series</div>
                      <div className="fieldValue">
                        <span>{ventaAntivirus.series}</span>
                      </div>
                    </div>
                  </div>
                </div>
                <div className="infoCliente">
                  <div className="fieldGroupTittle">Información Cliente</div>
                  <div className="fieldGroup">
                    <div className="fieldContainer">
                      <div className="fieldTitle">Nombres</div>
                      <div className="fieldValue">
                        <span>{cliente.nombreCompletos}</span>
                      </div>
                    </div>
                  </div>
                  <div className="fieldGroup">
                    <div className="fieldContainer">
                      <div className="fieldTitle">CI/RUC/PASAPORTE</div>
                      <div className="fieldValue">
                        <span>{cliente.identificacion}</span>
                      </div>
                    </div>
                    <div className="fieldContainer">
                      <div className="fieldTitle">Código</div>
                      <div className="fieldValue">
                        <span>{cliente.codigo}</span>
                      </div>
                    </div>
                  </div>
                  <div className="fieldGroup">
                    <div className="fieldContainer">
                      <div className="fieldTitle">Correo</div>
                      <div className="fieldValue" style={{ 'display': 'grid', 'gridTemplateColumns': '300px 20px', 'marginBottom': '10px' }}>
                        <TextBox text={email} onValueChanged={(value) => setEmail(value.value)} validationMessageMode='always' >
                          <Validator onValidated={({ isValid }) => {
                            setEmailValid(isValid);
                          }} >
                            <EmailRule message="Email incorrecto!!!" />
                            {/* <AsyncRule
                                                    message="Email is already registered"
                                                    validationCallback={asyncValidation} /> */}
                          </Validator>
                        </TextBox>
                        <Button icon='save' type='success' disabled={!emailValid} onClick={onEmailSave} />


                        {/* <form
                                        action={`${config.url.API_URL}/api/Clientes/UpdateEmailCliente?configuracionId=${configuracionId}&codigoCliente=${cliente.codigo}`}
                                        method='post'>
                                        <div className="fieldGroup">
                                            <div className="fieldValue">

                                            </div>
                                        </div>
                                        <div>
                                            
                                        </div>

                                    </form> */}
                      </div>
                    </div>
                  </div>
                  <div className="fieldGroup">
                    <div className="fieldContainer">
                      {/* <div className="fieldTitle">Contactos</div> */}
                      <div className="fieldValue">
                        <DataGrid
                          height={220}
                          dataSource={contactosDataSource}
                        >
                          <Column dataField="tipoContactoID"  >
                            <RequiredRule />
                            <Lookup dataSource={tiposContactos} valueExpr="ID" displayExpr="Nombre" />
                          </Column>
                          <Column dataField="contacto">
                            <RequiredRule />
                          </Column>
                          <Column dataField="persona">
                            <RequiredRule />
                          </Column>
                          <SearchPanel />
                          <HeaderFilter visible={true} />
                          <Export enabled={true} fileName="Employees" allowExportSelectedData={true} texts={{ 'exportAll': 'Exportar todos los contactos' }} />

                          <Editing
                            mode="popup"
                            allowUpdating={true}
                            allowAdding={true}
                            allowDeleting={true}
                            texts={{ 'addRow': 'Agregar contacto', 'editRow': 'Editar Contacto', 'deleteRow': 'Eliminar contacto' }}
                          >
                            <GridPopup title="Contacto" showTitle={true} width={400} height={250}>

                            </GridPopup>
                            <Form>
                              <Item dataField="tipoContactoID" />
                              <Item dataField="contacto" />
                              <Item dataField="persona" />
                            </Form>
                          </Editing>
                        </DataGrid>
                      </div>
                    </div>
                  </div>
                  <div className="fieldGroup">
                    <div className="buttonsContainer">
                      <div className="buttonItem">
                        <AntivirusLicenciasRegistrarNotificacionManual idLicencia={idLicencia} />
                      </div>
                      <div className="buttonItem">
                        <AntivirusLicenciasVerFacturaAsociadaALicencia idLicencia={idLicencia} codigoFactura={factura.transaccionCodigo} numeroFactura={factura.numero} />
                      </div>
                      <div className="buttonItem">
                        {!ventaAntivirus.renovada &&
                          <AntivirusLicenciasRenovarLicencia idLicencia={idLicencia} clienteCodigo={cliente.codigo} onUpdated={onUpdate} />
                        }
                        {ventaAntivirus.renovada &&
                          <Button icon="close" type='success' >
                            <span>Ver Renovación</span>
                            <AntivirusLicenciasInfoVentaAntivirus idLicencia={idLicencia} codigoCliente={cliente.codigo} />
                          </Button>
                        }

                      </div>
                      <div className="buttonItem">
                        <Button text="Ocultar Licencia" icon="close" type='danger' disabled={ventaAntivirus.oculta || oculta} onClick={ocultarVenta} />
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          }
        </div>
      </Popup>
    </>
  );
}


