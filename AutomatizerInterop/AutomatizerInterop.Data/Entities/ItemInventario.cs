using AutomatizerInterop.Data.Entities;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace AutomatizerInterop.Data.Interfaces
{
    public  class ItemInventario
    {
        public string Codigo { get; set; }
        public string Tipo { get; set; }
        public bool HasItems { get; set; }
        public bool Selected { get; set; }
        public string Nombre { get; set; }
        public byte Nivel { get; set; }
        public string Referencia { get; set; }
        public int ItemID { get; set; }
        public string CodigoPadre { get; set; }
        public decimal ExistenciaTotal { get; set; }
        public bool HasImages { get; set; }
    }


    public class CategoriaInventario : ItemInventario
    {
        public List<PaginaWebDatosAdicionalesItem> DatosWeb { get; set; }
        public List<dynamic> Imagenes { get; set; }

    }



    

}