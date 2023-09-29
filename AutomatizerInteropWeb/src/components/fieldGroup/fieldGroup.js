import React from 'react'


export function FieldGroup(props) {
  return (
    <>
      {props.title &&
        <div className='fieldGroup-Tittle'>{props.title}</div>
      }
      <div className={"fieldGroup"}>
        {props.children}
      </div>
    </>
  );
}