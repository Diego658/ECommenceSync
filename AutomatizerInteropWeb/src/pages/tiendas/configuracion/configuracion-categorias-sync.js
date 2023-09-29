import React, { useState, useCallback } from 'react'
import './configuracion-categorias.scss'
import { SeleccionaCategoriasSincronizar } from './configuracion-categorias-sync-selecciona';
import { MuestraCategoriasSeleccionadas } from './configuracion-categorias-sync-muestra-seleccion';
import { CustomLoadIndicator } from '../../../components';
import { useEffect } from 'react';
import { InventarioService } from '../../../_services/inventario.service';
import { Button } from 'devextreme-react';
import notify from 'devextreme/ui/notify';
import { LoadIndicator } from 'devextreme-react/load-indicator';


export function ConfiguracionCategoriasSync(props) {
  const [itemsSeleccionados, setitemsSeleccionados] = useState([]);
  const [categoriasSeleccionadas, setcategoriasSeleccionadas] = useState(null);
  const [categoriasOcultas, setCategoriasOcultas] = useState(null);
  const [items, setItems] = useState([]);
  const [saving, setSaving] = useState(false);

  const [loading, setLoading] = useState(true);


  const updateArbol = useCallback(function () {
    setLoading(true);
    InventarioService.getArbolItems().then(items => {
      setItems(items);
      setLoading(false);
      setcategoriasSeleccionadas({});
      setCategoriasOcultas({});
    });

  }, []);


  const guardarCategorias = useCallback(async () => {

    let categoriasIds = [];
    let categoriasOcultasIds = [];
    for (var key in categoriasSeleccionadas) {
      categoriasIds.push(categoriasSeleccionadas[key].itemID);
    }
    for (key in categoriasOcultas) {
      categoriasOcultasIds.push(categoriasOcultas[key].itemID);
    }
    try {
      await InventarioService.updateCategoriasToSync(categoriasIds, categoriasOcultasIds);
      notify('Categorias actualizadas.', 'info', 3000);
    } catch (error) {
      notify('Error al actualizar categorias ' + error.toString(), 'error', 3000);
    }

  }, [categoriasOcultas, categoriasSeleccionadas]);

  useEffect(() => {
    // InventarioService.getArbolItems().then(items=>{
    // setItems(items);
    // setLoading(false);
    // })
    // setcategoriasSeleccionadas({});
    // setCategoriasOcultas({});
    updateArbol();
  }, [updateArbol])




  const isProduct = useCallback(function (value) {
    return value.itemData.tipo === 'I';
  }, []);

  const processNode = useCallback(async function (node) {

    if (isProduct(node)) {
      let checkedItems = itemsSeleccionados;
      let itemIndex = -1;
      await checkedItems.forEach(async (item, index) => {
        if (item.codigo === node.key) {
          itemIndex = index;
          return false;
        }
      });
      if (node.selected && itemIndex === -1) {
        checkedItems.push(node.itemData)
      }
      else {
        checkedItems.splice(itemIndex, 1);
      }
      setitemsSeleccionados(checkedItems.slice(0));

    } else {
      let categorias = categoriasSeleccionadas;
      let ocultas = categoriasOcultas;
      if (node.selected) {
        if (!categorias[node.itemData.itemID]) {
          categorias[node.itemData.itemID] = node.itemData;
        }
        const deleteFn = async (categoria) => {
          delete ocultas[categoria.itemData.itemID]; //Eliminamos categoria
          //Buscamos si tiene categorias hijas y les damos baja
          await categoria.children.forEach(async (item, index) => {
            deleteFn(item);
          });



        }
        await deleteFn(node);
      }
      else {
        const deleteFn = async (categoria) => {
          delete categorias[categoria.itemData.itemID]; //Eliminamos categoria
          //Buscamos si tiene categorias hijas y les damos baja
          await categoria.children.forEach(async (item, index) => {
            deleteFn(item);
          });



        }
        await deleteFn(node);
        //if( !node.parent){
        ocultas[node.itemData.itemID] = node.itemData;
        //}


      }
      setcategoriasSeleccionadas({ ...categorias });
      setCategoriasOcultas({ ...ocultas });

    }


    //return 
  }, [isProduct, itemsSeleccionados, categoriasSeleccionadas, categoriasOcultas]);


  // const procesaCategoria(categoria){

  // }


  if (loading) {
    return (
      <React.Fragment>
        <CustomLoadIndicator message={"Cargando..."} title={"Categorias"} />
      </React.Fragment>
    );
  }

  return (
    <>
      <div>


        <div className="buttonsGroup">
          <div className="button">
            <Button icon="check"
              type="normal"
              id="button"
              onClick={async () => {
                setSaving(true);
                await guardarCategorias();
                setSaving(false);

              }} >
              <LoadIndicator className="button-indicator" visible={saving} />
              <span className="dx-button-text">{saving ? 'Guardando...' : 'Guardar'}</span>
            </Button>
          </div>
          <div className="button">
            <Button icon="refresh"
              type='normal'
              text="Recargar"
              onClick={() => updateArbol()} />
          </div>
        </div>

        <div className="form"> >
            <SeleccionaCategoriasSincronizar datasource={items} selectionChanges={async (node) => {
            await processNode(node.node);
          }} />
          {' '}
          <div className="selected-data">
            Categorias Seleccionadas
            <MuestraCategoriasSeleccionadas categorias={categoriasSeleccionadas} categoriasOcultas={categoriasOcultas} classNameExistencia='' />
          </div>
          {' '}
          <div className="selected-data">
            Categorias Ocultas
            <MuestraCategoriasSeleccionadas categorias={categoriasOcultas} classNameExistencia='categoriaConExistencia' />
          </div>
        </div>
      </div>
    </>
  );
}