import React, { useState, useEffect } from 'react'
import { TextBox, ScrollView, Button } from 'devextreme-react';
import { InventarioService } from '../../../../_services/inventario.service';
import { BlobImageManager } from '../../../../components/blobImageManager/blob-Image-manager';
import { useCallback } from 'react';
//import { HtmlEditorWithPopUp } from '../../../../components/html-editor/html-editor-with-popup';
import notify from 'devextreme/ui/notify';
import { CustomSunHtmlEditor } from '../../../../components/sun-editor/customHtmEditor';


export function InventarioConfiguracionMarca({ marcaId }) {
  const [marca, setMarca] = useState(null);
  //const [marcaEdicion, setMarcaEdicion] = useState({});
  const [cambios, setCambios] = useState({});
  const [tieneCambios, setTieneCambios] = useState(false);
  const [gridHeight] = useState(window.innerHeight - 250);

  useEffect(() => {
    InventarioService.getMarca(marcaId).then(response => {
      setMarca(response);
      setCambios({ id: marcaId });
      setTieneCambios(false);
    });
  }, [marcaId])


  const onFielChanged = (field, fieldValue) => {
    setCambios({ ...cambios, field: fieldValue });
    
    setTieneCambios(true);
  }

  const onBlobIdUpdated = useCallback((newBlobId) => {
    setMarca({ ...marca, logoBlobId: newBlobId });
  }, [marca]);

  const guardarCambios = () => {
    //console.log(marca);
    InventarioService.guardarMarca(marcaId, marca).then(response => {
      notify('Marca guardada correctamente...', 'success', 2000);
      setCambios({ id: marcaId });
      setTieneCambios(false);
    }, error => {
      notify('Error al guardar marca ' + error, 'error', 2000);
    });
  }


  const handleChanged= useCallback((value, field)=>{
    marca[field] = value;
    setMarca(marca);  
    setTieneCambios(true);
  }, [marca])

  if(marca ==null){
    return(<></>)
  }
  return (
    <ScrollView height={gridHeight}>
      <div className='content-block'>
        <div className='content-row buttons-container'  >
          <div className='button-container'>
            <Button icon='save' text='Guardar' disabled={!tieneCambios} onClick={guardarCambios} />
          </div>

        </div>
        <div className='content-row' >
          <div className="fieldTitle">Nombre</div>
          <TextBox value={marca.nombre} onValueChanged={(value) => { handleChanged(value.value, 'nombre') }} />
        </div>
        <div className='content-row'>
          <div className='fieldTitle content-row'>Logo</div>
          <BlobImageManager blobId={marca.logoBlobId} typeImage="MARCA" entityId={marcaId} onBlobIdChanged={onBlobIdUpdated} />
        </div>
        <div className='content-row'>
          <div className='fieldTitle content-row'>Descripción Corta</div>
          <CustomSunHtmlEditor showToolbar={false} initialContents={marca.descriptionShort} updateFunction={(content)=>{
            handleChanged(content, 'descriptionShort');
            
            }} />
        </div>
        <div className='content-row'>
          <div className='fieldTitle content-row' >Descripción</div>
          <CustomSunHtmlEditor showToolbar={true} initialContents={marca.description} updateFunction={(content)=>{
            handleChanged(content, 'description');
            }} />
          
        </div>
      </div>
    </ScrollView>
  );
}