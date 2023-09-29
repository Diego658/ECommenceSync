import React, { useState, useEffect } from 'react';
import './arbol-inventario.scss'
import { InventarioService } from "../../_services/inventario.service";
import { BlobImageManager } from '../blobImageManager/blob-Image-manager';
import { ScrollView } from 'devextreme-react';


export function ArbolInventarioCategoriaImagenes({ itemID, height }) {
    //const [loading, setloading] = useState(true);
    const [coverBlobId, setCoverBlobId] = useState(-1);

    useEffect(() => {
        //setloading(true);
        InventarioService.getImagesItem(itemID).then(data => {
            let tieneImagen = false;
            data.forEach(image => {
                if (image.type === "category_default") {
                    tieneImagen = true;
                    setCoverBlobId(image.blobID);
                }
            });
            if (!tieneImagen) setCoverBlobId(-1);
            //setloading(false);
        }, error => {
            //console.log(error);
            //setloading(false);
        })
    }, [itemID]);

    const onBlobIdChanged = (newId) => {
        setCoverBlobId(newId);
        //setloading(false);
    }

    return (
        <ScrollView height={height}>
            <div>
                <BlobImageManager blobId={coverBlobId} entityId={itemID} typeImage="category_default" onBlobIdChanged={onBlobIdChanged} containerHeight={'400px'} containerWidth={'100%'} />
            </div>
        </ScrollView>
    );

}