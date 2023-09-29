using AutomatizerInterop.Data.EntityFramewrok;
using AutomatizerInterop.Data.Entities.Facturacion;
using AutomatizerInterop.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutomatizerInterop.Data.Repositories
{
    public class FacturacionRepository    : AutoConfigRepository, IFacturacionRepository
    {
        public FacturacionRepository(IInteropConfiguracionProvider configuracionProvider, IRepository genericRepository, AutomatizerInteropDbContext dbContext) : base(configuracionProvider, genericRepository, dbContext)
        {
        }

        public Task<InformacionFactura> GetInformacionFactura(string codigo, int numero, bool fullData)
        {
            if (fullData)
            {
                throw new NotImplementedException();
            }
            else
            {
                return FirstAsync<InformacionFactura>(@"SELECT FacCab.TrnCod As TransaccionCodigo, FacCab.TrnNum As Numero, FacCab.CliCod As CLienteCodigo, FacCab.EstCod As Estado, FacCab.FacFec As Fecha,
                FacCab.FacSub As Subtotal, FacCab.FacImp As Impuesto, FacCab.FacDes As Descuento, FacCab.FacTot As Total, FacObs As Observaciones, FacCab.SucNum As SucursalClienteCodigo, 
                FacCab.EplCod As EmpleadoId, SerieFact As NumeroSerie, AutFact As NumeroAutorizacion
                FROM FacCab WHERE EmpCod = @0 AND TrnCOd = @1 AND TrnNum = @2", codigo, numero);
            }
            
        }
    }
}
