using AutomatizerInterop.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Entities
{
    public class ConfiguracionProgramaFacturacionElectronica
    {
        public int Id { get; set; }
        public string ServidorSQLServer { get; set; }

        public string UsuarioSQLServer { get; set; }

        public string ClaveUsuarioSQLServer { get; set; }

        public string CatalogoSqlServer { get; set; }

        public string CodigoEmpresa { get; set; }
        public int TiempoBusqueda { get; set; }

        public string PathPDF { get; set; }

        public string PathXmlGenerados { get; set; }

        public string PathXmlFirmados { get; set; }

        public string PathXmlAutorizados { get; set; }

        public string CorreoEnvioMails { get; set; }

        public string UsuarioEnvioMails { get; set; }

        public string ClaveEnvioMails { get; set; }
        public string SmtpEnvioMails { get; set; }

        public string PuertoEnvioMails { get; set; }

        public byte UsaSslEnvioMails { get; set; }

        public byte EnviaMails { get; set; }

        public string UsuarioEmpresa { get; set; }

        public string NombreConfiguracion { get; set; }

    }
}
