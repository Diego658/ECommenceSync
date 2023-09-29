using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutomatizerInterop.Data.Interfaces
{
    public interface IFacturacionRepository
    {
        Task<Entities.Facturacion.InformacionFactura> GetInformacionFactura(string codigo, int numero, bool fullData);
    }
}
