import React, { useState } from 'react'
import { useEffect } from 'react';
import { InventarioService } from '../../../_services/inventario.service';
import { CustomLoadIndicator } from '../../../components';
import './errores-sincronizacion.scss'
import List from 'devextreme-react/list';
import DataSource from "devextreme/data/data_source";
import ContextMenu from 'devextreme-react/context-menu';
import { ContextMenuItem } from 'devextreme-react/file-manager';
import { Popup, ScrollView } from 'devextreme-react';
import { ArbolInventarioItem } from '../../../components/arbol-inventario/arbol-inventario-item';
import { SpeedDialAction } from 'devextreme-react/speed-dial-action';

export function ErroresSincronizacion(props) {
  const [errores, setErrores] = useState([]);
  const [errorSeleccionado, setErrorSeleccionado] = useState(null);
  const [loading, setLoading] = useState(true);
  const [showProduct, setShowProduct] = useState(false);


  const cargarDatos = () => {
    InventarioService.getErroresSincronizacionProductos().then(response => {
      setErrores(response);
      setLoading(false);
      setErrorSeleccionado(null);
    });
  }

  useEffect(() => {
    cargarDatos();
  }, [props])

  if (loading) {
    return (
      <React.Fragment>
        <CustomLoadIndicator message={"Cargando..."} title={"Errores sincronizacion"} />
      </React.Fragment>
    );
  }

  const listDataSource = new DataSource({
    store: errores,
    group: "storeName"
  });

  return (
    <>
      <SpeedDialAction
        icon="refresh"
        index={1}
        visible={true}
        onClick={() => {
          cargarDatos();
        }} />
      <div className={'content-block'}>
        <div className={'dx-card responsive-paddings'}>
          <div className="gridContainer">
            <div className="listadoErrores">
              <List
                dataSource={listDataSource}
                height="100%"
                grouped={true}
                collapsibleGroups={true}
                itemRender={(item) => {
                  const divId = "errorSycn" + item.itemID;
                  return (
                    <div className="errorContainer" id={divId} >
                      <i className={"dx-icon-" + (item.tipo === "PRODUCTO" ? "cart" : "photo")} />
                      <div className="error">
                        <div className="nombreItem">{item.nombre}</div>
                        <div className="referenciaItem">{item.referencia}</div>
                      </div>
                      <ContextMenu onItemClick={(button) => {
                        setErrorSeleccionado(item);
                        setShowProduct(true);
                      }} target={"#errorSycn" + item.itemID} >
                        <ContextMenuItem text="Ver Error" />
                      </ContextMenu>
                    </div>
                  );
                }}
                onItemClick={(item) => { setErrorSeleccionado(item.itemData) }}

              />
            </div>
            <div className="arbolInventario">
              <ArbolInventarioItem itemId={errorSeleccionado == null? null: errorSeleccionado.itemID}  ></ArbolInventarioItem>
            </div>
          </div>
        </div>
      </div>
      <Popup
        width={900}
        height={600}
        visible={showProduct}
        showTitle={false}
        closeOnOutsideClick={true}
        onHiding={() => { setShowProduct(false); }}
      >
        <ScrollView>
          <p>{errorSeleccionado === null ? "" : errorSeleccionado.error}</p>
        </ScrollView>

      </Popup>
    </>
  )
}

// function GroupTemplate(item) {
//     return <div>{'Tienda: ' + item.storeName}</div>;
//   }

// function ErrorListInfo(item) {
//     return (
//         <div className="errorContainer">
//             <i className={"dx-icon-" + (item.tipo === "PRODUCTO"? "cart": "photo")} />
//             <div className="error">
//                 <div className="nombreItem">{item.nombre}</div>
//                 <div className="referenciaItem">{item.referencia}</div>
//             </div>
//         </div>
//     );
//   }