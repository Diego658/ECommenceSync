import React from 'react';

export  function ArbolInventarioAtributoInfo(item) {
  return (
    <React.Fragment>
      <div>{item.valor}</div>
      {item.color &&
        <div className='square' style={{ backgroundColor: item.color }} ></div>
      }
    </React.Fragment>
  );
}
