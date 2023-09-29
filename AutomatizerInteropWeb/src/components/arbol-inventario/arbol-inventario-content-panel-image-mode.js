import React, { useState, useEffect } from 'react';
import './arbol-inventario.scss'


export function ArbolInventarioContentPanelImageMode(props){
    const[item, setItem] = useState(null);

    useEffect(()=>{
        if(props.selectedItem === null){
            return;
        }
        setItem(props.selectedItem);
    },[props.selectedItem]);

    if(item==null){
        return (
            
                <div className="item-details"  >Seleccione un item / categoria para modificar...</div>
            
        );    
    }

    return(
        <>
            <p>{item.nombre}</p>
        </>
    );

    // return (
    //     <>
                
    //             <div className={'dx-card responsive-paddings'}>
    //             { item.tipo==="G" &&
    //                 <ArbolInventarioCategoria categoria={item} />
    //             }
    //             { item.tipo==="I" &&
    //                 <ArbolInventarioItem item={item} ></ArbolInventarioItem>
    //             }
    //             </div>
                
    //     </>        
    // );

}