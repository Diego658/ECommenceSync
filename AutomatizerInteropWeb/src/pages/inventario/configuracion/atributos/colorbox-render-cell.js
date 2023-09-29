import './color-box-grid.scss';
import React from 'react';

export function ColorBoxRenderGridCell(data) {
  return (
    <div className="renderValue">
      <div className='square' style={{ backgroundColor: data.value }} ></div>
      <span>{data.value}</span>
    </div>

  )
}