using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommenceSync.WooCommerce.Models
{

    internal class WooApiErrorData
    {
        public int Status { get; set; }
        public int Resource_Id { get; set; }
    }
    internal class WooApiErrorInfo
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public WooApiErrorData Data { get; set; }

    }
}
