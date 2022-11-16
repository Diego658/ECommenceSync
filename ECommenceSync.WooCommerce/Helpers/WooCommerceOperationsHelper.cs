using Microsoft.Extensions.Configuration;
using System;

namespace ECommenceSync.WooCommerce.Helpers
{
    public class WooCommerceOperationsHelper : IWooCommerceOperationsHelper
    {
        readonly IConfigurationSection _configuration;
        private readonly int _operationsWorkQueueWaitTime;
        private readonly string _taxClassIva0;
        private readonly string _taxClassIva12;
        private int _hoursToAdjust;

        public IConfigurationSection Configuration => _configuration;

        public int HoursToAdjust => _hoursToAdjust;

        public int OperationsWorkQueueWaitTime => _operationsWorkQueueWaitTime;

        public string TaxClassIva0 => _taxClassIva0;

        public string TaxClassIva12 => _taxClassIva12;

        public WooCommerceOperationsHelper(IConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _configuration = configuration.GetSection("ECommenceSync").GetSection("Stores").GetSection("WooCommerce");
            if (!_configuration.Exists())
            {
                throw new InvalidOperationException("No hay configuraciones para WooCommerce");
            }
            _hoursToAdjust = _configuration.GetValue<int>("HoursToAdjust", 0);
            _operationsWorkQueueWaitTime = _configuration.GetValue<int>(nameof(OperationsWorkQueueWaitTime), 250);
            _taxClassIva0 = _configuration.GetValue<string>("TaxClassIva0");
            _taxClassIva12 = _configuration.GetValue<string>("TaxClassIva12");
        }

        public int GetMaxRetryCount(ECommenceSync.Operations operation)
        {
            var opSection = _configuration.GetSection("Operations").GetSection($"{operation}").GetSection("MaxRetryCount");
            return opSection.Exists() ? int.Parse(opSection.Value) : 10;
        }

        public int GetSearchTime(ECommenceSync.Operations operation)
        {
            int searchTime = 20000;
            var opSection = _configuration.GetSection("Operations").GetSection($"{operation}");
            if (opSection.Exists() && opSection.GetSection("SearchTime").Exists())
            {
                searchTime = int.Parse(opSection["SearchTime"]);
            }
            return searchTime;
        }
    }
}
