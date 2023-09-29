import React from 'react'
import { useState } from 'react';
import { LoadIndicator, Popover, DateBox, Switch } from 'devextreme-react';
import { antivirusLicenciasService } from '../../../_services/antivirusLicencias.service';

const _ULTIMA_NOTIFICACION_ = "notificacionId";

export function AntivirusLicenciaUltimaNotificacionInfo({ notificacionId }) {
    const [loading, setLoading] = useState(false);
    const [visible, setVisible] = useState(false);
    const [data, setData] = useState({});


    const cargarDatos = () => {
        setLoading(true);
        antivirusLicenciasService.getInformacionNotificacion(notificacionId)
            .then(response => {
                setData(response);
                setLoading(false);
            }, error => {
                setLoading(false);
            })
    };

    const onHiding = () => {
        setVisible(false);
    }
    const onClick = () => {
        setVisible(true);
        cargarDatos();
    }

    const idDiv = `${_ULTIMA_NOTIFICACION_}${notificacionId}`;
    return (
        <>
            <div id={idDiv}>
                <div onClick={onClick} style={{ 'cursor': 'pointer' }} className="cellUltimaNotificacion" >Ver</div>
                <Popover
                    showTitle={true}
                    title="Información notificación"
                    target={`#${idDiv}`}
                    position="left"
                    width={400}
                    height={300}
                    visible={visible}
                    onHiding={onHiding}
                >
                    <div>
                        <div style={{ 'margin': 'auto' }}>
                            {loading &&
                                <LoadIndicator style={{ 'margin': 'auto' }} />
                            }
                            {!loading &&
                                <>

                                    <div className="dx-field">
                                        <div className="dx-field-label">Fecha Notificación</div>
                                        <div className="dx-field-value">
                                            <DateBox
                                                value={data.fechaNotificacion}
                                                type="datetime"
                                                useMaskBehavior={true}
                                                displayFormat="dd/MM/yyyy HH:mm:ss"
                                            />
                                        </div>
                                    </div>
                                    <div className="dx-field">
                                        <div className="dx-field-label">Canal Notificación</div>
                                        <div className="dx-field-value">
                                            {data.canalNotificacion}
                                        </div>
                                    </div>
                                    <div className="dx-field">
                                        <div className="dx-field-label">Persona Notificada</div>
                                        <div className="dx-field-value">
                                            {data.personaNotificada}
                                        </div>
                                    </div>
                                    <div className="dx-field">
                                        <div className="dx-field-label">Dirección Notificacion</div>
                                        <div className="dx-field-value">
                                            {data.direccionNotificacion}
                                        </div>
                                    </div>
                                    <div className="dx-field">
                                        <div className="dx-field-label">Observaciones</div>
                                        <div className="dx-field-value">
                                            <p>{data.observaciones }</p>
                                        </div>
                                    </div>
                                    <div className="dx-field">
                                        <div className="dx-field-label">Localizado</div>
                                        <div className="dx-field-value">
                                            <Switch value={data.localizado}/>
                                        </div>
                                    </div>
                                    <div className="dx-field">
                                        <div className="dx-field-label">Confirmado</div>
                                        <div className="dx-field-value">
                                            <Switch value={data.xonfirmado }/>
                                        </div>
                                    </div>
                                </>
                            }
                        </div>
                    </div>
                </Popover>
            </div>
        </>
    );
}
