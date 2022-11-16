using ECommenceSync.WooCommerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ECommenceSync.WooCommerce
{
    internal static class WooComerceErrorHelper
    {
        public static bool TryGetWooErrorInfo(this Exception ex, out WooApiErrorInfo apiErrorInfo)
        {
            try
            {
                apiErrorInfo = JsonSerializer.Deserialize<WooApiErrorInfo>(ex.Message, new JsonSerializerOptions() { PropertyNameCaseInsensitive= true } );
                return apiErrorInfo == null? false:true;
            }
            catch (Exception)
            {
                apiErrorInfo=null;
                return false;
            }
        } 
    }
}
