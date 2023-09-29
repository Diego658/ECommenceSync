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
    //public class ProductAttributesValuesFromColorsOperation: AutomatizerSQLOperation<ProductAttribute<int>>
    //{
    //    readonly int _timeToSleep;
    //    Dictionary<string, int> _updatedColsSchema;
    //    public override ECommenceSync.Operations Operation => ECommenceSync.Operations.Attributes;

    //    public override OperationModes Mode => OperationModes.Automatic;

    //    public override OperationDirections Direction => OperationDirections.ErpToStore;

    //    public override Guid Identifier => Guid.NewGuid();

    //    private OperationStatus _status;
    //    public override OperationStatus Status { get => _status; }

    //    public ProductAttributesValuesFromColorsOperation(IAutomatizerDataHelper dataHelper, IAutomatizerSQLOperationsHelper operationsHelper)
    //    {
    //        DataHelper = dataHelper;
    //        OperationHelper = operationsHelper;
    //        _timeToSleep = OperationHelper.GetSearchTime(Operation);
    //        _status = OperationStatus.Created;
    //    }


    //    public override async Task<List<ProductCategory<int>>> GetUpdated()
    //    {
    //        using var sqlConex = (SqlConnection)DataHelper.GetConnection();
    //        using var cmd = sqlConex.CreateCommand();
    //        cmd.CommandText = "SELECT  * FROM dbo.StoreSync_GetCategoriasActualizadas(@CodigoEmpresa, @Fecha) ORDER BY Code";
    //        cmd.Parameters.AddWithValue("CodigoEmpresa", DataHelper.CodigoEmpresa);
    //        cmd.Parameters.AddWithValue("Fecha", SyncTimeInfo.LastSyncTime);
    //        await sqlConex.OpenAsync();
    //        using var reader = await cmd.ExecuteReaderAsync();

    //        if (_updatedColsSchema is null)
    //        {
    //            ConfigureColsSchema(reader);
    //        }
    //        var categories = await CargarCategorias(reader);
    //        await sqlConex.CloseAsync();
    //        return categories;
    //    }
    //}

}
