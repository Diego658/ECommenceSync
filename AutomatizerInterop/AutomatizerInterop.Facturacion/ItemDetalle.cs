using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutomatizerInterop.Facturacion
{
    public class ItemDetalle : FacturaDetalle
    {
        IFacturacionHelper _facturacionHelper;
        public int IdItem { get;  }
        public string Referencia { get; set; }
        public string Nombre { get; set; }
        public byte Unidad { get; set; }
        public decimal Costo { get; set; }
        public ItemDetalle(IFacturacionHelper facturacionHelper, int idItem, decimal cantidad, decimal precio, decimal descuento, string bodega  )
        {
            _facturacionHelper = facturacionHelper;
            IdItem = idItem;
            Cantidad = cantidad;
            Precio = precio;
            Descuento = descuento;
            CodigoBodega = bodega;
        }


        public async Task Calculate()
        {
            using var dbCon = _facturacionHelper.GetDbConnection();
            await dbCon.OpenAsync();
            using var reader = await dbCon.ExecuteReaderAsync("SELECT ItmCod, ItmCodVen, ItmUA, ItmDsc, TieIva, CtaConVen As CuentaVentaIva, CtaConVen2 As CuentaVenta, CtaConDescVen As CuentaDescuentoVentaIva, CtaConDescVen2 As CuentaDescuentoVenta FROM ItmMae WHERE Secuencial = @id", new { id = IdItem });
            await reader.ReadAsync();
            Codigo = reader.GetString(0);
            Referencia = reader.GetString(1);
            Unidad = Convert.ToByte(reader.GetInt32(2));
            Nombre =  reader.GetString(3);
            Subtotal = Cantidad * Precio;
            TieneIva = reader.GetBoolean(4);
            CuentaVentaIva = reader.GetString(5);
            CuentaVenta = reader.GetString(6);
            CuentaDescuentoVentaIva = reader.GetString(7);
            CuentaDescuentoVenta = reader.GetString(8);
            if (TieneIva)
            {
                Impuesto = Subtotal * 0.12m;
            }
            else
            {
                Impuesto = 0;
            }
            TotalDescuento = Cantidad * Descuento;
            Total = Subtotal + Impuesto;   
        }


    }
}
