using AutoMapper;
using AutomatizerInterop.Data.Entities.Inventario;
using AutomatizerInterop.Data.Entities.Inventario.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Entities.MappingConfigurations.Inventario
{
    public class AtributoProfile: Profile
    {
        public AtributoProfile()
        {
            CreateMap<Atributo, AtributoDto>();
        }
    }
}
