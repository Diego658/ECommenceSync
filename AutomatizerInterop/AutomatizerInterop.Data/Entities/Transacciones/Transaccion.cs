using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Entities.Transacciones
{

    public enum Modulos
    {
        Administracionn =1,
        Bodega =2,
        Compras = 3,
        Ventas = 8,
        Bancos = 7,
        Contabilidad=10
    }

    public class Transaccion
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public Modulos Modulo { get; set; }
        public bool Devolucion { get; set; }
        public int DiasCredito { get; set; }
    }
}
