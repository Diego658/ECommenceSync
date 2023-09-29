import React, { useState, useEffect, useCallback } from 'react';
import { antivirusLicenciasService } from '../../../_services/antivirusLicencias.service'
import { SelectBox, Button } from 'devextreme-react';
import { ConfiguraPlantillaEmail } from './configuraPlantillaEmail'
import './configurarPlantillaEmail.scss'
import notify from 'devextreme/ui/notify'
import { FieldGroup } from '../../../components/fieldGroup/fieldGroup';
import { Field } from '../../../components/field/field';
//import EmailEditor from 'react-email-editor'



export function ConfiguraPlantillasEmail() {
  const [plantillas, setPlantillas] = useState([]);
  const [plantilla, setPlantilla] = useState(null);
  const [cambiosPendientes, setCambiosPendientes] = useState(false);
  const [cambios, setCambios] = useState({});
  useEffect(() => {
    antivirusLicenciasService.getPlantillasNotificacion().then(datos => {
      setPlantillas(datos);
      setCambiosPendientes(false);
    });
  }, []);

  const saveDesign = useCallback(() => {
    const { json, html } = cambios;
    antivirusLicenciasService.updatePlantillaNotificacion(plantilla, json, html).then(res => {
      plantilla.contenido = JSON.stringify(json);
      setCambiosPendientes(false);
      notify("Plantilla guardada correctamente.", 'success', 2000);
    }, (error) => {
      notify("Error al guardar plantilla " + error, "error", 2000);

    });
  }, [plantilla, cambios]);


  const onUpdatedData = useCallback((json, html) => {
    setCambiosPendientes(true);
    setCambios({ json: json, html: html });
  }, []);

  return (
    <React.Fragment>
      <FieldGroup>
        <Field title='Plantilla' height={60} >
          <div>
            <div>
              <SelectBox placeholder="Seleccione una plantilla" displayExpr="nombrePlantilla" items={plantillas} value={plantilla} onValueChanged={(e) => {
                setPlantilla(e.value);
              }} />
            </div>
            <div>
              <Button
                icon='save'
                text="Guardar Plantilla"
                type="default"
                disabled={!cambiosPendientes}
                onClick={saveDesign}
                stylingMode="contained"
              />
            </div>

          </div>

        </Field>
      </FieldGroup>
      <ConfiguraPlantillaEmail plantilla={plantilla} onUpdatedData={onUpdatedData} />
    </React.Fragment>
  );


}