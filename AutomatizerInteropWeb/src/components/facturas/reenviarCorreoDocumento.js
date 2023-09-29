import React from 'react';
import { FieldGroup } from '../fieldGroup/fieldGroup';
import { Field } from '../field/field';
import { TextArea, Button } from 'devextreme-react';
import { useState } from 'react';
import { facturacionElectronicaService } from '../../_services/facturacionElectronica.service';
import UserSesion from '../../utils/userSesion';
import notify from 'devextreme/ui/notify';

export default function FacturacionElectronicaReenviarEmail({ idDocumento }) {
  const [emails, setEmails] = useState('');
  const [enviado, setEnviado] = useState(false);
  return (
    <>
      <FieldGroup>
        <Field label='Emails (Use "," para separar distintas direcciones)' height={80} size='x2' >
          <TextArea
            value={emails}
            onValueChanged={(e) => setEmails(e.value)}

          >

          </TextArea>
        </Field>
      </FieldGroup>
      <FieldGroup>
        <div>
          <div>
            <Button
              text='Reenviar Emails'
              disabled={!emails || emails.trim().length === 0 || enviado}
              onClick={async () => {
                await facturacionElectronicaService.reenviarEmail(UserSesion.getConfiguracion().idConfiguracionPrograma, idDocumento, emails.trim());
                setEnviado(true);
                notify('El documento se puso en cola para reintentarse.', 'success', 2000);
              }}
            />
          </div>
        </div>
      </FieldGroup>
    </>
  );
}