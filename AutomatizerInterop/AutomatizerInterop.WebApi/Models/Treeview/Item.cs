using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AutomatizerInterop.WebApi.Models.Treeview
{
    [Table("StoreSync_ConsultaArbolItems")]
    public class Item
    {
        [Key]
        public int ItemID { get; set; }
        public int PadreID { get; set; }
        public string Codigo { get; set; }
        public string Tipo { get; set; }
        public string Nombre { get; set; }
        public string Referencia { get; set; }
        public bool TieneItems { get; set; }
        public decimal ExistenciaTotal { get; set; }

        public bool TieneImagenes { get; set; }
        public string EmpCod { get; set; }
    }
}
