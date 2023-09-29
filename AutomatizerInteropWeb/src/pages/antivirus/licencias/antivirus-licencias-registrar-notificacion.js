import React, { useState } from 'react'
import { Button, Popup } from 'devextreme-react'
import { Popover } from 'devextreme-react/popover';
import Form, { Item, Label, SimpleItem, ButtonItem, GroupItem } from 'devextreme-react/form';
import { useMemo } from 'react';
import notify from 'devextreme/ui/notify';
import { antivirusLicenciasService } from '../../../_services/antivirusLicencias.service';
import { Switch } from 'devextreme-react/switch';


const canalesNotificacion = [
    { ID: 1, Nombre: 'Teléfono' },
    { ID: 2, Nombre: 'Celular' },
    { ID: 3, Nombre: 'Email' }
];




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


export function AntivirusLicenciasRegistrarNotificacionManual({ idLicencia }) {
    const [showTooltip, setShowTooltip] = useState(false);
    const [show, setShow] = useState(false);
    const [programarAviso, setProgramarAviso] = useState(false);



    const data = useMemo(() => { return { canalNotificacion: 1, fechaNotificacion: new Date(), direccion: '', persona: '', observaciones: '', localizado: false, confirmado: false } }, []);

    const canalEditorOptions = useMemo(() => {
        return {
            items: canalesNotificacion,
            searchEnabled: true,
            displayExpr: 'Nombre',
            valueExpr: 'ID',
            value: null
        }
    }, []);

    const addressOptions = useMemo(() => {
        return {
            placeholder: 'Dirección (Canal)',
            maxLength: 100
        }
    }, []);

    const personaOptions = useMemo(() => {
        return {
            placeholder: 'Persona Notificada',
            maxLength: 250
        }
    }, []);

    const validationRules = useMemo(() => {
        return {
            canal: [
                { type: 'required', message: 'El canal de notificación es necesario.' }
            ],
            direccion: [
                { type: 'required', message: 'La dirección de notificación es necesaria.' },
            ],
            persona: [
                { type: 'required', message: 'La persona notificada es necesaria.' },
            ],
            fechaNotificacion: [
                { type: 'required', message: 'La fecha de notificación es necesaria.' }
            ],
            observaciones: [
                { type: 'required', message: 'Las observaciones son obligatorias.' },
                { type: 'stringLength', max: 250, min: 10, message: 'Longitud mínima: 10, Longitud máxima: 250' },
            ]
        }
    }, []);


    const toggleTooltip = () => {
        setShowTooltip(!showTooltip);
    }

    const toogleShow = () => setShow(!show);


    const handleSubmit = (e) => {
        notify('Registrando...', 'info', 1000);
        e.preventDefault();
        antivirusLicenciasService.registrarNotificacion(idLicencia, data).then(reponse => {
            notify('Registrado', 'success', 1000);
            data.canalNotificacion = 1;
            data.direccion = '';
            data.observaciones = '';
            data.persona = '';
            toogleShow();
        }, error => {
            notify('Error al registrar ' + error, 'error', 2500);

        });

    }


    const renderSwitchBox = () => {
        return (
            <Switch
                value={programarAviso}
                onValueChanged={value => setProgramarAviso(value.value)}
            />
        );
    }


    return (
        <>
            <div onMouseEnter={toggleTooltip} onMouseLeave={toggleTooltip}>
                <Button id="buttonRegistrarNotificacion" text="Notificación" icon='tel' type='normal' onClick={toogleShow} />
                <Popover
                    target="#buttonRegistrarNotificacion"
                    position="top"
                    animation={PopoverAnimationConfig}
                    visible={!show  && showTooltip}
                    closeOnOutsideClick={false}
                    width={300}
                >
                    Permite registrar una notificación manual sobre la caducidad de la licencia, puede establecer un recordatorio y traspasar la licencia al listado de confirmados.
                </Popover>
                <Popup
                    title="Registrar Notificación"
                    showTitle={true}
                    visible={show}
                    onHiding={toogleShow}
                    width={630}
                    height={450}
                >
                    <form onSubmit={handleSubmit} >
                        <Form

                            formData={data}
                            
                            colCount={2}
                            id='formRegistrarNotificacion'
                            showValidationSummary={false}
                            onContentReady={(e) => {
                                e.component.validate();
                            }}
                        >
                            <Item dataField="fechaNotificacion" editorType="dxDateBox" editorOptions={{ width: '100%', type: 'datetime' }} validationRules={validationRules.fechaNotificacion} />
                            <SimpleItem
                                dataField="canalNotificacion"
                                editorType='dxSelectBox'
                                editorOptions={canalEditorOptions}
                                validationRules={validationRules.canal}>
                                <Label visible={true} text="Canal Notificación" />
                            </SimpleItem>
                            <SimpleItem
                                dataField="direccion"
                                editorType="dxTextBox"
                                editorOptions={addressOptions}
                                validationRules={validationRules.direccion}
                                visible={true}
                            />
                            <SimpleItem
                                dataField="persona"
                                editorType="dxTextBox"
                                editorOptions={personaOptions}
                                validationRules={validationRules.persona}
                                visible={true}
                            />
                            <Item dataField="observaciones" colSpan={2} editorType="dxTextArea" editorOptions={{ height: 90 }} validationRules={validationRules.observaciones} />
                            <GroupItem colCount={2} colSpan={2}>
                                <SimpleItem
                                    dataField="localizado"
                                    editorType="dxSwitch"
                                    editorOptions={{}}
                                    visible={true}
                                >
                                    
                                </SimpleItem>
                                {/* <SimpleItem
                                    name="SwitchProgramarAviso"
                                    render={renderSwitchBox}
                                    editorOptions={{}}
                                    visible={true}

                                >
                                    <Label visible={true} text="Programar aviso" />
                                </SimpleItem> */}
                                <SimpleItem
                                    dataField="confirmado"
                                    editorType="dxSwitch"
                                    editorOptions={{'type':"datetime"}}
                                    visible={true}
                                >
                                    
                                </SimpleItem>
                            </GroupItem>
                            <ButtonItem horizontalAlignment='center' colSpan={2}
                                buttonOptions={{
                                    text: 'Registrar',
                                    type: 'success',
                                    useSubmitBehavior: true
                                }}
                            />
                        </Form>
                    </form>
                </Popup>
            </div>

        </>
    )
}