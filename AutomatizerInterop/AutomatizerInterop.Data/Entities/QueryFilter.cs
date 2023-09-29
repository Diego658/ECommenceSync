using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Entities
{

    public enum FilterTypes
    {
        Equals = 0,
        Begins = 1,
        Ends = 2,
        Contains = 3,
        BetWeen
    }

    public enum Operators
    {
        AND = 0,
        OR = 1
    }

    public class QueryFilter
    {
        public string Field { get; set; }
        public FilterTypes FilterType { get; set; }
        public Operators NextOperator { get; set; }
        public string Value1 { get; set; }
        public string Value2 { get; set; }


    }


    public sealed class QueryField
    {
        public string FieldAlias { get; set; }
        public string FieldDbName { get; set; }
    }



}
