using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Entities.Stores.Prestashop
{
    public class CambioPendiente
    {
        public string Destino { get; set; }
        public string ClaveExterna { get; set; }
        public bool TieneError { get; set; }
        public string Error { get; set; }
        public string StackTrace { get; set; }
    }


    

}
