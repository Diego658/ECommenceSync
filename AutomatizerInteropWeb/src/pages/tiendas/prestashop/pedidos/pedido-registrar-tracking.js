import React, { useState } from 'react';
import { FieldGroup } from '../../../../components/fieldGroup/fieldGroup';
import { Field } from '../../../../components/field/field';
import { useEffect } from 'react';
import { PrestashopService } from '../../../../_services/stores/prestashop-service';
import { TextBox, Button, LoadIndicator, SelectBox, NumberBox, FileUploader } from 'devextreme-react';
import { CssLoadingIndicator } from '../../../../components/cssLoader/css-loader';
import notify from 'devextreme/ui/notify';
import { GuiasTransporteService } from '../../../../_services/guiasTransporte.service';
import { GroupPanel } from '../../../../components/panel/GroupPanel';
import { config } from '../../../../constants/constants';
import { authHeader } from '../../../../_helpers';
import UserSesion from '../../../../utils/userSesion';
import { BlobService } from '../../../../_services/blob.service';


export default function PedidoRegistrarTracking({ idPedido }) {
  const [loading, setLoading] = useState(true);
  const [actualizado, setActulizado] = useState(false);
  const [actualizando, setActualizando] = useState(false);
  const [carrier, setCarrier] = useState(null);
  const [CarrierInfo, setCarrierInfo] = useState(null);
  const [pedido, setPedido] = useState(null);
  const [tracking, setTracking] = useState('');
  const [transportistas, settransportistas] = useState(null);
  const [transportista, settransportista] = useState(null);
  const [nroPiezas, setnroPiezas] = useState(1);
  const [costoEnvio, setcostoEnvio] = useState(0);
  const [blobId, setBlobId] = useState(0);
  const [blobUrl, setblobUrl] = useState(null);
  const [blob, setBlob] = useState(null);
  const [loadIndicatorVisible, setLoadIndicatorVisible] = useState(false);


  const cargarDatos = async (id) => {
    const p = await PrestashopService.getOrden(id);
    if (p) {
      const a2 = await PrestashopService.getAddres(p.id_address_delivery);

      const c = await PrestashopService.getCustomer(p.id_customer);

      const carInfo = await PrestashopService.getOrdenCarrier(id);
      setCarrierInfo(carInfo);
      const car = await PrestashopService.getCarrier(carInfo.id_carrier);
      setCarrier(car);
      setPedido(p);
      setTracking(p.shipping_number);
      setcostoEnvio(carInfo.shipping_cost_tax_incl);
      const list = await GuiasTransporteService.getCompaniasTransporte();
      settransportistas(list);

    }
    setLoading(false);
  }


  const actualizarTracking = async () => {
    try {
      const r = await PrestashopService.updateTracking(idPedido,
        {
          OrdenId: idPedido,
          TransporteId: transportista,
          Tracking: tracking,
          NumeroPiezas: nroPiezas,
          CostoEnvio: costoEnvio,
          BlobId: blobId
        });
      if (r.isOk) {
        notify('Número de seguimiento actualizado correctamente...', 'info', 1500);
        setActulizado(true);
      }
      else {
        notify('Error ' + r.error, 'error', 2500);
        setActulizado(false);
      }
    } catch (error) {
      notify('Error ' + error, 'error', 2500);
      setActulizado(false);
    }
    setActualizando(false);
  }

  useEffect(() => {
    cargarDatos(idPedido);
  }, [idPedido]);


  const cargarBlob = async (id) => {
    var data = await BlobService.getBlobData(id);
    setblobUrl(URL.createObjectURL(data));
    setBlob(data);
  };



  if (loading) {
    return <CssLoadingIndicator />
  }

  const auth = authHeader();
  const configuracionId = UserSesion.getConfiguracion().idConfiguracionPrograma;
  return (
    <div className='registroTracking'>
      <div className='registroInfoTracking'>
        <FieldGroup>
          <Field label='Referencia' size='x1' >
            <span>{pedido.reference}</span>
          </Field>
        </FieldGroup>
        <FieldGroup>
          <Field label={`Carrier (${carrier.name})`} size='x2' >
            <SelectBox
              items={transportistas}
              searchEnabled={true}
              searchMode='contains'
              showDataBeforeSearch={true}
              displayExpr='nombre'
              valueExpr='companiaID'
              showClearButton={true}
              value={transportista}
              onValueChanged={(e) => settransportista(e.value)}
            />
          </Field>
        </FieldGroup>
        <FieldGroup>
          <Field label='Tracking' size='x1' >
            <TextBox maxLength={50} value={tracking}
              onValueChanged={(e) => {
                setTracking(e.value)
              }} />
          </Field>
        </FieldGroup>
        <FieldGroup>
          <Field label='# Piezas' size='s1' >
            <NumberBox
              value={nroPiezas}
              showSpinButtons={true}
              showClearButton={false}
              min={1}
              max={50}
              onValueChanged={(e) => setnroPiezas(e.value)} />
          </Field>
          <Field label='Envío' size='s1' >
            <NumberBox value={costoEnvio} onValueChanged={(e) => setcostoEnvio(e.value)} />
          </Field>
        </FieldGroup>
        <FieldGroup>
          <Field size='x1' >
            <Button
              id='buttonActualizarTracking'
              disabled={actualizado || transportista == null || tracking == null}
              onClick={async () => {
                setActualizando(true);
                await actualizarTracking();
                setActualizando(false);
              }}

              width={180}
              height={40}
            >
              <LoadIndicator className="button-indicator" visible={actualizando} />
              <span className="dx-button-text">{(actualizando ? 'Guardando...' : 'Guardar')}</span>
            </Button>
          </Field>
        </FieldGroup>
      </div>
      <div className='registroImagenGuia'>
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
                  setBlobId(response[0].blobId);
                  //setblobId(response.blobId);
                }
              }} />
          }
          {blob &&
            <div className="imagenGuia" style={{ backgroundImage: `url(${blobUrl})` }} >
              <div className="imagenSize">{`${(blob.size / 1024).toFixed(2)} KB`}</div>
              <div className="deleteButton">
                <Button
                  type="danger"
                  stylingMode="contained"
                  icon="trash"
                  disabled={loadIndicatorVisible}
                  onClick={async () => {
                    setLoadIndicatorVisible(true);
                    setBlobId(0);
                    setBlob(null);
                    setLoadIndicatorVisible(false);
                  }}
                >
                  <i className="dx-icon-trash">
                    <LoadIndicator className="button-indicator" visible={loadIndicatorVisible} />
                  </i>
                </Button>
              </div>
            </div>
          }
        </GroupPanel>
      </div>
    </div>
  )
}