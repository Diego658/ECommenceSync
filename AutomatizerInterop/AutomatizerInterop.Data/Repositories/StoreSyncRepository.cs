using AutomatizerInterop.Data.EntityFramewrok;
using AutomatizerInterop.Data.Entities;
using AutomatizerInterop.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutomatizerInterop.Data.Repositories
{
    public class StoreSyncRepository : AutoConfigRepository, IStoreSyncRepository
    {
        public StoreSyncRepository(IInteropConfiguracionProvider configuracionProvider, IRepository genericRepository, AutomatizerInteropDbContext dbContext) : base(configuracionProvider, genericRepository, dbContext)
        {
        }

        public async Task<List<ErrorSincronizacionProducto>> GetErroresSincronizacionProductos(int idConfiguracion)
        {
            return  await QueryAsyncToList<ErrorSincronizacionProducto>("SELECT * FROM dbo.StoreSync_GetErroresSincronizacionProductos(@0)");
        }

        public async Task<List<ProductoSinImagen>> GetProductosSinImagen(int idConfiguracion)
        {
            return await QueryAsyncToList<ProductoSinImagen>(@"WITH
NumeroImagenesProductos
AS
(
	SELECT	StoreSync_Productos.ItemID, COUNT(StoreSync_ImagenesItems.BlobID) AS NumeroImagenes
	FROM            StoreSync_Productos LEFT OUTER JOIN
		StoreSync_ImagenesItems ON StoreSync_Productos.ItemID = StoreSync_ImagenesItems.ItemID
	GROUP BY StoreSync_Productos.ItemID
)
SELECT nip.ItemID , idc.Nombre, idc.Referencia, idc.ExistenciaTotal
FROM NumeroImagenesProductos nip
INNER JOIN StoreSync_ItemDatosCompletos idc 
ON idc.ItemId = nip.ItemID
WHERE idc.EmpCod = @0 AND nip.NumeroImagenes =0");
        }

        public Task UpdateCategoriesToSync(string idsCategorias, string idsCategoriasOmitir)
        {
            return ExecuteNonQueryProcAsync("[dbo].[StoreSync_UpdateCategoriesToSync]", new { IDS = idsCategorias, IDSOmitidos = idsCategoriasOmitir }  );
        }



    }
}
