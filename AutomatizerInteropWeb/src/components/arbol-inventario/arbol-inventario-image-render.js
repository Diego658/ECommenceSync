import React, { useState } from 'react';
import { FechHelper } from '../../_helpers/fech-helper';
import { FileUploader } from 'devextreme-react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import ContextMenu from 'devextreme-react/context-menu';
import { Popup } from 'devextreme-react';
import { CssLoadingIndicator } from '../cssLoader/css-loader';
import { useEffect } from 'react';
import { BlobService } from '../../_services/blob.service';
import { InventarioImagenesItemsService } from '../../_services/inventario/imagenes-items-service';
import notify from 'devextreme/ui/notify';
import { useCallback } from 'react';

export function ArbolInventarioImageRender({ data, agregarImagen, establecerPortada }) {
  const [imagen, setImagen] = useState(null);
  const [loading, setLoading] = useState(true);
  const [visible, setVisible] = useState(true);
  const [zoom, setZoom] = useState(false);

  const toogleZoom = useCallback(() => {
    setZoom(!zoom);
  }, [zoom]);

  const handleEvent = useCallback(async (event, data) => {
    console.log(event);
    if (event === 'zoom') {
      toogleZoom();
    } else if (event === 'eliminar') {
      const response = await InventarioImagenesItemsService.remove(data.id);
      if (response.ok) {
        setVisible(false);
        notify('Imagen eliminada', 'success', 1500);
      } else {
        notify(response.data, 'error', 2500);
      }
    } else if (event === 'portada') {
      data.type ="cover";
      const response = await InventarioImagenesItemsService.update(data.id, data);
      if (response.ok) {
        setImagen({...data});
        notify('Nueva portada establecida', 'success', 1500);
        establecerPortada(data.id);
      } else {
        notify(response.data, 'error', 2500);
      }
    }

  }, [establecerPortada, toogleZoom]);



  useEffect(() => {
    if (data.type !== "upload") {
      BlobService.getBlobData(data.blobID).then(blobData => {
        const tmp = {
          ...data,
          blob: blobData,
          url: URL.createObjectURL(blobData),
          zoom: toogleZoom,
          handleEvent: handleEvent
        }
        setImagen(tmp);
        setLoading(false);
      });
    }
    else {
      setLoading(false);
    }
  }, [data, handleEvent, toogleZoom])


  if (data == null || !visible) {
    return null;
  }

  if (loading) {
    return (
      <div style={{ width: 225, height: 150, }} className='produto-image-container'   >
        <CssLoadingIndicator />
      </div>

    )
  }

  if (data.type === 'upload') {
    return (
      <div style={{ width: 225, height: 150, }} className='produto-image-container'   >
        <div className="product-image-uploader"  >
          <FileUploader multiple={true} showFileList={false} accept="image/*" uploadMode='instantly'
            uploadUrl={FechHelper.getUrl('Inventario', 'ImagenesItems/Upload', null, { itemId: data.itemId })}
            uploadHeaders={FechHelper.getAutorizedHeader()}
            onUploaded={async (e) => {
              //console.log('On Uploaded' + e.request.responseText);
              const response = JSON.parse(e.request.responseText);
              if (response && response.length > 0) {
                for (let index = 0; index < response.length; index++) {
                  const element = response[index];
                  await agregarImagen(element);
                }
              }
            }} />
        </div>
      </div>
    )
  }






  const imgSize = imagen.blob.size / 1024;
  const imgKey = `productImage${imagen.id}`;

  return (
    <div style={{ width: 225, height: 150, }} className='produto-image-container'   >
      <div id={imgKey} className="product-image" style={{ backgroundImage: `url(${imagen.url})` }} onDoubleClick={toogleZoom} >
        {imagen.type === "cover" &&
          <FontAwesomeIcon icon='bookmark' size='2x' className='product-image-portada' />
        }
        <div className="product-image-size">{`${imgSize.toFixed(2)} KB`}</div>
        <Popup
          width='90%'
          height='90%'
          visible={zoom}
          onHiding={toogleZoom}
        >
          <div className="product-image-zoom" style={{ backgroundImage: `url(${imagen.url})` }} />
        </Popup>
        <ContextMenu
          dataSource={imageContextMenuItems}
          width={200}
          target={'#' + imgKey}
          itemRender={ContextMenuItemTemplate}
          onItemClick={({ itemData }) => { onContextMenuItemnClick(itemData, imagen) }} >
        </ContextMenu>
      </div>
    </div>
  );

}

function ContextMenuItemTemplate(e) {
  return (
    <React.Fragment>
      <FontAwesomeIcon icon={e.icon} />
      {e.items ? <span className="dx-icon-spinright" /> : null}
      <span style={{ marginLeft: '5px' }} >{e.text}</span>
    </React.Fragment>
  );
}


const imageContextMenuItems = [
  {
    id: 'portada',
    text: 'Establecer Portada',
    icon: 'bookmark',
  },
  {
    id: 'eliminar',
    text: 'Eliminar',
    icon: 'trash-alt'
  },
  {
    id: 'zoom',
    text: 'Zoom',
    icon: 'search-plus'
  }
];

function onContextMenuItemnClick(itemData, image) {
  image.handleEvent(itemData.id, image);
  // if (itemData.id === 'zoom') {
  //   image.zoom(image);
  // } else if (itemData.id === 'portada') {
  //   image.portada(image);
  // } else if (itemData.id === 'eliminar') {
  //   image.delete(image);
  //   image.agregarImagen(image.itemId);    
  // }

}
