using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutomatizerInterop.Data.Entities.Inventario;
using AutomatizerInterop.Data.EntityFramewrok;
using AutoMapper;
using AutomatizerInterop.Data.Entities.Inventario.Dtos;
using AutomatizerInterop.WebApi.Models;
using AutomatizerInterop.Data.Interfaces;
using AutomatizerInterop.Data.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using DevExtreme.AspNet.Data;

namespace AutomatizerInterop.WebApi.Controllers.Inventario
{
    [Route("api/Automatizer/Inventario/[controller]")]
    [ApiController]
    public class AtributoValoresController : ControllerBase
    {
        private readonly ConfiguracionProgramaFacturacionElectronica _configuracion;
        private readonly AutomatizerInteropDbContext _context;
        private readonly IMapper _mapper;

        public AtributoValoresController(AutomatizerInteropDbContext context, IInteropConfiguracionProvider configuracionProvider, IMapper mapper)
        {
            _configuracion = configuracionProvider.GetConfiguracion(context.ConfiguracionId);
            _context = context;
            _mapper = mapper;
        }

        // GET: api/AtributoValores
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AtributoValorDto>>> GetAtributoValores(int? atributoId)
        {
            if (atributoId is null)
            {
                return BadRequest("atributoId");
            }
            var atributo = await _context.Atributos.FindAsync(atributoId);
            if (atributo == null || !atributo.Activo)
            {
                return NotFound();
            }

            var atributos = _mapper.ProjectTo<AtributoValorDto>(_context.AtributoValores.Where(at => at.AtributoId == atributoId && at.Activo ));
            return await atributos.ToListAsync();
        }

        // GET: api/AtributoValores/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AtributoValorDto>> GetAtributoValor(int id)
        {
            var atributoValor = await _context.AtributoValores.FindAsync(id);

            if (atributoValor == null || !atributoValor.Activo)
            {
                return NotFound();
            }

            return _mapper.Map<AtributoValorDto>(atributoValor);
        }

        // PUT: api/AtributoValores/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAtributoValor(int id, AtributoValor atributoValor)
        {
            if (id != atributoValor.Id)
            {
                return BadRequest();
            }

            var atributoValorDb = _context.AtributoValores.Find(id);
            atributoValorDb.Color = atributoValor.Color ?? atributoValorDb.Color;
            atributoValorDb.Valor = atributoValor.Valor ?? atributoValorDb.Valor;
            atributoValorDb.FechaModificacion = DateTimeOffset.Now;
            

            _context.Entry(atributoValorDb).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AtributoValorExists(id))
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

        // POST: api/AtributoValores
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<AtributoValor>> PostAtributoValor([FromBody]AtributoValorDto atributoValor, int? atributoId)
        {
            if (atributoId == null)
            {
                return BadRequest();
            }
            var atributoValorDb = new AtributoValor
            {
                AtributoId = atributoId.Value,
                Valor = atributoValor.Valor,
                Orden = 0,
                Color = atributoValor.Color ?? atributoValor.Color,
                FechaModificacion = DateTimeOffset.Now,
                Activo =true
            };

            _context.AtributoValores.Add(atributoValorDb);
            await _context.SaveChangesAsync();

            atributoValor.Id = atributoValorDb.Id;
            atributoValor.AtributoId = atributoValorDb.AtributoId;
            atributoValor.Orden = atributoValorDb.Orden;
            return CreatedAtAction("GetAtributoValor", new { id = atributoValor.Id }, atributoValor);
        }

        // DELETE: api/AtributoValores/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<AtributoValor>> DeleteAtributoValor(int id)
        {
            var atributoValor = await _context.AtributoValores.FindAsync(id);
            if (atributoValor == null || !atributoValor.Activo)
            {
                return NotFound();
            }
            atributoValor.Activo = false;
            _context.AtributoValores.Update(atributoValor);
            //_context.AtributoValores.Remove(atributoValor);
            await _context.SaveChangesAsync();

            return atributoValor;
        }

        private bool AtributoValorExists(int id)
        {
            return _context.AtributoValores.Any(e => e.Id == id);
        }

        [HttpPost("CheckUniqueName")]
        public JsonResult CheckUniqueName([FromBody]ValidateField<string, int> data, int? atributoId)
        {
            if (atributoId is null)
            {
                return new JsonResult(new { isValid = false, message = nameof(atributoId) });
            }
            int? id = (int?)data.Id;
            string name = data.Value.ToString();
            var atributos = _context.AtributoValores.Where(at => at.AtributoId == atributoId);
            if (id.HasValue)
            {
                atributos = atributos.Where(at => at.Id != id);
            }
            bool isValid = !atributos.Any(at => at.Activo && at.Valor == name.Trim());
            return new JsonResult(isValid);
        }


        [HttpGet("GetAllAtributesWithValues")]
        public async Task<object> GetAllAtributosValues()
        {
            var atributosQuery = _context.AtributoValores
                .Where(atv => atv.Atributo.EmpCod == _configuracion.CodigoEmpresa && atv.Activo )
                .Select(atv =>
                    new
                    {
                        atributoId = atv.AtributoId,
                        atributoNombre = atv.Atributo.Name,
                        valorId = atv.Id,
                        valorValor = atv.Valor,
                        color = atv.Color,
                        fullName = $"{atv.Atributo.Name} - {atv.Valor}"
                    }
                );
            //if (Request.QueryString.HasValue)
            //{
            //    var datasourceLoader = new DataSourceLoadOptions
            //    {

            //    };
            //        ;
            //    var valores = await atributosQuery.ToListAsync();
            //    return valores;
            //}
            //else
            //{
            //    var valores = await atributosQuery.ToListAsync();
            //    return valores;
            //}
            var valores = await atributosQuery.ToListAsync();
            return valores;
            //return new JsonResult(atributosQuery);

        }

        [HttpGet("GetAllAtributesWithValuesGrouped")]
        public JsonResult GetAllAtributosValuesGrouped()
        {
            var atributosValores = _context.Atributos.AsQueryable();

            

            var searchMode = "";
            var searchField = "";
            var searchValue = "";
            if (Request.QueryString.HasValue)
            {
                var queryString = Request.QueryString.Value.Split('&');
                foreach (var query in queryString)
                {
                    if (query.Contains("skip"))
                    {
                        //if (!queryString.Any(q => q.Contains("searchValue"))) 
                        atributosValores = atributosValores.Skip(int.Parse(query.Substring(query.IndexOf('=') + 1, 1)));
                    }
                    else if (query.Contains("take"))
                    {
                        atributosValores = atributosValores.Take(int.Parse(query.Substring(query.IndexOf('=') + 1, 1)));
                    }
                    else if (query.Contains("searchOperation"))
                    {
                        var startIndex = query.IndexOf('=') + 1;
                        searchMode = query.Substring(startIndex, query.Length - startIndex);
                    }
                    else if (query.Contains("searchExpr"))
                    {
                        var startIndex = query.IndexOf('=') + 1;
                        searchField = query.Substring(startIndex, query.Length - startIndex);
                    }
                    else if (query.Contains("searchValue"))
                    {
                        var startIndex = query.IndexOf('=') + 1;
                        searchValue = $"%{query.Substring(startIndex, query.Length - startIndex)}%";
                    }
                }
            }

            if (searchValue.Length > 0)
            {
                atributosValores = atributosValores.Where(at => at.Activo && at.EmpCod == _configuracion.CodigoEmpresa &&  at.Valores.Any(x => EF.Functions.Like( x.Valor,searchValue)));
                var queryValores = atributosValores.Select(at =>
                    new
                    {
                        key = at.Name,
                        nombre = at.Name,
                        items = at.Valores.Select(atv => new
                        {
                            atributoId = atv.AtributoId,
                            valorId = atv.Id,
                            valor = atv.Valor,
                            color = atv.Color
                        }).Where(atv =>  EF.Functions.Like( atv.valor,searchValue))
                    }
                );
                return new JsonResult(queryValores);
                //atributosValores = atributosValores.Where (at=>  at.  )
            }
            else
            {
                var queryValores = atributosValores.Where(at=> at.Activo && at.EmpCod == _configuracion.CodigoEmpresa).Select(at =>
                    new
                    {
                        key = at.Name,
                        nombre = at.Name,
                        items = at.Valores.Select(atv => new
                        {
                            atributoId = atv.AtributoId,
                            valorId = atv.Id,
                            valor = atv.Valor,
                            color = atv.Color
                        })
                    }
                );
                

                return new JsonResult(queryValores);
            }






        }



    }
}
