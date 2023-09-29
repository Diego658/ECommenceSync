using AutomatizerInterop.Data.Entities;
using AutomatizerInterop.Data.Entities.Inventario.Dtos;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;

namespace AutomatizerInterop.Data.Interfaces
{
    public interface IInventarioRepository
    {
        Task<List<ItemInventario>> GetItemsHijos(string codigoPadre, byte nivel);
        Task<CategoriaInventario> GetCategoriaInventario(int categoriaID);
        Task<dynamic> GetItemDynamic(int itemID);
        Task<List<ImagenItem>> GetImagenesItem(int itemID);
        Task SetImageItemDeleted(int itemId, long blobId);
        Task<object> AddImagenItem(int itemID, string type, long blobId);
        Task<List<Marca>> GetMarcas();
        Task<Marca> GetMarca(int marcaId);
        Task<List<Unidad>> GetUnidades();
        Task<List<Color>> GetColores();
        Task<List<Talla>> GetTallas();
        Task<List<Procedencia>> GetProcedencias();
        Task<List<ItemInventario>> GetArbolItems(bool includeItems = false);
        Task UpdateItem(ExpandoObject updatedData, int itemId);
        Task DeleteImagenItem(string type, int entityId, int imageId, int blobId);
        Task GuardarMarca(int marcaId, Marca data);
        Task<List<AtributoDto>> GetAtributos();
        Task<ItemInventario> CreateCategory(
      int padreId,
      string codigo,
      string nombre);

        Task<ItemInventario> CreateProduct(
          int padreId,
          string referencia,
          string nombre);

        Task<bool> ExistSubCategoryCode(int value, string code);

        Task<bool> ExistSubCategoryName(int value, string name);

        Task<bool> ExistProductName(string name);

        Task<bool> ExistProductReference(string reference);
    }
}