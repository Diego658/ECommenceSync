import './arbol-inventario-item.scss'
import React, { useState, useEffect } from 'react'
import { LoadIndicator } from 'devextreme-react';
import { useCallback } from 'react';
import { InventarioImagenesItemsService } from '../../_services/inventario/imagenes-items-service'
import { ArbolInventarioImageRender } from './arbol-inventario-image-render';





export function ArbolInventarioItemImagenes({ itemId }) {
  const [imagenes, setImagenes] = useState([]);
  const [loading, setLoading] = useState(true);



  const cargaImagenes = useCallback(async () => {
    const respuesta = await InventarioImagenesItemsService.imagenes(itemId);
    if (respuesta.ok) {
      const imagenesData = respuesta.data;
      for (let index = 0; index < imagenesData.length; index++) {
        const element = imagenesData[index];
        element.visible = true;
      }
      
      imagenesData.unshift({ id:itemId, type: "upload", itemId: itemId, agregarImagen: cargaImagenes, visible: true });
      setImagenes(imagenesData);
    } else {
      setImagenes([]);
    }
    setLoading(false);
  }, [itemId]);


  const agregarImagen = useCallback(async (data)=>{
    const tmpImagenes = imagenes;
    tmpImagenes.push(data);
    setImagenes(tmpImagenes.slice(0));
  }, [imagenes])


  const establecerPortada= useCallback((imageId)=>{
    for (let index = 0; index < imagenes.length; index++) {
      const element = imagenes[index];
      if(element.type === "cover" && element.id !== imageId){
        element.type = "normal";
      }else if(element.id === imageId){
        element.type = "cover";
      }
    }
    setImagenes(imagenes.slice(0));
  }, [imagenes])


  useEffect(() => {
    if (itemId !== 0) {
      cargaImagenes(itemId);
    }
  }, [cargaImagenes, itemId]);



  if (loading) {
    return (
      <React.Fragment>
        <LoadIndicator id="small-indicator" height="64" width={64} />
      </React.Fragment>
    );
  }

  return (
    <>
      <div className="producto-imagenes-container" style={{}} >
        {imagenes.map(imagen => {
          return(
              <ArbolInventarioImageRender key={imagen.id} data={imagen} agregarImagen={agregarImagen} establecerPortada={establecerPortada} />
          )
        })

        }
      </div>
    </>);

  // return (
  //   <>
  //     <div className="paginaweb-imagenes-grid-container">
  //       <TileView  items={imagenes} baseItemHeight={144} baseItemWidth={224} itemMargin={10} itemRender={ArbolInventarioImageRender} direction='vertical'  >
  //       </TileView>
  //     </div>
  //   </>
  // );

}








