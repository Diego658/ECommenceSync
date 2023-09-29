import React from 'react'
import { TreeView } from 'devextreme-react';

// import { CustomLoadIndicator } from '../../../components';
// import { InventarioService } from '../../../_services/inventario.service';
import './configuracion-categorias.scss'
import { useCallback } from 'react';




export function SeleccionaCategoriasSincronizar(props) {
  //const[items, setItems] = useState([]);
  //const[categoriasSeleccionadas, setcategoriasSeleccionadas] = useState([]);
  //const[productosSeleccionados, setproductosSeleccionados] = useState([]);
  //    const[loading, setLoading] = useState(true);


  // useEffect(()=>{
  //     console.log('Cargar Arbol...');
  //     InventarioService.getArbolItems().then(items=>{
  //         //let checkedItems = items.filter((item)=> item.selected);

  //         setItems(items);
  //         setLoading(false);
  //     })
  // },[]);

  // if(loading){
  //     return(
  //         <React.Fragment>
  //             <CustomLoadIndicator message={"Cargando..."} title={"Categorias"}/>
  //         </React.Fragment>
  //         );
  // }
  const renderTreeItem = useCallback((data) => {
    return (
      <div className="treeItem box" key={data.itemID} >
        <div className="box-item">
          <i className={"dx-icon-" + (data.tipo === 'G' ? 'activefolder' : (data.existenciaTotal <= 0 ? 'close textoSinExistencia' : 'check textoConExistencia'))}></i>
        </div>
        {!data.hasImages &&
          <div className="box-item">
            <i className={"dx-icon-photo dx-icon-noimage"}></i>
          </div>
        }
        <div className="box-item">{(data.tipo === 'G' ? '' : data.referencia + ' - ') + data.nombre}</div>
        <div className="push">
          <span >{data.existenciaTotal}</span>
        </div>
      </div>
    );
  }, []);


  //console.log('Renderiza Arbol...');
  return (
    <>
      <TreeView
        id="selection-treeview"
        dataStructure="plain"
        items={props.datasource}
        keyExpr="codigo"
        parentIdExpr="codigoPadre"
        displayExpr="nombre"
        selectedExpr="selected"
        hasItemsExpr="HasItems"
        width={400}
        height={600}
        searchMode='contains'
        searchEnabled={false}
        selectionMode='multiple'
        selectByClick={false}
        selectNodesRecursive={false}
        showCheckBoxesMode='selectAll'
        virtualModeEnabled={true}
        //itemComponent={renderTreeItem}
        itemRender={renderTreeItem}
        onItemSelectionChanged={props.selectionChanges}
      />
    </>
  );
}

