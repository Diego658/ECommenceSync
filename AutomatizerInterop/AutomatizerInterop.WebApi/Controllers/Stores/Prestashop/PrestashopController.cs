using AutomatizerInterop.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutomatizerInterop.Data.EntityFramewrok;
using AutomatizerInterop.Data.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AutomatizerInterop.Data.Entities.Stores.Prestashop;
using Microsoft.EntityFrameworkCore;
using AutomatizerInterop.Data.Helper;
using Microsoft.Extensions.Configuration;
using Bukimedia.PrestaSharp.Factories;

namespace AutomatizerInterop.WebApi.Controllers.Stores.Prestashop
{

    [Route("api/Automatizer/Inventario/Stores/Prestashop/[controller]")]
    [ApiController]
    public class PrestashopController : ControllerBase
    {
        AutomatizerInteropDbContext _context;
        ConfiguracionProgramaFacturacionElectronica _configuracion;
        IPrestashopHelper _prestashopHelper;

        public PrestashopController(AutomatizerInteropDbContext context, IInteropConfiguracionProvider configuracionProvider, IPrestashopHelper prestashopHelper)
        {
            _context = context;
            _configuracion = configuracionProvider.GetConfiguracion(context.ConfiguracionId);
            _prestashopHelper = prestashopHelper;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> GetErrores()
        {
            var tipos = await _context.PrestashopCambiosPendientes.Where(c => c.TieneError).Select(e => e.Destino).Distinct().ToListAsync();
            var errores = new List<string>();
            return errores;
        }


        [HttpGet(nameof(EstadoOrdenes))]
        public async Task<ActionResult<dynamic>> EstadoOrdenes([FromQuery] int? year)
        {
            if (year is null)
            {
                return BadRequest($"No se especificó el año!!!");
            }
            using var db = DatabaseHelper.GetSqlDatabase(_configuracion);
            var estados = await db.FetchAsync<dynamic>("SELECT * FROM StoreSync_GetOrdenesEstados(@0, @1)", _configuracion.CodigoEmpresa, year);
            return new JsonResult(estados);
        }
        [HttpGet(nameof(EstadoOrden))]
        public async Task<ActionResult<dynamic>> EstadoOrden([FromQuery] int? id)
        {
            if (id is null)
            {
                return BadRequest($"No se especificó el id!!!");
            }
            using var db = DatabaseHelper.GetSqlDatabase(_configuracion);
            var estados = await db.FirstAsync<dynamic>("SELECT * FROM StoreSync_OrdersStates_Prestashop where id = @0", id);
            return new JsonResult(estados);
        }


        [HttpGet(nameof(Ordenes))]
        public async Task<ActionResult<dynamic>> Ordenes([FromQuery] int? year, string states)
        {
            if (year is null)
            {
                return BadRequest($"No se especificó el año!!!");
            }
            if (states is null)
            {
                return BadRequest($"No se especificaron los estados!!!");
            }
            using var db = DatabaseHelper.GetSqlDatabase(_configuracion);
            var estados = await db.FetchAsync<dynamic>("SELECT * FROM StoreSync_GetOrdenes(@0, @1, @2)", _configuracion.CodigoEmpresa, year, states);
            return new JsonResult(estados);
        }

        [HttpGet(nameof(Orden))]
        public async Task<ActionResult<dynamic>> Orden([FromQuery] long? id)
        {
            if (id is null)
            {
                return BadRequest($"No se especificó el año!!!");
            }

            using var db = DatabaseHelper.GetSqlDatabase(_configuracion);
            var orden = await db.FirstAsync<dynamic>("SELECT * FROM StoreSync_Orders_Prestashop WHERE id = @0", id);
            return orden;
        }

        [HttpGet(nameof(Addres))]
        public async Task<ActionResult<dynamic>> Addres([FromQuery] long? id)
        {
            if (id is null)
            {
                return BadRequest($"No se especificó el año!!!");
            }

            using var db = DatabaseHelper.GetSqlDatabase(_configuracion);
            var orden = await db.FirstAsync<dynamic>("SELECT * FROM StoreSync_Addresses_Prestashop WHERE id = @0", id);
            return orden;
        }

        [HttpGet(nameof(Carrier))]
        public async Task<ActionResult<dynamic>> Carrier([FromQuery] long? id)
        {
            if (id is null)
            {
                return BadRequest($"No se especificó el año!!!");
            }

            using var db = DatabaseHelper.GetSqlDatabase(_configuracion);
            var orden = await db.FirstAsync<dynamic>("SELECT * FROM StoreSync_Carriers_Prestashop WHERE id = @0", id);
            return orden;
        }

        [HttpGet(nameof(Customer))]
        public async Task<ActionResult<dynamic>> Customer([FromQuery] long? id)
        {
            if (id is null)
            {
                return BadRequest($"No se especificó el año!!!");
            }

            using var db = DatabaseHelper.GetSqlDatabase(_configuracion);
            var orden = await db.FirstAsync<dynamic>("SELECT * FROM StoreSync_Customers_Prestashop WHERE PrestashopId = @0", id);
            return orden;
        }

        [HttpGet(nameof(OrderDetails))]
        public async Task<ActionResult<dynamic>> OrderDetails([FromQuery] long? idOrden)
        {
            if (idOrden is null)
            {
                return BadRequest($"No se especificó el año!!!");
            }

            using var db = DatabaseHelper.GetSqlDatabase(_configuracion);
            var orden = await db.FetchAsync<dynamic>(@"SELECT * FROM StoreSync_Prestashop_GetOrdersDetails(@0)", idOrden);
            return orden;
        }

        [HttpGet(nameof(OrderDetail))]
        public async Task<ActionResult<dynamic>> OrderDetail([FromQuery] long? idDetail)
        {
            if (idDetail is null)
            {
                return BadRequest($"No se especificó el idDetail!!!");
            }

            using var db = DatabaseHelper.GetSqlDatabase(_configuracion);
            var orden = await db.FirstAsync<dynamic>(@"SELECT * FROM StoreSync_OrdersDetails_Prestashop WHERE id = @0", idDetail);
            return orden;
        }


        [HttpGet(nameof(OrderDetailsSeries))]
        public async Task<ActionResult<dynamic>> OrderDetailsSeries([FromQuery] long? idOrden)
        {
            if (idOrden is null)
            {
                return BadRequest($"No se especificó el idOrden!!!");
            }

            using var db = DatabaseHelper.GetSqlDatabase(_configuracion);
            var orden = await db.FetchAsync<dynamic>(@"SELECT        StoreSync_OrdersDetails_Prestashop_SeriesAutomatizer.id_detail, StoreSync_OrdersDetails_Prestashop_SeriesAutomatizer.EmpCod, StoreSync_OrdersDetails_Prestashop_SeriesAutomatizer.SecSerie, 
                         SeriesUnicas.NroSerie
            FROM            StoreSync_OrdersDetails_Prestashop_SeriesAutomatizer INNER JOIN
                         SeriesUnicas ON StoreSync_OrdersDetails_Prestashop_SeriesAutomatizer.EmpCod = SeriesUnicas.EmpCod AND StoreSync_OrdersDetails_Prestashop_SeriesAutomatizer.SecSerie = SeriesUnicas.SecSerie INNER JOIN
                         StoreSync_OrdersDetails_Prestashop ON StoreSync_OrdersDetails_Prestashop_SeriesAutomatizer.id_detail = StoreSync_OrdersDetails_Prestashop.id
            WHERE (StoreSync_OrdersDetails_Prestashop.id_order = @0)", idOrden);
            return orden;
        }


        [HttpGet(nameof(OrderCarrier))]
        public async Task<ActionResult<dynamic>> OrderCarrier([FromQuery] long? idOrden)
        {
            if (idOrden is null)
            {
                return BadRequest($"No se especificó el idOrden!!!");
            }

            using var db = DatabaseHelper.GetSqlDatabase(_configuracion);
            var orden = await db.FirstAsync<dynamic>("SELECT * FROM  StoreSync_OrdersCarriers_Prestashop WHERE id_order = @0", idOrden);
            return orden;
        }


        [HttpGet(nameof(OrderPayment))]
        public async Task<ActionResult<dynamic>> OrderPayment([FromQuery] string reference)
        {
            if (reference is null)
            {
                return BadRequest($"No se especificó el reference!!!");
            }

            using var db = DatabaseHelper.GetSqlDatabase(_configuracion);
            var orden = await db.FirstOrDefaultAsync<dynamic>("SELECT * FROM  StoreSync_OrdersPayments_Prestashop WHERE order_reference = @0", reference);
            return orden;
        }


        [HttpGet(nameof(ProductImage))]
        public async Task<ActionResult<dynamic>> ProductImage([FromQuery] long? id)
        {
            if (id is null)
            {
                return BadRequest($"No se especificó el id!!!");
            }
            using var db = DatabaseHelper.GetSqlDatabase(_configuracion);
            var imagen = await db.FirstOrDefaultAsync<dynamic>("SELECT * FROM StoreSync_ItemImagenPortadaPrestashop(@0)", id);
            return imagen;
        }


        [HttpPost(nameof(ConfirmPaymentByTransfer))]
        public async Task<ActionResult<dynamic>> ConfirmPaymentByTransfer([FromQuery] long? idOrden)
        {
            if (idOrden is null)
            {
                return BadRequest($"No se especificó el id!!!");
            }
            using var db = DatabaseHelper.GetSqlDatabase(_configuracion);
            var order = await db.FirstAsync<dynamic>("SELECT * FROM StoreSync_Orders_Prestashop WHERE id = @0", idOrden);
            var estado = await db.FirstAsync<dynamic>("SELECT * FROM StoreSync_OrdersStates_Prestashop where id = @0", order.current_state);
            if (estado.module_name == "ps_wirepayment" && !estado.paid)
            {
                try
                {
                    var factory = new OrderFactory(_prestashopHelper.ApiUrl, _prestashopHelper.ApiKey, "");
                    var psOrder = await factory.GetAsync(idOrden.Value);
                    psOrder.current_state = _prestashopHelper.EstadoPagoAceptado;
                    await factory.UpdateAsync(psOrder);
                    await db.ExecuteAsync("UPDATE StoreSync_Orders_Prestashop SET current_state = @1 WHERE id = @0", idOrden, _prestashopHelper.EstadoPagoAceptado);
                    return new { isOk = true, message = "Orden actualizada" };
                }
                catch (Exception ex)
                {
                    return new { isOk = false, message = ex.Message };
                }
            }
            else
            {
                return new { isOk = false, message = "El estado no es el esperado, actualice la orden" };
            }
        }


        [HttpGet(nameof(FacturarOrden))]
        public async Task<ActionResult<dynamic>> FacturarOrden(long? idOrden)
        {
            if (idOrden is null)
            {
                return BadRequest($"No se especificó el id!!!");
            }
            using var db = DatabaseHelper.GetSqlDatabase(_configuracion);
            var order = await db.FirstAsync<dynamic>("SELECT * FROM StoreSync_Orders_Prestashop WHERE id = @0", idOrden);
            //var estado = await db.FirstAsync<dynamic>("SELECT * FROM StoreSync_OrdersStates_Prestashop where id = @0", order.current_state);
            if (order.current_state == _prestashopHelper.EstadoPagoAceptado)
            {
                try
                {

                    var factory = new OrderFactory(_prestashopHelper.ApiUrl, _prestashopHelper.ApiKey, "");
                    var psOrder = await factory.GetAsync(idOrden.Value);
                    psOrder.current_state = _prestashopHelper.EstadoEnPreparacion;
                    await factory.UpdateAsync(psOrder);
                    await db.ExecuteAsync("UPDATE StoreSync_Orders_Prestashop SET current_state = @1 WHERE id = @0", idOrden, _prestashopHelper.EstadoEnPreparacion);
                    return new { isOk = true, message = "Orden actualizada" };
                }
                catch (Exception ex)
                {
                    return new { isOk = false, message = ex.Message };
                }
            }
            else
            {
                return new { isOk = false, message = "El estado no es el esperado, actualice la orden" };
            }
        }


        [HttpGet(nameof(PayAcceptedStatus))]
        public async Task<ActionResult<dynamic>> PayAcceptedStatus()
        {
            using var db = DatabaseHelper.GetSqlDatabase(_configuracion);
            var estado = await db.FirstAsync<dynamic>("SELECT * FROM StoreSync_OrdersStates_Prestashop where id = @0", _prestashopHelper.EstadoPagoAceptado);
            return estado;
        }


        


    }
}
