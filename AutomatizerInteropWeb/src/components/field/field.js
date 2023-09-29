import React from 'react'


export function Field(props) {
  const localStyle = {};
  if (props.height) {
    localStyle.height = props.height;
  }
  return (
    <>
      <div
        style={localStyle}
        className={"fieldContainer " + (props.size === undefined ? 'x1' : props.size) + (props.label === undefined ? '' : ' withTittle')}  >
        {props.label &&
          <div className="fieldTitle" >{props.label}</div>
        }
        <div className="fieldValue" >
          {props.children}
        </div>
      </div>
    </>
  );
}


