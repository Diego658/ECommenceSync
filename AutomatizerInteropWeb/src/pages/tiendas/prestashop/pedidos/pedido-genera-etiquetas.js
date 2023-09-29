import React, { useState } from 'react';
import { Button, Popup } from 'devextreme-react';
import { PDFDownloadLink, Document, Page } from '@react-pdf/renderer'
import PdfA4Label from '../../../guias-transporte/pdf/pdf-a4-label';
import './pedidos.scss';
import { CssLoadingIndicator } from '../../../../components/cssLoader/css-loader';
import { useEffect } from 'react';
import { PrestashopService } from '../../../../_services/stores/prestashop-service';


export default function PedidoGenerarEtiquetas({ idPedido }) {
  const [loading, setLoading] = useState(true);
  const [deliverAddress, setDeliverAddress] = useState({});
  const [carrier, setCarrier] = useState({});
  const [carrierInfo, setCarrierInfo] = useState({});
  const [customer, setCustomer] = useState(null);
  const [pedido, setPedido] = useState(null);

  const cargarDatos = async (id) => {
    const p = await PrestashopService.getOrden(id);
    if (p) {
      const a2 = await PrestashopService.getAddres(p.id_address_delivery);
      setDeliverAddress(a2);
      const c = await PrestashopService.getCustomer(p.id_customer);
      setCustomer(c);
      const carInfo = await PrestashopService.getOrdenCarrier(id);
      setCarrierInfo(carInfo);
      const car = await PrestashopService.getCarrier(carInfo.id_carrier);
      setCarrier(car);
      setPedido(p);
    }
    setLoading(false);
  }

  useEffect(() => {
    cargarDatos(idPedido);
  }, [idPedido])

  if (loading) {
    return (
      <CssLoadingIndicator />
    )
  }

  return (
    <div className='pdfLabelsLinks' >
      <div className='pdfLabelsLink'>
        <Button>
          <PDFDownloadLink
            document={<PdfA4Label deliverAddres={deliverAddress} carrier={carrier} carrierInfo = {carrierInfo} customer={customer} pedido={pedido} />} fileName="Label-A4.pdf">
            {({ blob, url, loading, error }) => (loading ? 'Loading document...' : 'Generar Etiqueta A4')}
          </PDFDownloadLink>
        </Button>
      </div>
      {/* <div className ='pdfLabelsLink'>
            <Button>
              <PDFDownloadLink document={<PdfA4Label idPedido={idPedido} />} fileName="Label-A4.pdf">
                {({ blob, url, loading, error }) => (loading ? 'Loading document...' : 'Generar Etiqueta A3')}
              </PDFDownloadLink>
            </Button>
          </div> */}
    </div>
  )
}