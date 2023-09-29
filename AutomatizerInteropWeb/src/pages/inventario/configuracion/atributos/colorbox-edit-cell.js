import React from 'react';
import ColorBox from 'devextreme-react/color-box';

export function ColorEditBoxGridCell(data) {
  return (
    <ColorBox defaultValue={data.value} onValueChanged={(value) => { data.setValue(value.value) }} />
  )
}