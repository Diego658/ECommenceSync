import React from 'react'
import SunEditor from 'suneditor-react';
import 'suneditor/dist/css/suneditor.min.css'; // Import Sun Editor's CSS File

export function CustomSunHtmlEditor({ initialContents, updateFunction, showToolbar }) {
  return (
    <SunEditor  showToolbar={true} enableToolbar={showToolbar} setContents={initialContents} onChange={updateFunction} />
  )
}