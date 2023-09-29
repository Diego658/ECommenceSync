import React, { useState } from 'react';
import { Button, Popup } from 'devextreme-react';
import PedidoIngresoDetalleSeries from './pedido-ingreso-detalle-series';
import { useEffect } from 'react';


export default function CellIngresarSerieProductoPrestashop({ idDetalle, seriesActuales, cantidad }) {
  const [show, setShow] = useState(false);
  const [series, setSeries] = useState([]);

  const toogleShow = () => {
    setShow(!show);
  }


  const actualizarSeries = (nuevasSeries) => {
    setSeries(nuevasSeries.split(';'));
    toogleShow();
  }

  useEffect(() => {
    if (seriesActuales != null) {
      setSeries(seriesActuales.split(';'));
    }
    else {
      setSeries([]);
    }

  }, [seriesActuales]);


  return (
    <>
      <div className='cellSeriesContainer'>
        <Button
          stylingMode='contained'
          type={cantidad === series.length? 'success': 'danger'}
          text='Editar'
          onClick={toogleShow}
        />
        {series && series.length < 4 &&
          <div className='cellSeries'>
            {series.map(item => {
              return <span key={item}>{item}</span>
            })}
          </div>
        }
        <Popup
          title="Selección de series"
          showTitle={true}
          visible={show}
          onHiding={toogleShow}
          width={640}
          height={480}
        >
          <div className='' >
            {show &&
              <PedidoIngresoDetalleSeries idDetalle={idDetalle} actualizarSeries={actualizarSeries} />
            }
          </div>
        </Popup>
      </div>
    </>
  );
}