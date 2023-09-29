using AutomatizerInterop.Data.Entities;
using AutomatizerInterop.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Helper
{
    public static class EntityQueryHelper
    {
        public static Tuple<string, object[]> GetWhereSQL( IQueryableRepository repository, List<QueryFilter> queryFilters)
        {
            throw new NotImplementedException();
        }
    }
}
