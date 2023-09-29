import React from 'react';
import './css-loader.scss';

export function CssLoadingIndicator(){
  return(
    <div className="lds-roller"><div></div><div></div><div></div><div></div><div></div><div></div><div></div><div></div></div>
  );
}