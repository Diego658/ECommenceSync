import './blob-Image-manager.scss'
import './buttons.scss'
import React, { useState } from 'react'
import { FileUploader,  } from 'devextreme-react';
import { config } from '../../constants/constants';
import UserSesion from '../../utils/userSesion';
import { useEffect } from 'react';
import { BlobService } from '../../_services/blob.service';
//import ReactImageZoom from 'react-image-zoom';
import { InventarioService } from '../../_services/inventario.service';
import { TransformWrapper, TransformComponent } from "react-zoom-pan-pinch";
import zoom_in from "./images/zoom-in.svg";
import zoom_out from "./images/zoom-out.svg";
import zoom_reset from "./images/zoom-reset.svg";
import trash from "./images/trash-alt-regular.svg"

export function BlobImageManager({ entityId = 0,
  blobId = 0,
  onBlobIdChanged = (newBlboId) => { },
  typeImage = '',
  maxFileSize = 8 * 1024 * 1024,
}) {

  const [blob, setBlob] = useState(null);
  const [blobUrl, setBlobUrl] = useState('');

  useEffect(() => {
    console.log(blobId);
    if (blobId <= 0) {
      setBlob(null);
      setBlobUrl('');
    } else {
      BlobService.getBlobData(blobId).then(blobResponse => {
        setBlobUrl(URL.createObjectURL(blobResponse));
        setBlob(blobResponse);
      });
    }

  }, [blobId]);


  const deleteImage = async () => {
    await InventarioService.deleteImageItem(entityId, 0, blobId, typeImage)
    onBlobIdChanged(0);
  }

  if (blobId <= 0) {
    return (
      <>
        <FileUploader maxFileSize={maxFileSize} selectButtonText="Seleccionar imagen" labelText="o Arrastrar imagen aqui." multiple={false} showFileList={true} accept="image/*" uploadMode='instantly'
          uploadUrl={`${config.url.API_URL}/api/Upload/Upload?configuracionId=${UserSesion.getConfiguracion().idConfiguracionPrograma}&itemID=${entityId}&type=${typeImage}`} onUploaded={async (e) => {
            const response = JSON.parse(e.request.responseText);
            if (response && response.length > 0) {
              await onBlobIdChanged(response[0].blobID);
              //await updateBlobId(response[0].blobID);
              //setblobId(response.blobId);
            }
          }} />
      </>
    );
  }

  
  return (
    <div className="container">
      <div className="row align-items-center">
        <div className="col-lg-12 order-lg-2 example">
          <TransformWrapper
            options={{
              limitToBounds: true,
              transformEnabled: true,
              disabled: false,
              limitToWrapper: true,
            }}
            pan={{
              disabled: false,
              lockAxisX: false,
              lockAxisY: false,
              velocityEqualToMove: true,
              velocity: true,
            }}
            pinch={{ disabled: false }}
            doubleClick={{ disabled: false }}
            wheel={{
              wheelEnabled: true,
              touchPadEnabled: true,
              limitsOnWheel: false,
            }}
          >
            {({
              zoomIn,
              zoomOut,
              resetTransform,
              setDefaultState,
              positionX,
              positionY,
              scale,
              previousScale,
              options: { limitToBounds, transformEnabled, disabled },
              ...rest
            }) => (
                <React.Fragment>
                  <div className="tools">
                    <div className="spacer" />
                    <button
                      className="btn-gradient blue mini"
                      onClick={zoomIn}
                      data-testid="zoom-in-button"
                    >
                      <img src={zoom_in} alt="" width={16} height={16} />
                    </button>
                    <button
                      className="btn-gradient blue mini"
                      onClick={zoomOut}
                      data-testid="zoom-out-button"
                    >
                      <img src={zoom_out} alt="" width={16} height={16} />
                    </button>
                    <button
                      className="btn-gradient blue mini"
                      onClick={resetTransform}
                      data-testid="reset-button"
                    >
                      <img src={zoom_reset} alt="" width={16} height={16} />
                    </button>
                    <button
                      className="btn-gradient red mini"
                      onClick={deleteImage}
                      data-testid="delete-button"
                    >
                      <img src={trash} alt="" width={16} height={16} />
                    </button>
                  </div>
                  <div className="element">
                    <TransformComponent>
                      <img
                        className="zoom"
                        src={blobUrl}
                        alt=""
                      />
                    </TransformComponent>
                  </div>
                  <div className="info">
                    <div className="imageSize">{blob==null? '' : `${(blob.size / 1024).toFixed(2)} KB` }</div>
                  </div>
                </React.Fragment>
              )}
          </TransformWrapper>
        </div>
      </div>
    </div>

  );


  //const props = { width: width, height :height ,  img: blobUrl, scale:1.5 };

  // return (
  //   <div className="container" style={{ width: width, height: height }}>
  //     <div className="imageZoom">
  //       <ReactImageZoom {...props} />
  //       <div className="imageSize">{`${(blob.size / 1024).toFixed(2)} KB`}</div>
  //       <div className="deleteButton">
  //         <Button
  //           type="danger"
  //           stylingMode="contained"
  //           icon="trash"
  //           hint='Eliminar Imagen'
  //           onClick={async () => {
  //             await InventarioService.deleteImageItem(entityId, 0, blobId, typeImage)
  //             onBlobIdChanged(0);
  //           }}
  //         >
  //           <i className="dx-icon-trash">

  //           </i>
  //         </Button>
  //       </div>
  //     </div>
  //   </div>
  // );
}