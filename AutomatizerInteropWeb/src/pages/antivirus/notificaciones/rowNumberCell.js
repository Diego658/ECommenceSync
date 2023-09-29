import React from 'react';


export default function RowNumberCell(cellData) {
  var index = cellData.component.pageIndex() * cellData.component.pageSize() + cellData.rowIndex + 1;
  return (
    <div >
      {index}
    </div>
  );
}