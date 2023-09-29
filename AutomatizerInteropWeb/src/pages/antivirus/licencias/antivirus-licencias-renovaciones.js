import React, { useState, useRef } from 'react'
import RangeSelector, { Scale, Behavior, SliderMarker, MinorTick } from 'devextreme-react/range-selector';
import { useEffect } from 'react';
import { FieldGroup } from '../../../components/fieldGroup/fieldGroup';
import { Field } from '../../../components/field/field';
import { SelectBox, TextBox } from 'devextreme-react';
import { CssLoadingIndicator } from '../../../components/cssLoader/css-loader';
import { antivirusLicenciasService } from '../../../_services/antivirusLicencias.service';
import DataGrid, { Column, SearchPanel, RequiredRule, HeaderFilter, Editing, Popup as GridPopup, Form, Lookup, Export } from 'devextreme-react/data-grid';
import notify from 'devextreme/ui/notify';
import PivotGridDataSource from 'devextreme/ui/pivot_grid/data_source';
import PivotGrid, {
  FieldChooser,
  StateStoring
} from 'devextreme-react/pivot-grid';
import Chart, {
  AdaptiveLayout,
  CommonSeriesSettings,
  Size,
  Tooltip,

} from 'devextreme-react/chart';


export function AntivirusLicenciasRenovaciones() {
  const [licencias, setLicencias] = useState([]);


  useEffect(() => {

  }, []);

  useEffect(() => {
    setLicencias([]);
  }, []);

  return (
    <>
      <FieldGroup>
        <Field height={400} size='x3'>
          {Renovaciones()}
        </Field>
        <Field height={800} size='x3'>
          {CostosRenovaciones()}
        </Field>
      </FieldGroup>
    </>
  );
}



function Renovaciones() {
  const [loading, setLoading] = useState(true);
  const [loadingData, setLoadingData] = useState(false);
  const [years, setYears] = useState([]);
  const [year, setYear] = useState(0);
  const [months, setMonths] = useState([]);
  const [month, setMonth] = useState(0);
  const [renovaciones, setRenovaciones] = useState([]);

  const cargarAnios = async () => {
    var y = await antivirusLicenciasService.getAniosRenovaciones();
    setYears(y);
    setYear((new Date()).getFullYear());
  };


  const cargarMeses = async (y) => {
    setLoading(true);
    const m = await antivirusLicenciasService.getMesesRenovaciones(y);
    setMonths(m);
    const currentYear = (new Date()).getFullYear();
    const currentMonth = (new Date()).getMonth() + 1;
    const tmp = m.filter(x => x.Mes === currentMonth);
    if (y === currentYear && tmp.length > 0) {
      setMonth(tmp[0]);
    }
    else {
      setMonth(m[0]);
    }
    setLoading(false);
  };


  const cargarDatos = async (y, m) => {
    if (y <= 0) {
      setRenovaciones([]);
      return;
    }
    if (!m) {
      setRenovaciones([]);
      return;
    }
    setLoadingData(true);
    try {
      var data = await antivirusLicenciasService.getRenovaciones(y, m.Mes);
      setRenovaciones(data);
    } catch (error) {
      notify(error, 'error', 3500);
    }
    setLoadingData(false);
  }


  useEffect(() => {
    if (year) {
      cargarMeses(year);
    }
  }, [year])

  useEffect(() => {
    cargarDatos(year, month);
  }, [year, month])

  useEffect(() => {
    cargarAnios();
  }, []);

  if (loading) {
    return <CssLoadingIndicator />
  }

  return (
    <>
      <FieldGroup title='Renovaciones'>
        <Field label='Año' size='s1' >
          <SelectBox
            items={years}
            value={year}
            valueExpr='Anio'
            displayExpr='Anio'
            itemRender={yearItemRender}
            onValueChanged={(e) => setYear(e.value)} />
        </Field>
        <Field label='Meses' size='s2' >
          <SelectBox
            items={months}
            displayExpr='NombreMes'
            value={month}
            onValueChanged={(e) => setMonth(e.value)}
          >
          </SelectBox>
        </Field>
      </FieldGroup>
      <FieldGroup>
        <Field height={300}>
          {!loadingData &&
            <DataGrid
              height={250}
              width={500}
              dataSource={renovaciones}
              showBorders={true}
            >
              <Column dataField='RenovacionId' caption='Ren.' width={80} />
              <Column dataField='VentaId' caption='Ant.' width={80} />
              <Column dataField='Producto' width={350} />
              <Column dataField='Cliente' width={200} />
              <Column dataField='Factura' width={150} />
            </DataGrid>
          }
          {loadingData &&
            <CssLoadingIndicator />
          }
        </Field>
      </FieldGroup>
    </>
  )
}

function yearFieldRender(data) {
  return (
    <div className="estadisticas-year-item">
      <div className='estadisticas-year-numeroRenovaciones'>
        {data.NumeroRenovaciones}
      </div>
      <div className="estadisticas-year-name">
        <TextBox
          defaultValue={data && data.Anio}
          readOnly={true} />
      </div>

    </div>
  );
}


function yearItemRender(data) {
  return (
    <div className="estadisticas-year-item">
      <div className='estadisticas-year-numeroRenovaciones'>
        {data.NumeroRenovaciones}
      </div>
      <div className="estadisticas-year-name">
        {data.Anio}
      </div>


    </div>
  );
}


function CostosRenovaciones() {
  const [loading, setLoading] = useState(true);
  const [loadingData, setLoadingData] = useState(false);
  const [years, setYears] = useState([]);
  const [year, setYear] = useState(0);
  const [ventas, setventas] = useState([]);
  const pivotGrid = useRef(null);
  const chart = useRef(null);

  const cargarAnios = async () => {
    var y = await antivirusLicenciasService.getAniosRenovaciones();
    setYears(y);
    setYear((new Date()).getFullYear());
    setLoading(false);
  };




  const cargarDatos = async (y) => {
    if (y <= 0) {
      setventas([]);
      return;
    }
    setLoadingData(true);
    try {
      var data = await antivirusLicenciasService.getRenovacionesCostos(y);
      setventas(data);
      if (pivotGrid.current) {
        pivotGrid.current.bindChart(chart.current, {
          alternateDataFields: false
        });
      }
    } catch (error) {
      notify(error, 'error', 3500);
    }
    setLoadingData(false);
  }


  const dataSource = new PivotGridDataSource({
    fields: [{
      caption: 'Producto',
      width: 120,
      dataField: 'Producto',
      area: 'row',
      sortBySummaryField: 'ValorVenta'
    }, {
      dataField: 'FechaVenta',
      dataType: 'date',
      area: 'column'
    }, {
      groupName: 'date',
      groupInterval: 'month',
      visible: true
    }, {
      caption: 'Venta',
      dataField: 'ValorVenta',
      dataType: 'number',
      summaryType: 'sum',
      format: 'currency',
      area: 'data'
    },    {
      caption: 'Costo',
      dataField: 'CostoVenta',
      dataType: 'number',
      summaryType: 'sum',
      format: 'currency',
      area: 'data'
    }
      , {
      summaryType: 'count',
      area: 'data'
    }],
    store: ventas
  });



  useEffect(() => {
    if (year) {
      cargarDatos(year);
    }
  }, [year])


  useEffect(() => {
    cargarAnios();
  }, []);

  if (loading) {
    return <CssLoadingIndicator />
  }

  return (
    <>
      <FieldGroup title='Montos Ventas'>
        <Field label='Año' size='s1' >
          <SelectBox
            items={years}
            value={year}
            valueExpr='Anio'
            displayExpr='Anio'
            itemRender={yearItemRender}
            onValueChanged={(e) => setYear(e.value)} />
        </Field>
      </FieldGroup>
      <FieldGroup>
        <Field height={400} size='x3'>
          {!loadingData &&
            <PivotGrid
              id="pivotgrid"
              dataSource={dataSource}
              allowSortingBySummary={true}
              allowFiltering={true}
              showBorders={true}
              showColumnTotals={false}
              showColumnGrandTotals={false}
              showRowTotals={false}
              showRowGrandTotals={false}
              ref={pivotGrid}
              height={700}
            >
              <FieldChooser enabled={true} height={400} />
              <StateStoring enabled={true} storageKey='pivotVentasAntivirus' />
            </PivotGrid>
          }
          {loadingData &&
            <CssLoadingIndicator />
          }
        </Field>
      </FieldGroup>
    </>
  );
}