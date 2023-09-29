using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace AutomatizerInterop.Facturacion
{
    public class AsientoContable
    {
        List<DetalleAsiento> _detalles;
        public ReadOnlyCollection<DetalleAsiento> Detalles { get => _detalles.AsReadOnly(); }

        public decimal TotalDebe { get => decimal.Round( _detalles.Where(x => x.TipoCuenta == TiposCuenta.Debe).Sum(x => x.Valor), 2) ; }
        public decimal TotalHaber{ get => decimal.Round( _detalles.Where(x => x.TipoCuenta == TiposCuenta.Haber).Sum(x => x.Valor), 2) ; }

        public AsientoContable()
        {
            _detalles = new List<DetalleAsiento>();
        }


        public DetalleAsiento AddDetalle(string cuenta, TiposCuenta tipo, decimal valor)
        {
            if (string.IsNullOrEmpty(cuenta))
            {
                throw new ArgumentNullException(nameof(cuenta));
            }
            if (valor ==0)
            {
                throw new InvalidOperationException("El valor no puede ser 0");
            }
            if (valor < 0)
            {
                throw new InvalidOperationException("El valor no puede ser <= 0");
            }


            var detalle = _detalles.FirstOrDefault(d => d.TipoCuenta == tipo && d.Cuenta == cuenta);
            if (_detalles.Any(d => d.TipoCuenta == tipo && d.Cuenta == cuenta))
            {
                detalle.Valor += valor; 
            }
            else
            {
                detalle = new DetalleAsiento { TipoCuenta = tipo, Cuenta = cuenta, Valor = valor };
                _detalles.Add(detalle);
            }
            return detalle;
        }
        public void Validar()
        {
            foreach (var detalle in _detalles)
            {
                detalle.Valor = decimal.Round(detalle.Valor, 2);
            }
            if (TotalDebe != TotalHaber)
            {
                throw new InvalidOperationException($"El asiento no cuadra Debe={TotalDebe}, Haber= {TotalHaber}");
            }
        }


    }


    public class DetalleAsiento
    {
        public string Cuenta { get; set; }
        public TiposCuenta TipoCuenta { get; set; }
        public decimal Valor { get; set; }
    }
}
