using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Interfaces
{
    public interface IEntity
    {
        public string GetFindByIdSql();
        public string GetQuerySql();
    }


}
