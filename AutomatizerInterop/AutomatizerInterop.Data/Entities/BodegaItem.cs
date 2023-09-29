using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Entities
{
    public class BodegaItem
    {
        public int ItemID { get; set; }
        public string CodigoBodega { get; set; }
        public string CodigoItem { get; set; }
        public decimal Existencia { get; set; }
        public decimal ExistenciaMinima { get; set; }
        public decimal ExistenciaMaxima { get; set; }
        public decimal CostoActual { get; set; }
        public bool Seleccionada { get; set; }
    }

    public class BodegaItemDetallado:BodegaItem
    {
        public string NombreBodega { get; set; }
    }


}
