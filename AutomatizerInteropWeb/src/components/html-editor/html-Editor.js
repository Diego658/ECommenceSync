import React, { useState } from 'react';
import HtmlEditor, { Toolbar, Item, MediaResizing } from 'devextreme-react/html-editor';
import { useCallback } from 'react';


const sizeValues = ['8pt', '10pt', '12pt', '14pt', '18pt', '24pt', '36pt'];
const fontValues = ['Arial', 'Courier New', 'Georgia', 'Impact', 'Lucida Console', 'Tahoma', 'Times New Roman', 'Verdana'];
const headerValues = [false, 1, 2, 3, 4, 5, 6];
const enabled = {
  enabled: true
};


export function CustomHtmlEditor({ markup, height, useToolBar, updateMarkupFunction, toolbarButtonOptions }) {
  return (
    <React.Fragment>
      <HtmlEditor
        style={{ margin: 'auto' }}
        className="dx-swatch-dark"
        onValueChanged={(value) => {
          updateMarkupFunction(value.value);
        }}
        value={markup}
        mediaResizing={enabled}
        height={height === undefined ? "400px" : height}
        width={"90%"}
      >

            <MediaResizing enabled={true} />
            <Toolbar multiline>
              <Item name="undo" />
              <Item name="redo" />
              <Item name="separator" />
              <Item
                name="size"
                acceptedValues={sizeValues}
              />
              <Item
                name="font"
                acceptedValues={fontValues}
              />
              <Item name="separator" />
              <Item name="bold" />
              <Item name="italic" />
              <Item name="strike" />
              <Item name="underline" />
              <Item name="separator" />
              <Item name="alignLeft" />
              <Item name="alignCenter" />
              <Item name="alignRight" />
              <Item name="alignJustify" />
              <Item name="separator" />
              <Item
                name="header"
                acceptedValues={headerValues}
              />
              <Item name="separator" />
              <Item name="orderedList" />
              <Item name="bulletList" />
              <Item name="separator" />
              <Item name="color" />
              <Item name="background" />
              <Item name="separator" />
              <Item name="link" />
              <Item name="image" />
              <Item name="video" />
              <Item name="separator" />
              <Item name="clear" />
              <Item name="codeBlock" />
              <Item name="blockquote" />
              <Item
                widget="dxButton"
                options={toolbarButtonOptions}
              />
            </Toolbar>
      
      </HtmlEditor>
    </React.Fragment>
  );
}