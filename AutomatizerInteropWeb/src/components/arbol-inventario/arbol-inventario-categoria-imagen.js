import React, { useState, useEffect } from 'react';
import './arbol-inventario.scss'
import './arbol-inventario-categoria.scss'
import { BlobService } from '../../_services/blob.service'
import { FileUploader, Button, LoadIndicator } from 'devextreme-react';
import { config } from '../../constants/constants'
import UserSesion from '../../utils/userSesion';


export function ArbolInventarioCategoriaImagen({ itemID, blobId, updateBlobId, deleteImage }) {
  const [blobUrl, setBlobUrl] = useState('');
  const [blob, setBlob] = useState(null);
  const [loading, setLoading] = useState(false);
  const [loadIndicatorVisible, setLoadIndicatorVisible] = useState(false);

  useEffect(() => {
    if (blobId > 0) {
      setLoading(true);
      BlobService.getBlobData(blobId).then(blobData => {
        setBlobUrl(window.URL.createObjectURL(blobData));
        setBlob(blobData);
        setLoading(false);
        setLoadIndicatorVisible(false);
      })
    }
  }, [blobId]);


  if (loading || blobId === 0) {
    return (
      <LoadIndicator id="small-indicator" height={20} width={20} />
    );
  }

  if (blobId > 0 && blob == null) {
    return (
      <React.Fragment>

      </React.Fragment>
    );
  }

  //No tiene imagen
  if (blobId === -1) {
    return (
      <React.Fragment>
        <FileUploader height={200} selectButtonText="Seleccionar imagen" labelText="o Arrastrar imagen aqui." multiple={false} showFileList={true} accept="image/*" uploadMode='instantly'
          uploadUrl={`${config.url.API_URL}/api/Upload/Upload?configuracionId=${UserSesion.getConfiguracion().idConfiguracionPrograma}&itemID=${itemID}&type=category_default`} onUploaded={async (e) => {
            const response = JSON.parse(e.request.responseText);
            //console.log(response);
            if (response && response.length > 0) {

              await updateBlobId(response[0].blobID);
              //setblobId(response.blobId);
            }
          }} />
      </React.Fragment>
    );
  }



  //const url = window.URL.createObjectURL(blob );
  //const length =  blob.size /1024;
  return (
    <React.Fragment>
      <div className="imagenCategoriaContainer">
        <div className="imagenCategoria" style={{ backgroundImage: `url(${blobUrl})` }} >
          <div className="imagenSize">{`${(blob.size / 1024).toFixed(2)} KB`}</div>
          <div className="deleteButton">
            <Button
              type="danger"
              stylingMode="contained"
              icon="trash"
              disabled={loadIndicatorVisible}
              onClick={async () => {
                setLoadIndicatorVisible(true);
                await deleteImage();
                setLoadIndicatorVisible(false);
              }}
            >
              <i className="dx-icon-trash">
                <LoadIndicator className="button-indicator" visible={loadIndicatorVisible} />
              </i>
            </Button>
          </div>
        </div>
      </div>
    </React.Fragment>
  );

}