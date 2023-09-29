import React, { useCallback, useState } from 'react';
import { Popup } from 'devextreme-react';
import { CustomHtmlEditor } from './html-Editor';
import { useMemo } from 'react';

export function HtmlEditorWithPopUp({ markup, height, useToolBar, updateMarkupFunction, popupTitle = "Texto" }) {
  const [pantallaCompleta, setpantallaCompleta] = useState(false);

  const tooglePantallaCompleta = useCallback(() => {
    setpantallaCompleta(!pantallaCompleta);
  }, [pantallaCompleta]);

  const toolbarButtonOptions = useCallback({
    text: 'Pantalla Completa',
    stylingMode: 'text',
    onClick: tooglePantallaCompleta
  }, []);

  const htmlEditor = useMemo(() => {
    return (<CustomHtmlEditor markup={markup} height={height} useToolBar={useToolBar} updateMarkupFunction={updateMarkupFunction} toolbarButtonOptions={toolbarButtonOptions} />)
  }, [height, markup, toolbarButtonOptions, updateMarkupFunction, useToolBar])


  return (
    <>
      {!pantallaCompleta &&
        htmlEditor
      }
      <Popup
        title={popupTitle}
        showTitle={true}
        visible={pantallaCompleta}
        onHiding={tooglePantallaCompleta}
        width={'70%'}
        height={'90%'}
      >
      {htmlEditor}
      </Popup>
    </>
  )

  // return (
  //   <>
  //     {!pantallaCompleta &&
  //       <CustomHtmlEditor markup={markup} height={height} useToolBar={useToolBar} updateMarkupFunction={updateMarkupFunction} toolbarButtonOptions={toolbarButtonOptions} />
  //     }

  //     <Popup
  //       title={popupTitle}
  //       showTitle={true}
  //       visible={pantallaCompleta}
  //       onHiding={tooglePantallaCompleta}
  //       width={'70%'}
  //       height={'90%'}
  //     >
  //       {pantallaCompleta &&
  //         <CustomHtmlEditor markup={markup} height={'100%'} useToolBar={useToolBar} updateMarkupFunction={updateMarkupFunction} />
  //       }
  //     </Popup>
  //   </>
  // )
}