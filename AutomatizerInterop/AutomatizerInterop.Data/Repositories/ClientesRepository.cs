using AutomatizerInterop.Data.EntityFramewrok;
using AutomatizerInterop.Data.Entities;
using AutomatizerInterop.Data.Helper;
using AutomatizerInterop.Data.Interfaces;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutomatizerInterop.Data.Repositories
{
    public class ClientesRepository : AutoConfigRepository, IClientesRepository, IQueryableRepository
    {
        private static List<QueryField> queryFields;

        public List<QueryField> GetFieldsFilters()
        {
            if (queryFields == null)
            {
                queryFields = new List<QueryField>
                {
                    new QueryField
                    {
                         FieldAlias = "NombreCompleto",
                         FieldDbName = "climae.NombreCompleto"
                    },
                    new QueryField
                    {
                        FieldAlias = "Identificacion",
                        FieldDbName = "climae.Clicedruc"
                    },
                    new QueryField
                    {
                        FieldAlias = "Estado",
                        FieldDbName = "climae.clista"
                    }
                };
            }
            return queryFields;
        }

        public ClientesRepository(IInteropConfiguracionProvider configuracionProvider, IRepository genericRepository,AutomatizerInteropDbContext dbContext)
            : base(configuracionProvider, genericRepository, dbContext)
        {
        }

        public async Task<ClienteAutomatizer> GetCliente(string codigoCliente, bool fullData)
        {
            if (fullData)
            {
                throw new NotImplementedException();
            }
            else
            {
                var cliente = await SingleAsync<ClienteAutomatizer>(@"SELECT climae.Clisec As Codigo, CliNom As Nombres, CliApl As Apellidos, Clicedruc As  Identificacion , Estado = cast(climae.clista as bit), PersonaNatural = cast(pernat as bit), NombreComercial = nomcom, Genero = CASE CliSex WHEn 0 THEN 'Masculino' ELSE 'Femenino' END,
                Email = ISNULL(CliEma , '')
                FROM climae INNER JOIN Clisuc ON climae.EMpCod = clisuc.empcod and climae.clisec = clisuc.CliSec and 1 = clisuc.sucnum
                where climae.EmpCod = @0 AND climae.Clisec = @1", codigoCliente);
                return cliente;
            }
            
        }

        public async Task<List<ClienteAutomatizer>> GetClientes(List<QueryFilter> queryFilters)
        {

            var whereSql = EntityQueryHelper.GetWhereSQL(this, queryFilters);

            using (var reader = await QueryAsync<ClienteAutomatizer>(@"SELECT Clisec As Codigo, CliNom As Nombres, CliApl As Apellidos, Clicedruc As  Identificacion , Estado = cast(clista as bit), PersonaNatural = cast(pernat as bit), NombreComercial = nomcom, Genero = CASE CliSex WHEn 0 THEN 'Masculino' ELSE 'Femenino' END
            FROM climae
            where EmpCod = @0" + whereSql.Item1, whereSql.Item2 ))
            {
                var clientes = new List<ClienteAutomatizer>(100);

                while (await reader.ReadAsync())
                {
                    clientes.Add(reader.Poco);
                }

                return clientes;
            }

        }

        public async Task<List<ContactoCliente>> GetContactosCliente(string codigoCliente)
        {

            var result = new List<ContactoCliente>(5);
            using (var reader = await QueryAsync<ContactoCliente>(@"SELECT TipoContactoID= cc.TipCod, SucursalID =  CliSec + '-' + CAST(SucNum as varchar(3)) +  '-' + CAST(cc.TipCod as varchar(3)) ,  ClienteCodigo = cc.clisec, SucursalNumero = SucNum, TipoContacto=  tc.Descripcion, Contacto = Numero, Persona = NomCon
            FROM CliComunic cc
            INNER JOIN 
            TiposComunic tc ON cc.TipCod = tc.TipCod
            WHERE EmpCod = @0 AND CliSec = @1", codigoCliente))
            {
                while (await reader.ReadAsync())
                {
                    result.Add(reader.Poco);
                }
            }
            
            return result;
        }

        public Task UpdateEmail(string clienteCodigo, byte sucrusalId, string email)
        {
            return ExecuteAsync("UPDATE CliSuc SET CliEma = @3 WHERE EmpCod = @0 AND CliSec = @1 AND SucNum = @2", clienteCodigo, sucrusalId, email);
        }
    }
}
