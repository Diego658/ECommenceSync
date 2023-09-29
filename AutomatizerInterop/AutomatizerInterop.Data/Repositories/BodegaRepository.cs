using AutomatizerInterop.Data.EntityFramewrok;
using AutomatizerInterop.Data.Entities;
using AutomatizerInterop.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutomatizerInterop.Data.Repositories
{
    public class BodegaRepository : AutoConfigRepository, IBodegaRepository
    {
        public BodegaRepository(IInteropConfiguracionProvider configuracionProvider, IRepository genericRepository, AutomatizerInteropDbContext dbContext) : base(configuracionProvider, genericRepository, dbContext)
        {
        }

        public Task<List<Bodega>> GetBodegas()
        {
            throw new NotImplementedException();
        }

        public Task<List<BodegaItem>> GetBodegasItem(int itemID)
        {
            throw new NotImplementedException();
        }

        public async Task<List<BodegaItemDetallado>> GetBodegasItemDetallado(int itemID)
        {
            return await QueryAsyncToList<BodegaItemDetallado>(@"SELECT CAST(CASE  WHEN ItmBod.BodCod IS NULL THEN '0' ELSE 1 END as bit)  As Seleccionada,ItmMae.Secuencial As ItemID, Bodega.BodCod As CodigoBodega, ItmBod.ItmCod As CodigoItem, Bodega.BodDes as NombreBodega , ItmBod.ItmExs As Existencia, ItmBod.ItmExsMax As ExistenciaMaxima, ItmBod.ItmExsMin As ExistenciaMinima, ItmBod.ItmCos As CostoActual
            FROM ItmBod
            INNER JOIN ItmMae
            ON ItmMae.EmpCod = ItmBod.EmpCod AND ItmMae.ItmCod = ItmBod.ItmCod
            RIGHT JOIN Bodega
            ON Bodega.EmpCod = ItmBod.EmpCod AND Bodega.BodCod = ItmBod.BodCod
            WHERE ItmBod.EmpCod = @0 AND ItmMae.Secuencial = @1", itemID);
        }
    }
}
