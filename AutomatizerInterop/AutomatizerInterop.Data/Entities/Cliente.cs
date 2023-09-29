using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Entities
{
    public class ClienteAutomatizer
    {
        public string Codigo { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string NombreCompletos { get => $"{Apellidos}{(Nombres?.Length>0? " ": "")}{Nombres}"; }
        public string Identificacion { get; set; }
        public bool Estado { get; set; }
        public bool PersonaNatural { get; set; }
        public string NombreComercial { get; set; }
        public string Genero { get; set; }
        public string Email { get; set; }
        public short TipoClienteId { get; set; }
    }
}
