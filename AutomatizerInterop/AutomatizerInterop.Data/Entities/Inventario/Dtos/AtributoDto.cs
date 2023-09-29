using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Entities.Inventario.Dtos
{
    public class AtributoDto
    {
        private string name;
        private string publicName;
        private TiposAtributo? attributeType;

        public int Id { get; set; }
        public string Name { get => name; set => name = value; }
        public string PublicName { get => publicName; set => publicName = value; }
        public TiposAtributo? AttributeType { get => attributeType; set => attributeType = value; }
    }

    //public class AutoDetectUpdates
    //{
    //    private List<string> _actualizadas;

    //    public AutoDetectUpdates()
    //    {
    //        _actualizadas = new List<string>(10);
    //    }

    //    public bool IsUpdated(string name)
    //    {

    //    }

    //}

}
