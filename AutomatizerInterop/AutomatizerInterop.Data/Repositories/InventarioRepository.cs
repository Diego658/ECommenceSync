using AutomatizerInterop.Data.EntityFramewrok;
using AutomatizerInterop.Data.Entities;
using AutomatizerInterop.Data.Entities.Inventario.Dtos;
using AutomatizerInterop.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AutomatizerInterop.Data.Repositories
{
    public class InventarioRepository : AutoConfigRepository, IInventarioRepository
    {
        public InventarioRepository(IInteropConfiguracionProvider configuracionProvider, IRepository genericRepository, AutomatizerInteropDbContext dbContext)
            : base(configuracionProvider, genericRepository, dbContext)
        {
        }


        public async Task<List<ItemInventario>> GetArbolItems(bool includeItems = false)
        {
            var filtro = includeItems ? "G,I" : "G";
            return await QueryAsyncToList<ItemInventario>(@"select * from dbo.GetArbolInventario(@0, @1) ORDER BY Codigo", filtro);
        }

        public async Task<List<ItemInventario>> GetItemsHijos(string codigoPadre, byte nivel)
        {
            var hijos = new List<ItemInventario>(20);
            var reader = await QueryAsync<ItemInventario>(@"SELECT * FROM StoreSync_GetItemsHijos(@0, @1, @2) ORDER BY Codigo", codigoPadre, nivel);
            while (await reader.ReadAsync())
            {
                hijos.Add(reader.Poco);
            }
            return hijos;
        }



        public async Task<CategoriaInventario> GetCategoriaInventario(int categoriaID)
        {
            var categoria = await FirstAsync<CategoriaInventario>(@"select Codigo = ItmCod, Tipo = ItmTip, Nombre = ItmDsc, Nivel = ItmNiv, Referencia = ItmCodVen, ItemID = itmmae.Secuencial, CodigoPadre = '', ExistenciaTotal = CAST(0 AS DECIMAL(14,2))
            from  ItmMae where secuencial = @1", categoriaID);
            var datosAdicionalesReader = await QueryAsync<PaginaWebDatosAdicionalesItem>(@"SELECT        @1 AS ItemID, StoreSync_DatosAdicionalesFieldTypes.Id AS FieldTypeID,StoreSync_DatosAdicionalesFieldTypes.FielType,  ISNULL(StoreSync_DatosAdicionalesWebItems.Contenido, '') AS Contenido
            FROM StoreSync_DatosAdicionalesFieldTypes LEFT  JOIN
                         StoreSync_DatosAdicionalesWebItems ON StoreSync_DatosAdicionalesFieldTypes.Id = StoreSync_DatosAdicionalesWebItems.FieldTypeID AND   (StoreSync_DatosAdicionalesWebItems.ItemID = @1)", categoriaID);
            categoria.DatosWeb = new List<PaginaWebDatosAdicionalesItem>();
            categoria.Imagenes = new List<dynamic>();

            while (await datosAdicionalesReader.ReadAsync())
            {
                categoria.DatosWeb.Add(datosAdicionalesReader.Poco);
            }
            datosAdicionalesReader.Dispose();




            var imagenesReader = await QueryAsync<dynamic>(@"SELECT        ItemID, Type, BlobID, FechaModificacion, ImageAlt
            FROM StoreSync_ImagenesItems
            WHERE (ItemID = @1)", categoriaID);

            while (await imagenesReader.ReadAsync())
            {
                categoria.Imagenes.Add(imagenesReader.Poco);
            }

            imagenesReader.Dispose();
            return categoria;

        }


        public async Task<dynamic> GetItemDynamic(int itemID)
        {
            return await FirstAsync<dynamic>(@"SELECT * FROM StoreSync_ItemDatosCompletos WHERE (ItemID = @1)", itemID);
        }



        public async Task<List<ImagenItem>> GetImagenesItem(int itemID)
        {
            var imageReader = await QueryAsync<ImagenItem>(@"SELECT Id, ItemID, Type, BlobID, FechaModificacion, ImageAlt, Operacion
            FROM StoreSync_ImagenesItems
            WHERE (ItemID = @1)", itemID);

            var imagenes = new List<ImagenItem>(3);
            while (await imageReader.ReadAsync())
            {
                imagenes.Add(imageReader.Poco);
            }

            return imagenes;
        }

        public async Task SetImageItemDeleted(int itemId, long blobId)
        {
            await ExecuteAsync("UPDATE StoreSync_ImagenesItems SET Operacion = 'delete', FechaModificacion = SYSDATETIMEOFFSET() WHERE ItemID = @2 AND BlobID = @1",
                blobId, itemId);

        }

        public async Task<object> AddImagenItem(int itemID, string type, long blobId)
        {

            using var database = GetSQLDatabase();
            if (type == "MARCA")
            {
                await database.ExecuteAsync("UPDATE StoreSync_Marcas SET FechaModificacionLogo = SYSDATETIMEOFFSET(), LogoBlobId = @1 WHERE ID = @0", itemID, blobId);
                return new { blobID = blobId };
            }
            else
            {

                var image = await database.FirstOrDefaultAsync<ImagenItem>("SELECT * FROM StoreSync_ImagenesItems WHERE ItemID = @0 AND Type = @1 AND BlobID = @2 AND Operacion <> 'delete'", itemID, type, blobId);
                if (image != null)
                {
                    return image;
                }
                else
                {
                    await database.ExecuteAsync(@"INSERT INTO StoreSync_ImagenesItems
                         (ItemID, Type, BlobID, FechaModificacion, ImageAlt, Operacion)
                       VALUES       (@0, @1, @2, SYSDATETIMEOFFSET(), '', 'Add')", itemID, type, blobId);

                    if (type == "category_default")
                    {
                        await database.ExecuteAsync("UPDATE StoreSync_Categorias SET FechaModificacionImage = SYSDATETIMEOFFSET() WHERE CategoriaID = @0", itemID);
                    }
                    return await database.FirstAsync<ImagenItem>("SELECT * FROM StoreSync_ImagenesItems WHERE ItemID = @0 AND Type = @1 AND BlobID = @2", itemID, type, blobId);
                }
            }


        }

        public async Task<List<Marca>> GetMarcas()
        {
            var marcasReader = await QueryAsync<Marca>(@"SELECT ssm.ID, Codigo = MarCod, Nombre = MarDsc 
            FROM Marcas INNER JOIN StoreSync_Marcas ssm ON Marcas.EmpCod = ssm.EmpCod AND Marcas.MarCod = ssm.MarcaID
            WHERE Marcas.EmpCod = @0");

            var marcas = new List<Marca>();

            while (await marcasReader.ReadAsync())
            {
                marcas.Add(marcasReader.Poco);
            }
            marcasReader.Dispose();
            return marcas;
        }

        public async Task<Marca> GetMarca(int marcaId)
        {
            var marca = await SingleAsync<Marca>(@"SELECT ID, Marcas.MarCod AS Codigo, Marcas.MarDsc AS Nombre, StoreSync_Marcas.DescriptionShort, StoreSync_Marcas.Description, StoreSync_Marcas.LogoBlobId
            FROM Marcas INNER JOIN
                         StoreSync_Marcas ON Marcas.EmpCod = StoreSync_Marcas.EmpCod AND Marcas.MarCod = StoreSync_Marcas.MarcaID
            WHERE (ID = @1)", marcaId);


            return marca;
        }


        public Task<List<Unidad>> GetUnidades()
        {
            return QueryAsyncToList<Unidad>(@"SELECT Codigo = UniCod, Nombre = UniDes 
            FROM Unidades
            WHERE(EmpCod = @0)");
        }

        public Task<List<Color>> GetColores()
        {
            return QueryAsyncToList<Color>(@"SELECT Codigo = IdColor, Nombre = Color 
            FROM ItmColores
            WHERE(EmpCod = @0)");
        }

        public Task<List<Talla>> GetTallas()
        {
            return QueryAsyncToList<Talla>(@"SELECT Codigo = IdTalla, Nombre = Talla
            FROM ItmTallas    
            WHERE EmpCod = @0
            ORDER BY Orden");
        }

        public Task<List<Procedencia>> GetProcedencias()
        {
            return QueryAsyncToList<Procedencia>(@"select Codigo = ProvCod, Nombre = Proveniencia 
            from ProvenienciasItm
            Where EmpCod = @0");
        }


        static Dictionary<string, string> _itmMaePropertiesVsFields = new Dictionary<string, string>
        {
            { "Nombre", "ItmMae.ItmDsc" },
            { "Observaciones", "ItmMae.Obs" },
            {"UnidadID" , "ItmMae.ItmUA"},
            {"CodigosProveedor" , "ItmMae.CodigosProveedor"},
            {"Contiene" , "ItmMae.Contiene"},
            {"IdTalla" , "ItmMae.IdTalla"},
            {"MarcaID" , "ItmMae.Marcod"},
            {"Modelo" , "ItmMae.Modelo"},
            {"NroParte" , "ItmMae.NroParte"},
            {"Peso" , "ItmMae.Utilidad"},
            {"Original" , "ItmMae.Original"},
            {"ProvCod" , "ItmMae.ProvCod"},
            {"TarProd" , "ItmMae.TarProd"}
        };

        static Dictionary<string, string> _storeSyncProduxctosPropertiesVsFields = new Dictionary<string, string>
        {
            {"HasOptions", "StoreSync_Productos.TieneCombinaciones" }
        };




        static Dictionary<string, byte> _datosAdicionalesWebPropertiesVsFields = new Dictionary<string, byte>
        {
            {"Description-Html", 1},
            { "Description-Short-Html", 3 },
            { "Link-Rewrite", 5 }
        };


        public async Task UpdateItem(ExpandoObject updatedData, int itemId)
        {
            var sqlFields = "";

            using (var sqlDb = new SqlConnection(GetConnectionString))
            {
                await sqlDb.OpenAsync();
                var tran = sqlDb.BeginTransaction();
                var cmd = (SqlCommand)sqlDb.CreateCommand();
                try
                {
                    foreach (var field in updatedData)
                    {
                        if (_itmMaePropertiesVsFields.ContainsKey(field.Key))
                        {
                            sqlFields += $"{_itmMaePropertiesVsFields[field.Key]} = @{field.Key},";
                            var jElement = (System.Text.Json.JsonElement)field.Value;
                            var param = cmd.CreateParameter();
                            param.ParameterName = field.Key;
                            if (jElement.ValueKind == System.Text.Json.JsonValueKind.String) param.Value = jElement.GetString();
                            if (jElement.ValueKind == System.Text.Json.JsonValueKind.Number) param.Value = jElement.GetDouble();
                            if (jElement.ValueKind == System.Text.Json.JsonValueKind.True) param.Value = jElement.GetBoolean();
                            if (jElement.ValueKind == System.Text.Json.JsonValueKind.False) param.Value = jElement.GetBoolean();
                            if (param.Value != null) cmd.Parameters.Add(param);
                        }
                        if (_datosAdicionalesWebPropertiesVsFields.ContainsKey(field.Key))
                        {
                            var jElement = (System.Text.Json.JsonElement)field.Value;
                            var cmd2 = sqlDb.CreateCommand();
                            cmd2.Transaction = tran;
                            cmd2.Parameters.AddWithValue("ItemID", itemId);
                            cmd2.Parameters.AddWithValue("FieldTypeID", _datosAdicionalesWebPropertiesVsFields[field.Key]);
                            cmd2.Parameters.AddWithValue("Contenido", jElement.GetString());
                            cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd2.CommandText = "StoreSync_AddDatoAdicionalWebItems";
                            await cmd2.ExecuteNonQueryAsync();
                        }
                        if (_storeSyncProduxctosPropertiesVsFields.ContainsKey(field.Key))
                        {
                            var jElement = (System.Text.Json.JsonElement)field.Value;
                            var cmd2 = sqlDb.CreateCommand();
                            cmd2.Transaction = tran;
                            cmd2.Parameters.AddWithValue("ItemID", itemId);
                            cmd2.Parameters.AddWithValue("TieneCombinaciones", jElement.GetBoolean());
                            cmd2.CommandType = System.Data.CommandType.Text;
                            cmd2.CommandText = "UPDATE StoreSync_Productos SET TieneCombinaciones = @TieneCombinaciones WHERE ItemID = @ItemID";
                            await cmd2.ExecuteNonQueryAsync();
                        }
                        if (field.Key == "Tags")
                        {
                            var jElement = (System.Text.Json.JsonElement)field.Value;
                            var cmd2 = sqlDb.CreateCommand();
                            cmd2.Transaction = tran;

                            cmd2.Parameters.AddWithValue("ItemId", itemId);
                            cmd2.Parameters.AddWithValue("Tags", jElement.GetString().ToUpperInvariant());
                            cmd2.Parameters.AddWithValue("EmpCod", this.CodigoEmpresa);
                            cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd2.CommandText = "StoreSync_UpdateProductTags";
                            await cmd2.ExecuteNonQueryAsync();
                        }
                    }
                    if (sqlFields.Length > 0)
                    {
                        var sql = $"UPDATE ItmMae SET {sqlFields.Substring(0, sqlFields.Length - 1)} WHERE Itmmae.Secuencial = @Secuencial";
                        cmd.CommandText = sql;
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.Transaction = tran;
                        cmd.Parameters.AddWithValue("Secuencial", itemId);
                        await cmd.ExecuteNonQueryAsync();

                    }

                    await tran.CommitAsync();
                }
                catch (Exception)
                {
                    await tran.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task DeleteImagenItem(string type, int entityId, int imageId, int blobId)
        {
            if (type == "ITEM" || type == "category_default")
            {
                await SetImageItemDeleted(entityId, blobId);
            }
            else if (type == "MARCA")
            {
                await ExecuteAsync("UPDATE StoreSync_Marcas SET FechaModificacionLogo = sysdatetimeoffset() WHERE ID = @1", entityId);
            }
        }

        public async Task GuardarMarca(int marcaId, Marca marca)
        {
            using var sqlDb = GetSQLDatabase();
            await sqlDb.BeginTransactionAsync();

            if (marcaId == 0)
            {
                var codigo = await sqlDb.ExecuteScalarAsync<int>("SELECT MAX(MARCOD) AS ID FROM Marcas WHERE  EmpCod = @0", CodigoEmpresa);
                codigo++;
                await sqlDb.ExecuteAsync("INSERT INTO Marcas(EmpCod, MarCod, MarDsc) VALUES (@0, @1, @2)", CodigoEmpresa, codigo, marca.Nombre);
                await sqlDb.ExecuteAsync("UPDATE StoreSync_Marcas SET FechaModificacion = sysdatetimeoffset(), DescriptionShort = @1, Description = @2 WHERE ID = @0", marcaId, marca.DescriptionShort, marca.Description);
            }
            await sqlDb.ExecuteAsync("UPDATE StoreSync_Marcas SET FechaModificacion = sysdatetimeoffset(), DescriptionShort = @1, Description = @2 WHERE ID = @0", marcaId, marca.DescriptionShort, marca.Description);
            sqlDb.CompleteTransaction();
        }

        public Task<List<AtributoDto>> GetAtributos()
        {
            return DbContext
                .Atributos
                .Where(at => at.EmpCod == CodigoEmpresa)
                .Select(at => new AtributoDto
                {
                    Id = at.Id,
                    AttributeType = at.AttributeType,
                    Name = at.Name,
                    PublicName = at.PublicName
                }).ToListAsync();
        }

        public async Task<ItemInventario> CreateCategory(
              int padreId,
              string codigo,
              string nombre)
        {

            using var sqlDb = GetSQLDatabase();

            var categoria = await sqlDb.FetchProcAsync<ItemInventario>("Inventario_CrearCategoria",
                new
                {
                    EmpCod = CodigoEmpresa,
                    Codigo = codigo,
                    Nombre = nombre,
                    PadreId = padreId
                });


            return categoria.FirstOrDefault();

        }

        public async Task<bool> ExistSubCategoryCode(int padreId, string code)
        {
            using var sqlDb = GetSQLDatabase();

            int? flag;
            flag = await sqlDb.ExecuteScalarAsync<int?>("SELECT Secuencial FROM Itmmae WHERE ItmCod = (SELECT ItmCod FROM ItmMae WHERE Secuencial = @0) + @1",      padreId, code);
            return flag.HasValue;
        }

        public async Task<bool> ExistSubCategoryName(int padreId, string name)
        {

            using var sqlDb = GetSQLDatabase();

            int? flag;
            flag = await sqlDb.ExecuteScalarAsync<int?>("SELECT Secuencial, ItmDsc As Nombre\r\n            FROM Itmmae\r\n            WHERE EmpCod = @0 AND ItmTip = 'G' AND ItmCod Like (SELECT ItmCod FROM ItmMae WHERE Secuencial = @1) + '%' AND ItmDsc = @2", 
                padreId, name);
            return flag.HasValue;

        }

        public async Task<ItemInventario> CreateProduct(
          int padreId,
          string referencia,
          string nombre)
        {

            using var sqlDb = GetSQLDatabase();
            var items = await sqlDb.FetchProcAsync<ItemInventario>("Inventario_CrearProducto", new 
            {
                EmpCod = CodigoEmpresa,
                Referencia = referencia,
                Nombre = nombre,
                PadreId = padreId
            });
            return items.FirstOrDefault();
        }

        public async Task<bool> ExistProductName(string name)
        {
            using var sqlDb = GetSQLDatabase();
            string itmCod;
            itmCod = await sqlDb.FirstOrDefaultAsync<string>("SELECT ItmCod FROM ItmMae WHERE EmpCod = @0 AND ItmDsc = @1", CodigoEmpresa, name);
            return !(itmCod is null);
        }

        public async Task<bool> ExistProductReference(string reference)
        {
            using var sqlDb = GetSQLDatabase();
            string itmCod;
            itmCod = await sqlDb.FirstOrDefaultAsync<string>("SELECT ItmCod FROM ItmMae WHERE EmpCod = @0 AND ItmCodVen = @1", CodigoEmpresa, reference);
            return !(itmCod is null);
        }


     

    }
}
