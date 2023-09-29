import React from 'react'


export function SpliContainer({ leftContent, rigthContent, leftWidth = '30%', rigthWidth='70%' }) {
  return (
    <>
      <div className='content block' style={{ display: 'grid', width:'100%', height:'100%',  gridTemplateColumns: `${leftWidth} ${rigthWidth}`, gridGap:'10px' }} >
        <div>
          {leftContent}
        </div>
        <div>
          {rigthContent}
        </div>
      </div>
    </>
  );
}