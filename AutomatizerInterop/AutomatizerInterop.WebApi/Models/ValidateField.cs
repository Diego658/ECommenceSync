using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutomatizerInterop.WebApi.Models
{
    public class ValidateField<TValue, TKey> where TKey:struct
    {
        public TKey? Id { get; set; }
        public TValue Value { get; set; }
    }
}
