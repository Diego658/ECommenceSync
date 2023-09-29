using AutomatizerInterop.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutomatizerInterop.Data.Interfaces
{
    public interface IClientesRepository
    {
        Task<ClienteAutomatizer> GetCliente(string codigoCliente, bool fullData);
        Task<List<ClienteAutomatizer>> GetClientes(List<QueryFilter> queryFilters);
        Task<List<ContactoCliente>> GetContactosCliente(string codigoCliente);
        Task UpdateEmail(string clienteCodigo, byte sucrusalId, string email);
    }
}
