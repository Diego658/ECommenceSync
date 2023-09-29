using AutomatizerInterop.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutomatizerInterop.Data.Repositories
{
    public interface IStoreSyncRepository
    {
        Task UpdateCategoriesToSync(string idsCategorias, string idsCategoriasOmitir);
        Task<List<ErrorSincronizacionProducto>> GetErroresSincronizacionProductos(int idConfiguracion);
        Task<List<ProductoSinImagen>> GetProductosSinImagen(int idConfiguracion);
    }
}
