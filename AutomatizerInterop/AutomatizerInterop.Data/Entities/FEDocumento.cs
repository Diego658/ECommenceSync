using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Entities
{

    [PetaPoco.TableName("FAC_ELE_Documentos")]
    [PetaPoco.PrimaryKey("ID", AutoIncrement = true)]
    public class FEDocumento
    {
        public int ID { get; set; }
        public string EmpCod { get; set; }
        public byte ModIde { get; set; }
        public string TrnCod { get; set; }
        public int TrnNum { get; set; }
        public byte TipoSri { get; set; }
        public bool Migrado { get; set; }
        public DateTime FechaEmision { get; set; }
        public string SerieDocumento { get; set; }
        public long IDDocumento { get; set; }
        public int NumeroAuxiliar { get; set; }
    }
}
