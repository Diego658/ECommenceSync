import './guias-transporte.scss'
import React, { useState } from 'react';
import { useEffect } from 'react';
import { LoadIndicator, CheckBox, Popup } from 'devextreme-react';
import { GuiasTransporteService } from '../../_services/guiasTransporte.service';
import { BlobService } from '../../_services/blob.service';
import { FieldGroup } from '../../components/fieldGroup/fieldGroup';
import { Field } from '../../components/field/field';
import { PrestashopService } from '../../_services/stores/prestashop-service';



export default function GuiasGuiaTransporte({ id }) {
  const [loading, setloading] = useState(true);
  const [guia, setGuia] = useState(null);
  const [blob, setBlob] = useState(null);
  const [blobUrl, setBlobUrl] = useState(null);
  const [showImage, setShowImage] = useState(false);


  const cargarDatos = async (idGuia) => {
    const g = await GuiasTransporteService.getGuia(idGuia);
    if (g.idBlob > 0) {
      const tmp = await BlobService.getBlobData(g.idBlob);
      setBlob(tmp);
      setBlobUrl(URL.createObjectURL(tmp));
    }
    setGuia(g);
    setloading(false);
  }

  useEffect(() => {
    cargarDatos(id);
  }, [id])

  if (loading) {
    return (<LoadIndicator />);
  }

  return (
    <div className='guiaTransporteContent'>
      <div className='guiaTransporteInfo'>

        <FieldGroup>
          <Field label='# Guía' size='x1'>
            <span>{guia.numeroGuia}</span>
          </Field>
          <Field label='Fecha'>
            <span>{guia.fecha}</span>
          </Field>
        </FieldGroup>
        <FieldGroup>
          <Field label='Cliente' size='x2'>
            <span>{guia.cliente}</span>
          </Field>
        </FieldGroup>
        <FieldGroup>
          <Field label='Compañia Transporte' size='x2'>
            <span>{guia.compania}</span>
          </Field>
        </FieldGroup>
        <FieldGroup>
          <div>
            <div style={{ fontSize: 'small', color: 'rgb(168, 165, 162)', fontWeight: 'lighter', margin: '10px' }} >Detalle Mercadería</div>
            <p style={{ 'marginLeft': '15px', textOverflow: 'clip' }} >{guia.detalleMercaderia}</p>
          </div>
        </FieldGroup>
        <FieldGroup>
          <Field label='# piezas' size='s1'>
            <span>{guia.numeroPiezas}</span>
          </Field>
          <Field label='Cobrado' size='s1'>
            <CheckBox value={guia.cobrado} />
          </Field>
          <Field label='Entregado' size='s1'>
            <CheckBox value={guia.entregado} />
          </Field>
          <Field label='Costo' size='s1'>
            <span>{guia.costoEnvio}</span>
          </Field>
        </FieldGroup>

      </div>
      <div className='guiaTransporteImagen'>
        <div className='imagenGuiaTransporte' onClick={()=>{setShowImage(true)}} style={{ backgroundImage: `url(${blobUrl ? blobUrl : './no-image-icon'})` }}>

        </div>
        <Popup
          title='Ver Imagen'
          showTitle={true}
          visible={showImage}
          onHiding={()=> setShowImage(false)}
          closeOnOutsideClick={true}
          width='80%'
          height='90%'
        >
          <img src={`${blobUrl ? blobUrl : './no-image-icon'}`} alt='' height='100%' width='auto' />
        </Popup>
      </div>
    </div>
  )
}