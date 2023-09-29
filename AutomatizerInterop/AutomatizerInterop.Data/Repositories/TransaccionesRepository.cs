using AutomatizerInterop.Data.EntityFramewrok;
using AutomatizerInterop.Data.Entities.Transacciones;
using AutomatizerInterop.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutomatizerInterop.Data.Repositories
{
    public class TransaccionesRepository : AutoConfigRepository, ITransaccionesRepository
    {
        public TransaccionesRepository(IInteropConfiguracionProvider configuracionProvider, IRepository genericRepository, AutomatizerInteropDbContext dbContext)
            : base(configuracionProvider, genericRepository, dbContext)
        {
        }

        public Task<Transaccion> GetTransaccion(string codigo, byte modulo)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Transaccion>> GetTransacciones(byte modulo, string categoria)
        {
            return await QueryAsyncToList<Transaccion>(@"SELECT TrnCod As Codigo, TrnDes As Nombre, Dev As Devolucion
            FROM Trn
            WHERE (EmpCod = @0) AND (ModIde = @1) AND TrnCat = @2 ORDER BY TrnDes", modulo, categoria);
        }
    }
}
