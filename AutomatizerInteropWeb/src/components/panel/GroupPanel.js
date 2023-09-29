import React from 'react'


export function GroupPanel(props) {
  return (
    <div className={"groupPanel"}>
      <div className="groupPanel-header">
        <span>{props.header}</span>
      </div>
      <div className='groupPanel-content'>
        {props.children}
      </div>

    </div>
  );
}