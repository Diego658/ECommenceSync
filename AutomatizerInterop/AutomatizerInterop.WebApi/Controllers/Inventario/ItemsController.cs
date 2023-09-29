using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutomatizerInterop.Data.Entities.Inventario;
using AutomatizerInterop.Data.EntityFramewrok;
using Microsoft.AspNetCore.Authorization;
using AutomatizerInterop.Data.Interfaces;
using AutomatizerInterop.Data.Entities;
using AutomatizerInterop.Data.Helper;

namespace AutomatizerInterop.WebApi.Controllers.Inventario
{
    [Authorize]
    [Route("api/Automatizer/Inventario/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly AutomatizerInteropDbContext _context;
        private readonly ConfiguracionProgramaFacturacionElectronica _configuracion;

        public ItemsController(AutomatizerInteropDbContext context, IInteropConfiguracionProvider configuracionProvider)
        {
            _context = context;
            _configuracion = configuracionProvider.GetConfiguracion(context.ConfiguracionId);
        }

        // GET: api/Items
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Item>>> GetItems(int? skip, int? take)
        {
            if (!skip.HasValue || !take.HasValue)
            {
                return BadRequest("Debe establecer la informacion de paginacion.");
            }
            var items = _context.Items.Where(i => i.EmpCod == _configuracion.CodigoEmpresa).OrderBy(i => i.ItmDsc).Skip(skip.Value).Take(take.Value);
            return await items.ToListAsync();
        }

        // GET: api/Items/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Item>> GetItem(int id)
        {
            var item = await _context.Items.FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            return item;
        }

        // PUT: api/Items/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutItem(int id, Item item)
        {
            if (id != item.Secuencial)
            {
                return BadRequest();
            }

            _context.Entry(item).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Items
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Item>> PostItem(Item item)
        {
            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetItem", new { id = item.Secuencial }, item);
        }

        // DELETE: api/Items/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Item>> DeleteItem(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();

            return item;
        }

        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.Secuencial == id);
        }


        [HttpGet(nameof(SeriesDisponibles))]
        public async Task<ActionResult<dynamic>> SeriesDisponibles(int? idItem)
        {
            using var db = DatabaseHelper.GetSqlDatabase(_configuracion);
            var estado = await db.FetchAsync<dynamic>(@"SELECT SeriesUnicas.SecSerie, SeriesUnicas.ItmCod, SeriesUnicas.NroSerie, SeriesUnicasDet.Saldo
                FROM SeriesUnicas
	                INNER JOIN ItmMae ON 
		                SeriesUnicas.EmpCod = ItmMae.EmpCod AND SeriesUnicas.ItmCod = ItmMae.ItmCod
	                INNER JOIN SeriesUnicasDet ON
		                SeriesUnicas.EmpCod = SeriesUnicasDet.EmpCod AND SeriesUnicas.SecSerie = SeriesUnicasDet.SecSerie
                WHERE SeriesUnicas.EmpCod = @0 AND ItmMae.Secuencial = @1 AND SeriesUnicasDet.BodCod = 'B1'  AND SeriesUnicas.Saldo > 0", _configuracion.CodigoEmpresa, idItem);
            return estado;
        }



    }
}
