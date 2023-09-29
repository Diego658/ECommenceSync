using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutomatizerInterop.Facturacion
{
    public class ServicioDetalle: FacturaDetalle
    {
        private IFacturacionHelper _facturacionHelper;
        readonly string _empCod;

        public string Descripcion { get; set; }
        public string CodigoCuenta { get; set; }
        public ServicioDetalle(IFacturacionHelper facturacionHelper, string empCod, string codigo, decimal cantidad, decimal precio, decimal descuento, string bodega)
        {
            Tipo = TipoDetalle.Servicio;
            _empCod = empCod;
            _facturacionHelper = facturacionHelper;
            Codigo = codigo;
            Cantidad = cantidad;
            Precio = precio;
            Descuento = descuento;
            CodigoBodega = bodega;
        }


        public async Task Calculate()
        {
            using var dbCon = _facturacionHelper.GetDbConnection();
            await dbCon.OpenAsync();
            using var reader = await dbCon.ExecuteReaderAsync(@"SELECT SrvDsc, SrvCtaCod As CuentaVentaIva, SrvCtaCod2 As CuentaVenta, CtaConDescVen As CuentaDescuentoVentaIva, CtaConDescVen2 As CuentaDescuentoVenta, TieIva
            FROM Servicios WHERE (EmpCod = @EmpCod) AND  SrvCod = @SrvCod ", new { EmpCod = _empCod, SrvCod = Codigo });
            await reader.ReadAsync();
            
            Subtotal = Cantidad * (Precio - Descuento);
            Descripcion = reader.GetString(0);
            CuentaVentaIva = reader.GetString(1);
            CuentaVenta = reader.GetString(2);
            CuentaDescuentoVentaIva = reader.GetString(3);
            CuentaDescuentoVenta = reader.GetString(4);
            TieneIva = reader.GetBoolean(5);
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
