import React, { useState } from 'react';
import { ClientesService } from '../../_services/clientes.service'
import { useEffect } from 'react';
import { LoadIndicator } from 'devextreme-react/load-indicator';

export function GridContactos(props) {
  const [contactos, setContactos] = useState(null);

  useEffect(() => {
    if (props.codigoCliente === undefined) {
      return;
    }
    ClientesService.getContactosCliente(1, props.codigoCliente).then(datos => {
      setContactos(datos);

    });
  }, [props.codigoCliente]);

  if (contactos === null) {
    return (
      <React.Fragment>
        <LoadIndicator id="small-indicator" height={20} width={20} />
      </React.Fragment>
    );
  }
  // const filas = contactos.map(contacto => {
  //     return `<tr><td>${contacto.tipoContacto}</td><td>${contacto.contacto}</td><td>${contacto.persona}</td></tr>`
  // });


  const filas = contactos.map((contacto) =>
    <tr key={contacto.sucursalID}>
      <td>{contacto.tipoContacto}</td>
      <td>{contacto.contacto}</td>
      <td>{contacto.persona}</td>
    </tr>
  );

  return (
    <React.Fragment>
      <table id="contacs">
        <thead>
          <tr key="contacsHeader">
            <th>Tipo</th>
            <th>Contacto</th>
            <th>Persona</th>
          </tr>
        </thead>
        <tbody>{filas}</tbody>
      </table>
    </React.Fragment>
  );
}
