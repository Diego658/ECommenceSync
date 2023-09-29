using AutomatizerInterop.Data.Entities;
using AutomatizerInterop.Data.Helper;
using AutomatizerInterop.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace AutomatizerInterop.Data.Repositories
{
    public class UsaPrimeraConfiguracionUsuariosRepository : IUsuariosRepository
    {
        private readonly IInteropConfiguracionProvider configuracionProvider;

        public UsaPrimeraConfiguracionUsuariosRepository(IInteropConfiguracionProvider configuracionProvider)
        {
            this.configuracionProvider = configuracionProvider;
        }


        public User GetUser(string userName, string password)
        {
            var configuracion = configuracionProvider.GetConfiguracion(1);
            using (var database = DatabaseHelper.GetSqlDatabase(configuracion))
            {
                return database.FirstOrDefault<User>(@"SELECT Id = SgrUsr.EplSec, Username  = SgrUsr,  FirstName = EplMae.EplNom, LastName = EplMae.EplApl, Password = SgrUsr.SgrClvUsr, Token = 'Token'
                FROM SgrUsr INNER JOIN
                EplMae ON EplMae.EmpCod = SgrUsr.EmpCod AND EplMae.EplSec = SgrUsr.EplSec
                WHERE SgrUsr.EmpCod = @0 AND SgrUsr.SgrEst = 1 AND SgrUsr.SgrClvUsr = @1", configuracion.CodigoEmpresa, Encriptar(userName.ToLower() + "~" + password) );
            }
        }


        private string Encriptar(string texto)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(texto);
                MD5CryptoServiceProvider cryptoServiceProvider1 = new MD5CryptoServiceProvider();
                byte[] hash = cryptoServiceProvider1.ComputeHash(Encoding.UTF8.GetBytes("Solinteg jwptadgt_158.."));
                cryptoServiceProvider1.Clear();
                TripleDESCryptoServiceProvider cryptoServiceProvider2 = new TripleDESCryptoServiceProvider();
                cryptoServiceProvider2.Key = hash;
                cryptoServiceProvider2.Mode = CipherMode.ECB;
                cryptoServiceProvider2.Padding = PaddingMode.PKCS7;
                byte[] inArray = cryptoServiceProvider2.CreateEncryptor().TransformFinalBlock(bytes, 0, bytes.Length);
                cryptoServiceProvider2.Clear();
                texto = Convert.ToBase64String(inArray, 0, inArray.Length);
            }
            catch (Exception ex)
            {
            }
            return texto;
        }



    }
}
