using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutomatizerInterop.Data.Interfaces;
using AutomatizerInterop.WebApi.Data;
using DevExtreme.AspNet.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AutomatizerInterop.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsSearchController : ControllerBase
    {
        private readonly SearchDbContext searchDbContext;
        private readonly IInteropConfiguracionProvider _configuracionProvider;

        public ProductsSearchController(SearchDbContext searchDbContext, IInteropConfiguracionProvider configuracionProvider)
        {
            this.searchDbContext = searchDbContext;
            _configuracionProvider = configuracionProvider;
        }


        [HttpGet("ArbolInventarioItems")]
        public object ArbolInventarioItems(DataSourceLoadOptions options)
        {
            //return searchDbContext.ArbolInventario.Where(item => item.PadreID == 0);

            if (options.Filter == null)
            {
                return BadRequest("Debe de establecerce un filtro");
            }
            var idConfig = Request.Headers["ConfiguracionId"];
            if (int.TryParse(idConfig, out var id))
            {
                var config = _configuracionProvider.GetConfiguracion(id);
                return DataSourceLoader.Load(searchDbContext.ArbolInventario.Where(x => x.EmpCod == config.CodigoEmpresa), options);
            }
            return BadRequest("Debe de establecerce un id de configuracion en los headers");
        }

    }


    [ModelBinder(typeof(DataSourceLoadOptionsHttpBinder))]
    public class DataSourceLoadOptions : DataSourceLoadOptionsBase { }





    internal class DataSourceLoadOptionsHttpBinder : IModelBinder
    {


        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            DataSourceLoadOptions sourceLoadOptions = new DataSourceLoadOptions();
            DevExtreme.AspNet.Data.Helpers.DataSourceLoadOptionsParser.Parse(sourceLoadOptions, (Func<string, string>)(key => bindingContext.ValueProvider.GetValue(key).Values));

            //bindingContext.Model = sourceLoadOptions;



            bindingContext.Result = ModelBindingResult.Success(sourceLoadOptions);
        }
    }




}