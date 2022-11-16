using Dapper;
using ECommenceSync.AutomatizerSQL.Helpers;
using ECommenceSync.Entities;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommenceSync.AutomatizerSQL.Operations
{
    public class ProductVariationOperation : AutomatizerSQLOperation<ProductVariant<int>>
    {
        private const string SqlGetUpdated = "SELECT * FROM dbo.StoreSync_GetProductosVariablesFromFixedAtributosActualizados(@CodigoEmpresa, @LastSyncTime ,  @TipoPrecioPaginaWeb)";
        OperationStatus _status;
        readonly int _timeToSleep;
        public override ECommenceSync.Operations Operation => ECommenceSync.Operations.ProductsVariations;
        public override OperationModes Mode => OperationModes.Automatic;

        public override OperationDirections Direction => OperationDirections.ErpToStore;
        public override Guid Identifier => Guid.NewGuid();

        public override OperationStatus Status => _status;
        public ProductVariationOperation(IAutomatizerDataHelper dataHelper, IAutomatizerSQLOperationsHelper operationsHelper)
        {
            DataHelper = dataHelper;
            OperationHelper = operationsHelper;
            _timeToSleep = OperationHelper.GetSearchTime(Operation);
            _status = OperationStatus.Created;
        }


        public override async Task<List<ProductVariant<int>>> GetUpdated()
        {
            await using var sqlConex = (SqlConnection)DataHelper.GetConnection();
            await using var reader = await sqlConex.ExecuteReaderAsync(SqlGetUpdated, new { DataHelper.CodigoEmpresa, SyncTimeInfo.LastSyncTime, DataHelper.TipoPrecioPaginaWeb });
            var parser = reader.GetRowParser<Product<int>>();
            var variants = new List<ProductVariant<int>>();
            ProductVariant<int> variant=null;
            var colColorId = reader.GetOrdinal("AttributeColorId");
            var colColorTermId = reader.GetOrdinal("AttributeColorTermId");
            var colTallaId = reader.GetOrdinal("AttributeTallaId");
            var colTallaTermId = reader.GetOrdinal("AttributeTallaTermId");
            var colParentId = reader.GetOrdinal("ParentId");
            while (await reader.ReadAsync())
            {
                var varianId = reader.GetInt32(0);
                if (variant == null || varianId != variant.Id)
                {
                    variant = new ProductVariant<int>()
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        ParentId = reader.GetInt32(colParentId),
                        VariantVariations = new List<ProductVariantVariation<int>>()
                    };
                    variants.Add(variant);
                }
                var produt = parser.Invoke(reader);
                var terms = new List<ProductAttributeTerm<int>>();
                if (reader.GetByte(colColorId) > 0)
                {
                    terms.Add(new() { AttributeId = reader.GetByte(colColorId), Id = reader.GetInt32(colColorTermId) });
                }
                if (reader.GetByte(colTallaId) > 0)
                {
                    terms.Add(new() { AttributeId = reader.GetByte(colTallaId), Id = reader.GetInt32(colTallaTermId) });
                }

                variant.VariantVariations.Add(new() { Product = produt, AttributeTerms = terms });
                //products.Add(parser.Invoke(reader));
            }
            return variants;
        }


        public async override Task Work()
        {
            if (Processors.Count == 0)
            {
                _status = OperationStatus.Stopped;
                return;
            }
            _status = OperationStatus.Working;
            await Sleep(_timeToSleep);
            while (!CancelTokenSource.IsCancellationRequested)
            {
                await BeginSync();
                var updated = await GetUpdated();
                if (updated.Any())
                {
                    foreach (var proc in Processors)
                    {
                        await proc.ProcessChanges(updated);
                        System.Diagnostics.Debug.WriteLine($"{updated.First().Name}");
                    }
                }
                await EndSync();
                await Sleep(_timeToSleep);
            }
            _status = OperationStatus.Stopped;
        }

       

        public override async Task<List<ProductVariant<int>>> ResolveEntities(List<int> keys)
        {
            await using var sqlConex = (SqlConnection)DataHelper.GetConnection();
            await using var reader = await sqlConex.ExecuteReaderAsync("SELECT * FROM StoreSync_GetProductosVariablesFromFixedAtributosByKeys(@CodigoEmpresa, @Keys, @PrecioPaginaWeb)",
                new { Keys = string.Join(',', keys.Select(k => k.ToString())), PrecioPaginaWeb = DataHelper.TipoPrecioPaginaWeb, CodigoEmpresa= DataHelper.CodigoEmpresa });
            var parser = reader.GetRowParser<Product<int>>();
            var variants = new List<ProductVariant<int>>();
            ProductVariant<int> variant = null;
            var colColorId = reader.GetOrdinal("AttributeColorId");
            var colColorTermId = reader.GetOrdinal("AttributeColorTermId");
            var colTallaId = reader.GetOrdinal("AttributeTallaId");
            var colTallaTermId = reader.GetOrdinal("AttributeTallaTermId");
            var colParentId = reader.GetOrdinal("ParentId");
            while (await reader.ReadAsync())
            {
                var varianId = reader.GetInt32(0);
                if (variant == null || varianId != variant.Id)
                {
                    variant = new ProductVariant<int>()
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        ParentId = reader.GetInt32(colParentId),
                        VariantVariations = new List<ProductVariantVariation<int>>()
                    };
                    variants.Add(variant);
                }
                var produt = parser.Invoke(reader);
                var terms = new List<ProductAttributeTerm<int>>();
                if (reader.GetByte(colColorId) > 0)
                {
                    terms.Add(new() { AttributeId = reader.GetByte(colColorId), Id = reader.GetInt32(colColorTermId) });
                }
                if (reader.GetByte(colTallaId) > 0)
                {
                    terms.Add(new() { AttributeId = reader.GetByte(colTallaId), Id = reader.GetInt32(colTallaTermId) });
                }

                variant.VariantVariations.Add(new() { Product = produt, AttributeTerms = terms });
                //variants.Add(parser.Invoke(reader));
            }
            return variants;

        }


        public override Dictionary<int, int> GetHierarchy()
        {
            using var sqlConex = (SqlConnection)DataHelper.GetConnection();
            using var cmd = sqlConex.CreateCommand();
            cmd.CommandText = @"SELECT dbo.StoreSync_ItemsVsPadres.ItemID, dbo.ItmMae.Secuencial AS Padre
            FROM dbo.StoreSync_ItemsVsPadres INNER JOIN
                         dbo.ItmMae ON dbo.StoreSync_ItemsVsPadres.EmpCod = dbo.ItmMae.EmpCod AND dbo.StoreSync_ItemsVsPadres.CodigoPadre = dbo.ItmMae.ItmCod
            WHERE (dbo.StoreSync_ItemsVsPadres.EmpCod = @0) AND (dbo.StoreSync_ItemsVsPadres.Tipo = 'G')";
            cmd.Parameters.AddWithValue("@0", DataHelper.CodigoEmpresa);
            sqlConex.Open();
            using var reader = cmd.ExecuteReader();
            var hierarchy = new Dictionary<int, int>();
            while (reader.Read())
            {
                hierarchy.Add(reader.GetInt32(0), reader.GetInt32(1));
            }
            return hierarchy;
        }

    }
}
