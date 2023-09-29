using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutomatizerInterop.WebApi
{
    public interface IAutomatizerConfiguracion
    {
        public int ConfiguracionId { get; set; }



    }


    public class AutomatizerConfiguracion : IAutomatizerConfiguracion
    {
        public int ConfiguracionId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

}
