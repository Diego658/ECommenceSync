using AutomatizerInterop.Data.EntityFramewrok;
using AutomatizerInterop.Data.Entities;
using AutomatizerInterop.Data.Helper;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutomatizerInterop.Data.Interfaces
{

    public interface IRepository
    {
        public int ConfiguracionID { get; set; }
        public IInteropConfiguracionProvider ConfiguracionProvider { get; set; }
        IDatabase GetSQLDatabase();
        IDatabase GetSQLDatabase(int configuracionID);
    }


    public interface IQueryableRepository
    {
        public List<QueryField>  GetFieldsFilters();
        //public Tuple<string, object[]> GetWhereSQL(); 
    }

    public class GenericRepository : IRepository
    {
        public int ConfiguracionID { get; set; }

        public IInteropConfiguracionProvider ConfiguracionProvider { get; set; }

        


        /// <summary>
        /// El campo ConfiguracionID debe ser establecido manualmente antes de llamar a la funcion
        /// </summary>
        /// <returns></returns>
        public IDatabase GetSQLDatabase()
        {
            if (ConfiguracionID == 0)
            {
                throw new InvalidOperationException("No se establecio el id de Configuracion!!!!");
            }
            return GetSQLDatabase(this.ConfiguracionID);
        }

        public IDatabase GetSQLDatabase(int configuracionID)
        {
            var configuracion = ConfiguracionProvider.GetConfiguracion(configuracionID);
            var database = DatabaseHelper.GetSqlDatabase(configuracion);
            return database;
        }
    }



    public interface IAutoConfigRepository
    {
        IRepository GenericRepository { get; set; }
    }

    public class AutoConfigRepository : IAutoConfigRepository, IRepository
    {
        private readonly AutomatizerInteropDbContext dbContext;

        public int ConfiguracionID { get => GenericRepository.ConfiguracionID; set => throw new NotImplementedException(); }
        public ConfiguracionProgramaFacturacionElectronica Configuracion { get => GenericRepository.ConfiguracionProvider.GetConfiguracion(GenericRepository.ConfiguracionID); }
        public string CodigoEmpresa { get => Configuracion.CodigoEmpresa; }
        public IInteropConfiguracionProvider ConfiguracionProvider { get => GenericRepository.ConfiguracionProvider; set => throw new NotImplementedException(); }
        public IRepository GenericRepository { get; set; }

        public string GetConnectionString { get => DatabaseHelper.GetSqlConnectionString(GenericRepository.ConfiguracionProvider.GetConfiguracion(GenericRepository.ConfiguracionID) ); }
        public AutoConfigRepository(IInteropConfiguracionProvider configuracionProvider, IRepository genericRepository,
            AutomatizerInteropDbContext dbContext)
        {
            genericRepository.ConfiguracionProvider = configuracionProvider;
            GenericRepository = genericRepository;
            this.dbContext = dbContext;
            this.dbContext.ConfiguracionId = genericRepository.ConfiguracionID;
        }


        public IDatabase GetSQLDatabase()
        {
            return GenericRepository.GetSQLDatabase();
        }

        public IDatabase GetSQLDatabase(int configuracionID)
        {
            return GenericRepository.GetSQLDatabase(configuracionID);
        }


        private object[] GetNewArgs(params object[] args)
        {
            var newArgs = new object[args.Length +1];
            newArgs[0] = CodigoEmpresa;
            Array.Copy(args, 0, newArgs, 1, args.Length);
            return newArgs;
        }

        public Task<IAsyncReader<T>> QueryAsync<T>(string sql, params object[] args) where T : class
        {
            var db = GetSQLDatabase();
            return db.QueryAsync<T>(sql,  GetNewArgs( args));
        }


        public async Task<List<T>> QueryAsyncToList<T>(string sql, params object[] args) where T : class
        {
            var lista = Activator.CreateInstance<List<T>>();
            using (var db = GetSQLDatabase())
            {
                using (var reader = await db.QueryAsync<T>(sql, GetNewArgs(args)))
                {
                    while (await reader.ReadAsync() )
                    {
                        lista.Add(reader.Poco);
                    }
                }
            }
            
            return lista;
        }


        public Task<T> SingleAsync<T>(string sql, params object[] args) where T : class
        {
            var db = GetSQLDatabase();
            return db.SingleAsync<T>(sql, GetNewArgs(args));
        }


        public Task<T> FirstAsync<T>(string sql, params object[] args) where T : class
        {
            var db = GetSQLDatabase();
            return db.FirstAsync<T>(sql, GetNewArgs(args));
        }


        public async Task<int> ExecuteAsync(string sql, params object[] args)
        {
            using (var db = GetSQLDatabase())
            {
                return await db.ExecuteAsync(sql, GetNewArgs(args));
            }
        }
     


        public async Task<int> ExecuteNonQueryProcAsync(string sql, params object[] args)
        {
            using (var db = GetSQLDatabase())
            {
                return await db.ExecuteNonQueryProcAsync(sql, args);
            }
        }


        public AutomatizerInteropDbContext DbContext { get=> dbContext;  }


    }





}
