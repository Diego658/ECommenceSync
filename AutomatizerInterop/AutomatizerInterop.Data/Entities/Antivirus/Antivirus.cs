using AutomatizerInterop.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Entities.Antivirus
{
    public class Antivirus : IEntity
    {
        #region IEntity

        private const string findByIdSql = @"SELECT ItmMae.ItmCod, ItmMae.ItmCodVen, ItmMae.Secuencial, ItmMae.ItmDsc
        FROM Antivirus INNER JOIN
             ItmMae ON Antivirus.IdItem = ItmMae.Secuencial
        WHERE (ItmMae.EmpCod = @0) AND (ItmMae.ItmCodVen = @1)";


        private const string querySql = @"SELECT ItmMae.ItmCod, ItmMae.ItmCodVen, ItmMae.Secuencial, ItmMae.ItmDsc
        FROM Antivirus INNER JOIN
             ItmMae ON Antivirus.IdItem = ItmMae.Secuencial
        WHERE (ItmMae.EmpCod = @0) ORDER BY ItmMae.ItmDsc";

        private static readonly IEntity entity= new Antivirus();

        public string GetFindByIdSql()
        {
            return findByIdSql; ;
        }

        public string GetQuerySql()
        {
            return querySql;
        }


        public static IEntity GetDefaultEntity()
        {
            return entity;
        }
        #endregion


        [PetaPoco.Column("ItmCodVen")]
        public string Id { get; set; }
        [PetaPoco.Column("ItmCod")]
        public string Codigo { get; set; }
        [PetaPoco.Column("Secuencial")]
        public int Secuencial { get; set; }
        [PetaPoco.Column("ItmDsc")]
        public string Nombre { get; set; }


    }
}
