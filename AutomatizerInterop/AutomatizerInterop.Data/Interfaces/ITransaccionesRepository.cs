using AutomatizerInterop.Data.Entities.Transacciones;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutomatizerInterop.Data.Interfaces
{
    public interface ITransaccionesRepository
    {
        Task<List<Transaccion>> GetTransacciones(byte modulo, string categoria);
        Task<Transaccion> GetTransaccion(string codigo, byte modulo);

    }
}
