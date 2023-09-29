using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Entities.Inventario.Dtos
{
    public class AtributoValorDto
    {
        public int Id { get; set; }
        public int AtributoId { get; set; }
        public string Valor { get; set; }
        public int? Orden { get; set; }
        public string Color { get; set; }
    }
}
