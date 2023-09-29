import React from 'react';
export default function ConfiguracionInfo(item) {
  return (
    <React.Fragment>
      <div>
        <span>{item.nombreConfiguracion}</span>
      </div>
    </React.Fragment>
  );
}