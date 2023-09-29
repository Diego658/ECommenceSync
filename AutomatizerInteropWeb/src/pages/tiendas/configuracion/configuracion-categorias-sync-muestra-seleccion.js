import React, { useState, useEffect } from 'react'
import { List } from 'devextreme-react';
import './configuracion-categorias.scss'
export function MuestraCategoriasSeleccionadas({ categorias, classNameExistencia }) {
  const [categoriasSeleccionadas, setCategoriasSeleccionadas] = useState([]);
  //const[categoriasOcultas, setcategoriasOcultas] = useState([]);

  useEffect(() => {
    if (categorias) {
      let cat = [];
      for (var key in categorias) {
        const value = categorias[key];
        cat.push(value);
      }
      setCategoriasSeleccionadas(cat);
    };
  }, [categorias])

  return (
    <>
      <List
        className="checked-items"
        width={400}
        items={categoriasSeleccionadas}
        itemRender={(item) => {
          return <div className={item.existenciaTotal > 0 ? classNameExistencia : ''} >{`${item.nombre} (${item.referencia})`}</div>;
        }}
      />

    </>
  );
}