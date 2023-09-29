import React, { useState } from 'react'
import { BlobService } from '../../_services/blob.service';
import { FieldGroup } from '../../components/fieldGroup/fieldGroup';
import { Field } from '../../components/field/field';
import { CheckBox, NumberBox, TextBox, DateBox, Lookup, SelectBox, Popup, TextArea, Button, TagBox, FileUploader } from 'devextreme-react';
import './guias-transporte.scss'
import { GroupPanel } from '../../components/panel/GroupPanel';
import { config } from '../../constants/constants';
import { authHeader } from '../../_helpers';
import UserSesion from '../../utils/userSesion';
import { useEffect } from 'react';
import { GuiasTransporteService } from '../../_services/guiasTransporte.service';
import { PrestashopService } from '../../_services/stores/prestashop-service';

export default function GuiasTransporteAgregarGuia() {
  const [loading, setloading] = useState(true);
  const [idGuia, setidGuia] = useState(0);
  const [numeroGuia, setnumeroGuia] = useState(null);
  const [fecha, setfecha] = useState(new Date());
  const [clientes, setclientes] = useState([]);
  const [cliente, setcliente] = useState(null);
  const [companias, setCompanias] = useState([]);
  const [compania, setcompania] = useState(null);
  const [detalleMercaderia, setdetalleMercaderia] = useState(null);
  const [direccion, setdireccion] = useState(null);
  const [numeroPiezas, setnumeroPiezas] = useState(1);
  const [cobrado, setcobrado] = useState(false);
  const [entregado, setentregado] = useState(false);
  const [costoEnvio, setcostoEnvio] = useState(0);
  const [guiaConFactura, setguiaConFactura] = useState(true);
  const [blobId, setblobId] = useState(0);
  const [blobUrl, setblobUrl] = useState(null);
  const [blob, setBlob] = useState(null);
  const [ShowImage, setShowImage] = useState(false);
  const [fechaBusqueda, setfechaBusqueda] = useState(new Date());
  const [facturas, setfacturas] = useState([]);
  const [factura, setFactura] = useState(null);


  const cargarBlob = async (id) => {
    var data = await BlobService.getBlobData(id);
    setblobUrl(URL.createObjectURL(data));
    setBlob(data);
  };

  const cargarFacturtas = async (fecha, clienteId) => {
    if (clienteId) {
      var facturas = await GuiasTransporteService.getFacturasParaGuia(clienteId, fecha.toJSON());
      setfacturas(facturas);
    } else {
      setfacturas([]);
      setFactura(null);
    }
  }

  const cargarClientes = async (fecha) => {
    try {
      const clientes = await GuiasTransporteService.getClientesParaGuia(fecha.toJSON());
      setclientes(clientes);
      setcliente(null);
    } catch (error) {
      console.log(error);
    }
  };

  useEffect(() => {
    const cargarCompanias = async () => {
      const c = await GuiasTransporteService.getCompaniasTransporte();
      setCompanias(c);
    }
    cargarCompanias();
  }, []);


  useEffect(() => {
    if (guiaConFactura) {
      if (cliente) {
        cargarFacturtas(fechaBusqueda, cliente.Clisec);
      } else {
        cargarClientes(fechaBusqueda);
      }

    } else {
      cargarClientes(fechaBusqueda);
    }
  }, [guiaConFactura, fechaBusqueda, cliente]);

  const auth = authHeader();
  const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;

  return (
    <div>
      <FieldGroup>
        <Field label='Fecha'>
          <DateBox value={fecha} pickerType='native' type='datetime' onValueChanged={(e) => {
            setfecha(e.value);
          }} />
        </Field>

        <Field label='# Guía' size='x1'>
          <TextBox value={numeroGuia} onValueChanged={(e) => {
            setnumeroGuia(numeroGuia);
          }}></TextBox>
        </Field>
        <Field label='Compañia Transporte' size='x2'>
          <SelectBox
            items={companias}
            searchEnabled={true}
            searchMode='contains'
            showDataBeforeSearch={true}
            displayExpr='nombre'
            valueExpr='companiaID'
            showClearButton={true}
            onValueChanged={(e) => {
              setcompania(e.value);
            }}
          />
        </Field>
      </FieldGroup>
      <FieldGroup>
        <Field label='Cliente' size='x2'>
          <Lookup
            items={clientes}
            displayExpr='RazonSocial'
            searchEnabled={true}
            onValueChanged={(e) => {
              setcliente(e.value);
            }}
          />
        </Field>
      </FieldGroup>
      <FieldGroup>
        <Field label='Factura' size='s1'>
          <CheckBox value={guiaConFactura} onValueChanged={(e => setguiaConFactura(e.value))} />
        </Field>
        {guiaConFactura &&
          <>
            <Field label='Fecha Busqueda' size='s2' >
              <DateBox value={fechaBusqueda} pickerType='native' type='date' onValueChanged={(e) => {
                setfechaBusqueda(e.value);
              }} />
            </Field>
            <Field label='Facturas' size='x1'>
              <TagBox
                displayExpr='Numerofactura'
                items={facturas}
                showSelectionControls={true}
                applyValueMode='useButtons'
                onValueChanged={async (e) => {
                  console.log(e.value);
                  setFactura(e.value);
                  if (e.value.length > 0) {
                    let detalle = "Envío de mercadería según factura(s):";
                    detalle = `${detalle} \n${e.value.map(f => `${f.Numerofactura} del ${f.Fecha.substring(0, 10)}`).join("\n")}`;
                    setdetalleMercaderia(detalle);
                    const infofactura = await GuiasTransporteService.getInfofacturaParaGuia(e.value[0].TrnCod, e.value[0].TrnNum);
                    const direccion = `${infofactura.CiudadCliente}, ${infofactura.DireccionCliente}`;
                    setdireccion(direccion);
                  } else {
                    setdetalleMercaderia("");
                  }

                }} />
            </Field>
          </>
        }
      </FieldGroup>
      <FieldGroup>
        <FieldGroup>
          <Field label='Detalle de Mercaderia' size='x2' height={80} >
            <TextArea height={60} value={detalleMercaderia} onValueChanged={(e) => setdetalleMercaderia(e.value)} ></TextArea>
          </Field>
        </FieldGroup>
        <FieldGroup>
          <Field label='Dirección' size='x2' height={80}>
            <TextArea height={60} value={direccion} onValueChanged={(e) => setdireccion(e.value)} ></TextArea>
          </Field>
        </FieldGroup>
      </FieldGroup>
      <FieldGroup>
        <Field label='# piezas' size='s1'>
          <NumberBox min={1} max={250} value={numeroPiezas} onValueChanged={(e) => setnumeroPiezas(e.value)} />
        </Field>
        <Field label='Cobrado' size='s1'>
          <CheckBox value={cobrado} onValueChanged={(e) => setcobrado(e.value)} />
        </Field>
        <Field label='Entregado' size='s1'>
          <CheckBox value={entregado} onValueChanged={(e) => setentregado(e.value)} />
        </Field>
        <Field label='Costo' size='s1'>
          <NumberBox value={costoEnvio} onValueChanged={(e) => setcostoEnvio(e.value)} ></NumberBox>
        </Field>
      </FieldGroup>
      <FieldGroup>
        <GroupPanel header='Imagen de Guía'>
          {blobId === 0 &&
            <FileUploader
              height={200}
              selectButtonText="Seleccionar imagen"
              labelText="o Arrastrar imagen aqui."
              multiple={false}
              showFileList={true}
              accept="image/*"
              uploadMode='instantly'
              uploadHeaders={{ 'Authorization': auth.Authorization, 'ConfiguracionId': configuracionId }}
              uploadUrl={`${config.url.API_URL}/api/Automatizer/GuiasTransporte/GuiasTransporte/UploadImageGuia`}
              onUploaded={async (e) => {
                const response = JSON.parse(e.request.responseText);
                //console.log(response);
                if (response && response.length > 0) {
                  await cargarBlob(response[0].blobId);
                  setblobId(response.blobId);
                }
              }} />
          }
          {blobUrl &&
            <div className="imagenGuia" style={{ height: 220, backgroundImage: `url(${blobUrl})` }} >
              <div className="deleteButton">
                <Button
                  type="danger"
                  stylingMode="contained"
                  icon="trash"
                  onClick={async () => {
                    setblobId(0);
                    setblobUrl(null);
                    setBlob(null);
                  }}
                >
                </Button>
              </div>
            </div>
          }
        </GroupPanel>
      </FieldGroup>
      <div className='buttonsBar'>
        <div className='buttonBarItem'>
          <Button text='Guardar Guía' icon='save' />
        </div>
      </div>
    </div>
  );
}