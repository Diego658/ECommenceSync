import React, { useState } from 'react'
import { Button, Popup,  ScrollView } from 'devextreme-react'
import { Popover } from 'devextreme-react/popover';
//import { Document } from 'react-pdf/dist/entry.webpack';
//import { Document } from 'react-pdf/dist/entry.webpack';
import { Document, Page } from 'react-pdf';
import { facturacionElectronicaService } from '../../../_services/facturacionElectronica.service';
import { useCallback } from 'react';
import { LoadingIndicator } from 'devextreme-react/bar-gauge';
import './licencias.scss'


const PopoverAnimationConfig = {
    show: {
        type: 'slide',
        from: {
            top: -100,
            opacity: 0
        },
        to: {
            top: 0,
            opacity: 1
        }
    },
    hide: {
        type: 'pop',
        from: {
            scale: 1,
            opacity: 1
        },
        to: {
            scale: 0.1,
            opacity: 0
        }
    }
};

export function AntivirusLicenciasVerFacturaAsociadaALicencia({ idLicencia, codigoFactura, numeroFactura }) {
    const [showTooltip, setShowTooltip] = useState(false);
    const [show, setShow] = useState(false);
    const [pdfBlobUrl, setPdfBlobUrl] = useState('');
    // eslint-disable-next-line no-unused-vars
    const [pdfBlob, setPdfBlob] = useState(null);
    const [pdfReady, setPdfReady] = useState(false);
    const [pageNumber, setpageNumber] = useState(1);
    const [numPages, setnumPages] = useState(1);
    const [loading, setLoading] = useState(false);
    const toggleTooltip = () => {
        setShowTooltip(!showTooltip);
    }

    const toogleShow = () => {
        const isShow = show;
        setShow(!show);

        if (isShow) {
            setPdfReady(false);
        }
        else {
            if (pdfBlobUrl === '') {
                loadPdf();
            }
            else {
                setPdfReady(true);
            }

        }

    };

    const loadPdf = useCallback(() => {
        setLoading(true);
        facturacionElectronicaService.getBlobPdf(1, codigoFactura, numeroFactura)
            .then(response => response.blob())
            .then(blob => {
                const url = URL.createObjectURL(blob);
                setPdfBlobUrl(url);
                setPdfBlob(blob);
                setPdfReady(true);
                setLoading(false);
            })
    }, [codigoFactura, numeroFactura]);


    const onDocumentLoadSuccess = (document) => {
        const { numPages } = document;
        setnumPages(numPages);
        setpageNumber(1);
    };


    const changePage = useCallback(offset => {
        setpageNumber(pageNumber + offset);
    }, [pageNumber]);

    const previousPage = () => changePage(-1);

    const nextPage = () => changePage(1);

    const descargarPdf = useCallback(() => {
        const link = document.createElement('a');
        link.href = pdfBlobUrl;
        link.setAttribute('download', `${codigoFactura}-${numeroFactura}.pdf`);
        document.body.appendChild(link);
        link.click();
        link.parentNode.removeChild(link);
    }, [codigoFactura, numeroFactura, pdfBlobUrl]);



    return (
        <div onMouseEnter={toggleTooltip} onMouseLeave={toggleTooltip}>
            <Button id="buttonVerFactura" text="Factura" icon='pdffile' type='normal' onClick={toogleShow} />
            <Popover
                target="#buttonVerFactura"
                position="top"
                animation={PopoverAnimationConfig}
                visible={!show && showTooltip}
                closeOnOutsideClick={false}
                width={300}
            >
                Ver el documento pdf de la factura asociada a la licencia.
            </Popover>

            <Popup
                title="Factura"
                showTitle={true}
                visible={show}
                onHiding={toogleShow}
                width={850}
                height={800}
            >
                <div>
                    {pdfReady &&
                        <LoadingIndicator />
                    }
                    {pdfReady &&
                        <ScrollView


                            width="100%"
                            height={700}
                        >
                            <div className="pdfViewer">
                                <div className="pdfViewerHeader">
                                    <div className="buttonItem">
                                        <Button
                                            type="normal"
                                            icon='chevronleft'
                                            disabled={pageNumber <= 1}
                                            onClick={previousPage}
                                            text="Anterior"
                                        >

                                        </Button>
                                    </div>
                                    <div className="buttonItem">
                                        <p>
                                            Página {pageNumber || (numPages ? 1 : '--')} de {numPages || '--'}
                                        </p>
                                    </div>
                                    <div className="buttonItem">
                                        <Button
                                            icon='chevronright'
                                            type="normal"
                                            disabled={pageNumber >= numPages}
                                            onClick={nextPage}
                                            text="Siguiente"
                                        >

                                        </Button>
                                    </div>

                                    <div className="buttonItem">
                                        <Button
                                            icon='download'
                                            type="normal"
                                            onClick={descargarPdf}
                                            text="Descargar"
                                        >

                                        </Button>
                                    </div>

                                </div>
                                <Document
                                    file={pdfBlobUrl}
                                    onLoadSuccess={onDocumentLoadSuccess}
                                >
                                    <Page pageNumber={pageNumber} width={800} height={750} />
                                </Document>
                            </div>
                        </ScrollView>
                    }
                </div>
            </Popup>
        </div >
    )

}