using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatizerInterop.Facturacion
{
    public class FacturaVenta
    {
        decimal _total;
        private string _empCod;
        private string _trnCod;
        private short _empleado;
        private string _usuario;
        private List<FacturaDetalle> _detalles;
        readonly IFacturacionHelper _facturacionHelper;
        List<FormaPago> _formasPago;
        TransaccionVenta _transaccionVenta;
        ClienteFactura _cliente;
        ImpuestoIva _iva;
        DescuentoManual _descuentoManual;

        public string Codigo { get => _trnCod; }
        public int Numero { get; private set; }
        public DateTime Fecha { get; set; }
        public string ClienteCodigo { get; set; }
        public byte ClienteSucursal { get; set; }
        public short VendedorCodigo { get; set; }
        public decimal PorcentajeDescuento { get; set; }
        public decimal Subtotal { get; private set; }
        public decimal Descuento { get; private set; }
        public decimal Impuesto { get; private set; }
        public decimal Total { get; private set; }
        public string Observaciones { get; set; }
        public string Wrk { get; set; }
        public decimal Recibido { get; set; }
        public decimal Cambio { get; set; }
        public ReadOnlyCollection<FacturaDetalle> Detalles { get => _detalles.AsReadOnly(); }
        public ReadOnlyCollection<FormaPago> FormasPago { get => _formasPago.AsReadOnly(); }

        public decimal Contado { get => _formasPago.Where(fp => fp.Forma == Facturacion.FormasPago.Contado).Sum(fp => fp.Valor); }
        public decimal Credito { get => _formasPago.Where(fp => fp.Forma == Facturacion.FormasPago.Credito).Sum(fp => fp.Valor); }
        public FacturaVenta(IFacturacionHelper facturacionHelper, string empCod, string trnCod, short empleado, string usuario)
        {
            _facturacionHelper = facturacionHelper;
            _empCod = empCod;
            _trnCod = trnCod;
            _empleado = empleado;
            _usuario = usuario;
            Fecha = DateTime.Now;
            _detalles = new List<FacturaDetalle>();
            _formasPago = new List<FormaPago>();
        }




        public static FacturaVenta CrearNueva(IFacturacionHelper facturacionHelper, string empCod, string trnCod, short empleado, string usuario)
        {
            return new FacturaVenta(facturacionHelper, empCod, trnCod, empleado, usuario);
        }


        public async Task<ItemDetalle> AgregarItem(int idItem, decimal cantidad, decimal precio, decimal descuento, string bodega)
        {
            var item = new ItemDetalle(_facturacionHelper, idItem, cantidad, precio, descuento, bodega);
            await item.Calculate();
            item.Secuencial = _detalles.Where(d => d.Tipo == TipoDetalle.Item).Cast<ItemDetalle>().Where(d => d.IdItem == idItem).Count() + 1;
            _detalles.Add(item);
            return item; ;
        }

        public async Task<FacturaDetalle> AgregarServicio(string codigo, decimal cantidad, decimal precio, decimal descuento, string bodega)
        {
            var servicio = new ServicioDetalle(_facturacionHelper, _empCod, codigo, cantidad, precio, descuento, bodega);
            await servicio.Calculate();
            servicio.Secuencial = _detalles.Where(d => d.Tipo == TipoDetalle.Servicio).Cast<ServicioDetalle>().Where(d => d.Codigo == codigo).Count() + 1;
            _detalles.Add(servicio);
            return servicio; ;
        }

        public async Task AgregarFormaPago(FormaPago formaPago)
        {
            _formasPago.Add(formaPago);
        }

        public async Task Calcular()
        {
            var subtotal = _detalles.Sum(d => d.Cantidad * d.Precio);
            var descuento = _detalles.Sum(d => d.TotalDescuento);
            var impuesto = _detalles.Sum(d => d.Impuesto);
            var total = _detalles.Sum(d => d.Total);
            Subtotal = decimal.Round(subtotal, 2);
            Descuento = decimal.Round(descuento, 2);
            Impuesto = decimal.Round(impuesto, 2);
            Total = decimal.Round(total, 2);
        }


        public async Task Guardar()
        {
            try
            {
                CalcularNumero();
                await Validar();
                using var dbCon = _facturacionHelper.GetDbConnection();
                await dbCon.OpenAsync();
                using var dbTran = await dbCon.BeginTransactionAsync();
                await GuardarCabecera(dbCon, dbTran);
                await GuardarDesgloseCobrosFac(dbCon, dbTran);
                await GuardarDetalles(dbCon, dbTran);
                await GuardarItemsVsClientes(dbCon, dbTran);
                await GuardarKardexNuevo(dbCon, dbTran);
                if (FormasPago.Where(fp => fp.Forma == Facturacion.FormasPago.Credito).Any(fp => fp.Valor > 0))
                {
                    await GuardarFacIng(dbCon, dbTran);
                }
                if (FormasPago.Where(fp => fp.Forma == Facturacion.FormasPago.Contado).Any(fp => fp.Valor > 0) && _transaccionVenta.IngAut)
                {
                    await GuardarIngreso(dbCon, dbTran);
                }
                await ContabilizaFactura(dbCon, dbTran);
                dbTran.Commit();
            }
            catch (Exception)
            {
                throw;
            }
        }


        private async Task ContabilizaFactura(DbConnection dbCon, DbTransaction dbTran)
        {
            if (Contado > 0 && string.IsNullOrEmpty(_cliente.CuentaContableContado))
            {
                throw new InvalidOperationException($"El Cliente {_cliente.Apellidos} {_cliente.Nombres} no tiene una cuenta contable para cargar el valor del pago en efectivo!!");
            }

            if (Credito > 0 && string.IsNullOrEmpty(_cliente.CuentaContableCredito))
            {
                throw new InvalidOperationException($"El Cliente {_cliente.Apellidos} {_cliente.Nombres} no tiene una cuenta contable para cargar el valor del pago a crédito!!");
            }


            var asiento = new AsientoContable();

            foreach (var detalle in _detalles)
            {
                switch (detalle.Tipo)
                {
                    case TipoDetalle.Item:
                        var itemDetalle = detalle as ItemDetalle;
                        asiento.AddDetalle(detalle.TieneIva ? itemDetalle.CuentaVentaIva : itemDetalle.CuentaVenta, TiposCuenta.Haber, detalle.Subtotal);
                        if (detalle.Descuento > 0)
                        {
                            asiento.AddDetalle(detalle.TieneIva ? itemDetalle.CuentaDescuentoVentaIva : itemDetalle.CuentaDescuentoVenta, TiposCuenta.Debe, detalle.TotalDescuento);
                        }

                        break;
                    case TipoDetalle.Servicio:
                        var servicioDetalle = detalle as ServicioDetalle;
                        asiento.AddDetalle(detalle.TieneIva ? servicioDetalle.CuentaVentaIva : servicioDetalle.CuentaVenta, TiposCuenta.Haber, detalle.Subtotal);
                        if (detalle.Descuento > 0)
                        {
                            asiento.AddDetalle(detalle.TieneIva ? servicioDetalle.CuentaDescuentoVentaIva : servicioDetalle.CuentaDescuentoVenta, TiposCuenta.Debe, detalle.TotalDescuento);
                        }
                        break;
                    default:
                        break;
                }
                if (detalle.TieneIva) /*Cuenta iva*/
                {
                    asiento.AddDetalle(_iva.CuentaContable, TiposCuenta.Haber, detalle.Impuesto);
                }
            }


            foreach (var pago in _formasPago)
            {
                switch (pago.Forma)
                {
                    case Facturacion.FormasPago.Contado:
                        if (string.IsNullOrEmpty(_transaccionVenta.CuentaCaja))
                        {
                            asiento.AddDetalle(_cliente.CuentaContableContado, TiposCuenta.Debe, pago.Valor);
                        }
                        else
                        {
                            asiento.AddDetalle(_transaccionVenta.CuentaCaja, TiposCuenta.Debe, pago.Valor);
                        }
                        break;
                    case Facturacion.FormasPago.Credito:
                        if (string.IsNullOrEmpty(_transaccionVenta.CuentaCredito))
                        {
                            asiento.AddDetalle(_cliente.CuentaContableCredito, TiposCuenta.Debe, pago.Valor);
                        }
                        else
                        {
                            asiento.AddDetalle(_transaccionVenta.CuentaCredito, TiposCuenta.Debe, pago.Valor);
                        }
                        break;
                    default:
                        break;
                }
            }

            asiento.Validar();

            await dbCon.ExecuteAsync("StoreSync_Facturas_GenerarCC", new
            {
                EmpCod = _empCod,
                TrnCod = _trnCod,
                TrnNum = Numero,
                TrnTip = _transaccionVenta.TipCmp,
                FacFec = Fecha.ToString("yyyyMMdd"),
                CmpTotDeb = asiento.TotalDebe,
                CmpTotHab = asiento.TotalHaber,
                SgrUsr = _usuario,
                EplSec = _empleado

            }, dbTran, commandType: System.Data.CommandType.StoredProcedure);

            var numCmp = await dbCon.ExecuteScalarAsync<string>(@"SELECT CmpNum
            FROM TrnVsCmp   
            WHERE EmpCod = @EmpCod AND ModIde = 8 AND TrnCod = @TrnCod AND TrnNum = @TrnNum",
            new
            {
                EmpCod = _empCod,
                TrnCod = _trnCod,
                TrnNum = Numero,
            }, dbTran);

            foreach (var detalle in asiento.Detalles)
            {
                await dbCon.ExecuteAsync(@"INSERT INTO CmpDetCue
                (EmpCod, SucCod, Año, PrdSec, TrnTip, TrnNum, AsiNro, Tip, CtaCod, SecCta, Comentario, ValorDebe, ValorHaber, CtaTip, Codigo101, Base101, SecCambiosPat)
                VALUES  (@EmpCod, '01', @Ano, @PrdSec, @TrnTip, @TrnNum, '01', @Tip, @CtaCod, 1, @Comentario, @ValorDebe, @ValorHaber, 
                    'M', NULL, 0, 0)",
                new
                {
                    EmpCod = _empCod,
                    Ano = Fecha.Year,
                    PrdSec = Fecha.Month,
                    TrnTip = _transaccionVenta.TipCmp,
                    TrnNum = numCmp,
                    Tip = detalle.TipoCuenta == TiposCuenta.Debe?"D":"H",
                    CtaCod = detalle.Cuenta,
                    Comentario = "",
                    ValorDebe = detalle.TipoCuenta == TiposCuenta.Debe ? detalle.Valor : 0,
                    ValorHaber = detalle.TipoCuenta == TiposCuenta.Debe ? 0: detalle.Valor
                }, dbTran);
            }

        }

        private async Task GuardarIngreso(DbConnection dbCon, DbTransaction dbTran)
        {
            if (string.IsNullOrEmpty(_transaccionVenta.IngCod))
            {
                throw new InvalidOperationException("No ha establecido la transaccion para el ingreso automatico.");
            }
            var pagoEfectivo = _formasPago.First(fp => fp.Forma == Facturacion.FormasPago.Contado);
            var numPago = _formasPago.IndexOf(pagoEfectivo) + 1;
            await dbCon.ExecuteAsync("StoreSync_Factura_GuardaIngresoAutomatico", new
            {
                EmpCod = _empCod,
                TrnCod = _trnCod,
                TrnNum = Numero,
                _transaccionVenta.IngCod,
                Fecha = Fecha.ToString("yyyyMMdd hh:mm:ss"),
                EstCod = "R",
                CliSec = ClienteCodigo,
                pagoEfectivo.Valor,
                Obs = $"Ing. automático generado por Factura: {_trnCod}-{Numero} ",
                SucNum = ClienteSucursal,
                TotImp = Impuesto,
                TotDes = Descuento,
                CobCod = _empleado,
                Wrk = Wrk ?? Environment.MachineName,
                SgrUsr = _usuario,
                NumPago = numPago
            }, dbTran, commandType: System.Data.CommandType.StoredProcedure); ; ;
        }




        private async Task GuardarFacIng(DbConnection dbCon, DbTransaction dbTran)
        {
            int numPago = 1;
            foreach (var pago in _formasPago)
            {
                switch (pago.Forma)
                {
                    case Facturacion.FormasPago.Contado:
                        await dbCon.ExecuteAsync("StoreSync_Facturas_GuardaFacIng", new
                        {
                            EmpCod = _empCod,
                            TrnCod = _trnCod,
                            TrnNum = Numero,
                            NumPago = numPago,
                            EstCod = "R",
                            CliSec = ClienteCodigo,
                            FacTot = pago.Valor,
                            FacCrePag = pago.Valor,
                            FacCrePen = 0,
                            FacCreDias = 0,
                            FecVence = Fecha.ToString("yyyyMMdd"),
                            NumCuotas = 1
                        }, dbTran, commandType: System.Data.CommandType.StoredProcedure);
                        break;
                    case Facturacion.FormasPago.Credito:
                        await dbCon.ExecuteAsync("StoreSync_Facturas_GuardaFacIng", new
                        {
                            EmpCod = _empCod,
                            TrnCod = _trnCod,
                            TrnNum = Numero,
                            NumPago = numPago,
                            EstCod = "P",
                            CliSec = ClienteCodigo,
                            FacTot = pago.Valor,
                            FacCrePag = 0,
                            FacCrePen = pago.Valor,
                            FacCreDias = 5,
                            FecVence = Fecha.AddDays(5).ToString("yyyyMMdd"),
                            NumCuotas = 1
                        }, dbTran, commandType: System.Data.CommandType.StoredProcedure);
                        break;
                    default:
                        break;
                }
                numPago++;
            }
        }


        private async Task GuardarKardexNuevo(DbConnection dbCon, DbTransaction dbTran)
        {
            await dbCon.ExecuteAsync("StoreSync_FacturaVenta_GuardaKardexNuevo", new
            {
                EmpCod = _empCod,
                TrnCod = _trnCod,
                TrnNum = Numero
            }, dbTran, commandType: System.Data.CommandType.StoredProcedure);
        }



        private async Task GuardarItemsVsClientes(DbConnection dbCon, DbTransaction dbTran)
        {
            await dbCon.ExecuteAsync("StoreSync_GuardaItemsVsCli", new
            {
                EmpCod = _empCod,
                TrnCod = _trnCod,
                TrnNum = Numero
            }, dbTran, commandType: System.Data.CommandType.StoredProcedure);
        }
        private async Task GuardarCabecera(DbConnection dbCon, DbTransaction dbTran)
        {

            await dbCon.ExecuteAsync("StoreSync_GuardaCabeceraFactura",
                new
                {
                    EmpCod = _empCod,
                    TrnCod = _trnCod,
                    TrnNum = Numero,
                    CliCod = ClienteCodigo,
                    FacFec = Fecha.ToString("yyyyMMdd hh:mm:ss"),
                    FacSub = Subtotal,
                    FacImp = Impuesto,
                    FacDes = Descuento,
                    FacTot = Total,
                    FacObs = Observaciones,
                    SucCliente = ClienteSucursal,
                    Contado = _formasPago.Where(fp => fp.Forma == Facturacion.FormasPago.Contado).Sum(fp => fp.Valor),
                    Credito = _formasPago.Where(fp => fp.Forma == Facturacion.FormasPago.Credito).Sum(fp => fp.Valor),
                    Vendedor = _empleado,
                    Wrk = Wrk ?? Environment.MachineName,
                    BodCod = "",
                    TarCod = "",
                    TarNumBou = "",
                    TarNumCuo = 0,
                    TarEst = "",
                    TarVal = 0,
                    TarObs = "",
                    TrnNumTar = "",
                    NumPagos = 1,
                    NumDias = 0,
                    BieTarIva = _detalles.Where(d => d.Tipo == TipoDetalle.Item && d.TieneIva).Sum(d => d.Subtotal - d.TotalDescuento),
                    BieTarCero = _detalles.Where(d => d.Tipo == TipoDetalle.Item && !d.TieneIva).Sum(d => d.Subtotal - d.TotalDescuento),
                    SrvTarIva = _detalles.Where(d => d.Tipo == TipoDetalle.Servicio && d.TieneIva).Sum(d => d.Subtotal - d.TotalDescuento),
                    SrvTarCero = _detalles.Where(d => d.Tipo == TipoDetalle.Servicio && !d.TieneIva).Sum(d => d.Subtotal - d.TotalDescuento),
                    SgrUsr = _usuario,
                    Recibido,
                    Cambio
                }, transaction: dbTran, commandType: System.Data.CommandType.StoredProcedure);

        }


        private async Task GuardarDetalles(DbConnection dbCon, DbTransaction dbTran)
        {
            var orden = 1;
            foreach (var detalle in _detalles)
            {
                switch (detalle.Tipo)
                {
                    case TipoDetalle.Item:
                        var detalleItem = detalle as ItemDetalle;
                        await dbCon.ExecuteAsync("StoreSync_Factura_GuardaDetalleItemFactura", new
                        {
                            EmpCod = _empCod,
                            TrnCod = _trnCod,
                            TrnNum = Numero,
                            ItmCod = detalleItem.Referencia,
                            ISec = detalle.Secuencial,
                            ItmCan = detalle.Cantidad,
                            ItmUni = detalleItem.Unidad,
                            ItmPre = detalle.Precio,
                            ItmImp = detalle.Impuesto,
                            ItmDes = detalle.Descuento,
                            BodCod = detalle.CodigoBodega,
                            Obs = detalle.Observaciones ?? "",
                            Orden = orden,
                            CodigoBod = detalleItem.Codigo,
                            TieneIvaParaRep = detalleItem.Impuesto > 0
                        }, dbTran, commandType: System.Data.CommandType.StoredProcedure);
                        break;
                    case TipoDetalle.Servicio:
                        var detalleServicio = detalle as ServicioDetalle;
                        await dbCon.ExecuteAsync("StoreSync_Factura_GuardaDetalleServicioFactura", new
                        {
                            EmpCod = _empCod,
                            TrnCod = _trnCod,
                            TrnNum = Numero,
                            SerNum = detalleServicio.Codigo,
                            SSec = detalleServicio.Secuencial,
                            SerDsc = detalleServicio.Descripcion,
                            SerPre = detalleServicio.Precio,
                            SerImp = detalleServicio.Impuesto,
                            SerDes = detalleServicio.Descuento,
                            CtaCod = detalleServicio.CodigoCuenta,
                            SerCan = detalleServicio.Cantidad,
                            Obs = detalleServicio.Observaciones,
                            Orden = orden,
                            TieneIvaParaRep = detalleServicio.Impuesto > 0,
                            Vendedor = _empleado
                        }, dbTran, commandType: System.Data.CommandType.StoredProcedure);
                        break;
                    default:
                        break;
                }
                orden++;
            }
        }

        private async Task GuardarDesgloseCobrosFac(DbConnection dbCon, DbTransaction dbTran)
        {
            var tipoPago = "";
            if (_formasPago.Where(fp => fp.Forma == Facturacion.FormasPago.Credito).Sum(fp => fp.Valor) > 0)
            {
                tipoPago = "Cred/";
            }
            else
            {
                foreach (var formaPago in _formasPago)
                {
                    switch (formaPago.Forma)
                    {
                        case Facturacion.FormasPago.Contado:
                            tipoPago += "E/";
                            break;
                        case Facturacion.FormasPago.Credito:
                            tipoPago += "C/";
                            break;
                        default:
                            break;
                    }
                }
            }

            tipoPago = tipoPago.Substring(0, tipoPago.Length - 1);

            await dbCon.ExecuteAsync(@"INSERT INTO DesgloseCobrosFac
                         ( EmpCod, DevDesc, TrnCodFue, TrnNumFue, TrnFecFue, TrnCodFac, TrnNumFac, TrnValor, TrnTipPag, TrnEst, EplCod)
            VALUES        (@EmpCod, @DevDesc, @TrnCodFue, @TrnNumFue, @TrnFecFue, @TrnCodFac, @TrnNumFac, @TrnValor, @TrnTipPag, @TrnEst, @EplCod)",
                new
                {
                    EmpCod = _empCod,
                    DevDesc = 0,
                    TrnCodFue = _trnCod,
                    TrnNumFue = Numero,
                    TrnFecFue = Fecha.ToString("yyyyMMdd"),
                    TrnCodFac = _trnCod,
                    TrnNumFac = Numero,
                    TrnValor = _formasPago.Any(fp => fp.Forma != Facturacion.FormasPago.Contado) ? 0 : Total,
                    TrnTipPag = tipoPago,
                    TrnEst = _formasPago.Any(fp => fp.Forma != Facturacion.FormasPago.Contado) ? "P" : "R",
                    EplCod = _empleado
                }, transaction: dbTran);

        }

        private async Task Validar()
        {
            using var dbCon = _facturacionHelper.GetDbConnection();
            _transaccionVenta = await dbCon.QueryFirstAsync<TransaccionVenta>("SELECT CAST(IngAut as bit) As IngAut, IngCod, DiaGra, CtaCaja As CuentaCaja, CtaCredito As CuentaCredito, TipCmp  FROM Trn WHERE EmpCod = @EmpCod AND trncod = @TrnCod",
                new
                {
                    EmpCod = _empCod,
                    TrnCod = _trnCod
                });


            _iva = await dbCon.QueryFirstAsync<ImpuestoIva>(@"SELECT CodImp As CodigoImpuesto, DesImp As Nombre, CtaCod As CuentaContable, 
                SUBSTRING(Formula ,CHARINDEX('=',Formula ) + 2, DATALENGTH(FORMULA))
                FROM Impuestos
                WHERE EmpCod = @EmpCod AND ModIde = 8 AND FrmCod = 'FA' AND DesImp = 'IVA_Det'",
                new
                {
                    EmpCod = _empCod
                });

            _descuentoManual = await dbCon.QueryFirstAsync<DescuentoManual>(@"SELECT CodDes As Codigo, DesDes As Nombre, CtaCod As CuentaContable, 
            SUBSTRING(Formula ,CHARINDEX('=',Formula ) + 2, DATALENGTH(FORMULA)) As Formula
            FROM Descuentos
            WHERE EmpCod = @EmpCod  AND ModIde = 8 AND FrmCod = 'FA'  AND DesDes = 'Desc_Manual'",
                new
                {
                    EmpCod = _empCod
                });


            _cliente = await dbCon.QueryFirstAsync<ClienteFactura>(@"SELECT Clisec As Codigo,  CliNom As Nombres, CliApl As Apellidos, 
                Clicedruc As Identificacion, nomcom As NombreComercial,ctacodco As  CuentaContableContado, ctacodcr As CuentaContableCredito
                FROM climae WHERE  EmpCod = @EmpCod AND Clisec = @Clisec",
                new
                {
                    EmpCod = _empCod,
                    Clisec = ClienteCodigo
                });

            if (Detalles.Count == 0)
            {
                throw new InvalidOperationException($"No hay detalles que guardar!!!");
            }
            if (string.IsNullOrEmpty(ClienteCodigo))
            {
                throw new InvalidOperationException($"Debe estabelcer el cliente !!!");
            }
            if (ClienteSucursal <= 0)
            {
                throw new InvalidOperationException($"Debe estabelcer la sucursal del cliente!!!");
            }
            if (VendedorCodigo <= 0)
            {
                throw new InvalidOperationException($"Debe establecer el vendedor !!!");
            }
            if (FormasPago.Count == 0)
            {
                throw new InvalidOperationException($"Debe establecer las formas de pago !!!");
            }
            if (FormasPago.Sum(fp => fp.Valor) != Total)
            {
                throw new InvalidOperationException($"La suma de formas de pago es diferente del total!!!");
            }


        }


        private void CalcularNumero()
        {
            using var dbCon = _facturacionHelper.GetDbConnection();
            Numero = dbCon.ExecuteScalar<int>("SELECT ISNULL(MAX(TrnNum),0) +1 As Numero FROM FacCab WHERE EmpCod = @EmpCod AND TrnCod = @TrnCod",
                new
                {
                    EmpCod = _empCod,
                    TrnCod = _trnCod
                });
        }

    }
}
