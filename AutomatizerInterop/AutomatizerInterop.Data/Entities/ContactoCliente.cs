using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Entities
{
    public  class ContactoCliente
    {
        public string ClienteCodigo { get; set; }
        public string SucursalID { get; set; }
        public byte SucursalNumero { get; set; }
        public byte TipoContactoID { get; set; }
        public string TipoContacto { get; set; }
        public string Contacto { get; set; }
        public string Persona { get; set; }
    }
}
