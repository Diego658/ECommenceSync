import React from 'react';
import { Field } from '../field/field';
import { FieldGroup } from '../fieldGroup/fieldGroup';
import { Switch, CheckBox, SelectBox } from 'devextreme-react';

export default function ArbolInventarioItemPoliticas({ item, updateDataFunction, updateHtmlEditorData }) {
  return (
    <div style={{ display: 'flex', flexDirection: 'column' }}>
      <div style={styles.CheckItem}>
        <CheckBox
          text="Maneja Series"
          value={item.TieneSeries}
          onValueChanged={(value) => updateDataFunction({ ...item, TieneSeries: value.value }, 'TieneSeries')} />
      </div>
      <div style={styles.CheckItem}>
        <CheckBox
          text="Permitir venta por debajo del costo"
          value={item.PermitePerdidas}
          onValueChanged={(value) => updateDataFunction({ ...item, PermitePerdidas: value.value }, 'PermitePerdidas')} />
      </div>
      <div style={styles.CheckItem}>
        <CheckBox
          text="Se puede obsequiar al facturar"
          value={item.VentaCero}
          onValueChanged={(value) => updateDataFunction({ ...item, VentaCero: value.value }, 'VentaCero')} />
      </div>
      <div style={styles.CheckItem}>
        <CheckBox
          text="Precio Regulado"
          value={item.PrecioRegulado}
          onValueChanged={(value) => updateDataFunction({ ...item, PrecioRegulado: value.value }, 'PrecioRegulado')} />
      </div>
      <div style={styles.CheckItem}>
        <CheckBox
          text="No se vende"
          value={item.NoSevende}
          onValueChanged={(value) => updateDataFunction({ ...item, NoSevende: value.value }, 'NoSevende')} />
      </div>
      <div style={styles.CheckItem}>
        <CheckBox
          text="Maneja Garantías"
          value={item.ManejaGarantias}
          onValueChanged={(value) => updateDataFunction({ ...item, ManejaGarantias: value.value }, 'ManejaGarantias')} />
      </div>
      <div style={styles.CheckItem}>
        <CheckBox
          text="Tiene Iva"
          value={item.TieneIva}
          onValueChanged={(value) => updateDataFunction({ ...item, TieneIva: value.value }, 'TieneIva')} />
      </div>
      {item.TieneIva &&
        <div style={styles.CheckItem}>
          <SelectBox width={120} />
        </div>
      }

      <div style={styles.CheckItem}>
        <CheckBox
          text="Tiene Ice"
          value={item.TieneIce}
          onValueChanged={(value) => updateDataFunction({ ...item, TieneIce: value.value }, 'TieneIce')} />
      </div>


    </div>
  )
}



const styles = {
  "CheckItem": {
    'padding': 5
  }
}