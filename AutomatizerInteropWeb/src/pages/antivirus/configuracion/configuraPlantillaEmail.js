import React, { useRef } from 'react';
import './configurarPlantillaEmail.scss'
import EmailEditor from 'react-email-editor'
import { useCallback } from 'react';
import { useEffect } from 'react';
import customBlocks from './customBlocks.json'

export function ConfiguraPlantillaEmail({ plantilla, onUpdatedData }) {
  const refHtmlEditor = useRef(null);


  const onUpdated = useCallback(() => {
    refHtmlEditor.current.exportHtml(function (data) {
      var json = data.design; // design json
      var html = data.html; // design html
      if (onUpdatedData) {
        onUpdatedData(json, html);
      }
      // Save the json, or html here
    });
  }, [onUpdatedData]);

  useEffect(() => {
    if (window.unlayer && plantilla) {
      refHtmlEditor.current.loadDesign(JSON.parse(plantilla.contenido));
      refHtmlEditor.current.addEventListener('design:updated', onUpdated);
      //refHtmlEditor.current.options.blocks= {customBlocks};
      //return refHtmlEditor.current.removeEventListener('design:updated', onUpdated);
    }
  }, [onUpdated, plantilla]);

  const arrayBlocks = customBlocks;
  //arrayBlocks.push(customBlocks);
  return (
    <React.Fragment>
      <EmailEditor
        ref={refHtmlEditor}
        appearance={{ theme: 'dark' }}
        projectId={4655}
        minHeight='90%'
        options={{
          displayMode: 'email',
          locale: 'es-ES',
          blocks: arrayBlocks,
          mergeTags: {
            nombre_cliente: {
              name: "Nombre cliente",
              value: "{{nombre_cliente}}"
            },
            nombre_producto: {
              name: "Producto",
              value: "{{nombre_producto}}"
            },
            cantidad_producto: {
              name: "Cantidad",
              value: "{{cantidad_producto}}"
            },
            fecha_vencimiento:{
              name: "Fecha Vencimiento",
              value: "{{fecha_vencimiento}}"
            }
          }
        }
        }
      />
    </React.Fragment>
  );
}