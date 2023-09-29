using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace AutomatizerInterop.Facturacion
{
    public interface IFacturacionHelper
    {
        public DbConnection GetDbConnection();
    }
}
