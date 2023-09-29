import React from 'react';

export default function GridNumerador(cellData) {
  return (
    <div className="current-value">
      {cellData.row.rowIndex + 1}
    </div>
  );
}