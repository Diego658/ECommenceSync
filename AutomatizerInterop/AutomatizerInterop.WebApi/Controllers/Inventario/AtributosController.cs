using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutomatizerInterop.Data.Entities.Inventario;
using AutomatizerInterop.Data.EntityFramewrok;
using AutomatizerInterop.Data.Entities.Inventario.Dtos;
using AutomatizerInterop.Data.Repositories;
using AutomatizerInterop.Data.Interfaces;
using AutomatizerInterop.Data.Entities;
using AutoMapper;
using Newtonsoft.Json.Linq;
using AutomatizerInterop.WebApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace AutomatizerInterop.WebApi.Controllers.Inventario
{
    [Authorize]
    [Route("api/Automatizer/Inventario/[controller]")]
    [ApiController]
    public class AtributosController : ControllerBase
    {
        private readonly AutomatizerInteropDbContext _context;
        private readonly ConfiguracionProgramaFacturacionElectronica _configuracion;
        private readonly IMapper _mapper;

        public AtributosController(AutomatizerInteropDbContext context, IInteropConfiguracionProvider configuracionProvider, IMapper mapper)
        {
            _context = context;
            _configuracion = configuracionProvider.GetConfiguracion(context.ConfiguracionId);
            _mapper = mapper;
        }

        // GET: api/Atributos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AtributoDto>>> GetAtributos()
        {
            var atributos = _mapper.ProjectTo<AtributoDto>(_context.Atributos.Where(a => a.EmpCod == _configuracion.CodigoEmpresa && a.Activo));
            return await atributos.ToListAsync();
        }

        // GET: api/Atributos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AtributoDto>> GetAtributo(int id)
        {
            var atributo = await _context.Atributos.FindAsync(id);

            if (atributo == null || !atributo.Activo)
            {
                return NotFound();
            }

            return _mapper.Map<AtributoDto>(atributo);
        }

        // PUT: api/Atributos/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAtributo(int id, AtributoDto atributo)
        {
            if (id != atributo.Id)
            {
                return BadRequest();
            }
            // var atributoDb = _mapper.Map<Atributo>(atributo);


            var atributoDb = await _context.Atributos.FindAsync(id);

            if (atributoDb == null || !atributoDb.Activo ) return NotFound();

            atributoDb.FechaModificacion = DateTimeOffset.Now;
            atributoDb.EmpCod = _configuracion.CodigoEmpresa;
            atributoDb.Name = atributo.Name ?? atributoDb.PublicName;
            atributoDb.PublicName = atributo.PublicName ?? atributoDb.PublicName;
            atributoDb.AttributeType = atributo.AttributeType ?? atributoDb.AttributeType;

            _context.Entry(atributoDb).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AtributoExists(id))
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

        // POST: api/Atributos
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Atributo>> PostAtributo(AtributoDto atributo)
        {
            if (atributo.AttributeType == null)
            {
                return BadRequest(nameof(atributo.AttributeType));
            }

            var atributoDb = new Atributo
            {
                EmpCod = _configuracion.CodigoEmpresa,
                Name = atributo.Name,
                AttributeType = atributo.AttributeType.Value,
                PublicName = atributo.PublicName,
                FechaModificacion = DateTimeOffset.Now
            };


            _context.Atributos.Add(atributoDb);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAtributo", new { id = atributoDb.Id }, atributo);
        }

        // DELETE: api/Atributos/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<AtributoDto>> DeleteAtributo(int id)
        {
            var atributo = await _context.Atributos.FindAsync(id);
            if (atributo == null)
            {
                return NotFound();
            }

            atributo.Activo = false;
            _context.Atributos.Update(atributo);

            var valores =  _context.AtributoValores.Where(atv => atv.AtributoId == id && atv.Activo).AsTracking();
            foreach (var item in valores)
            {
                item.Activo = false;
            }
            await _context.SaveChangesAsync();

            return _mapper.Map<AtributoDto>( atributo);
        }

        private bool AtributoExists(int id)
        {
            return _context.Atributos.Any(e => e.Id == id);
        }


        


        [HttpPost("CheckUniqueName")]
        public JsonResult CheckUniqueName([FromBody]ValidateField<string, int> data)
        {
            int? id = (int?)data.Id;
            string name = data.Value.ToString();
            var atributos = _context.Atributos.Where(at => at.EmpCod == _configuracion.CodigoEmpresa && at.Activo);
            if (id.HasValue)
            {
                atributos = atributos.Where(at => at.Id != id);
            }
            bool isValid = !atributos.Any(at => at.Name == name.Trim());
            return new JsonResult(isValid);
        }


    }
}
