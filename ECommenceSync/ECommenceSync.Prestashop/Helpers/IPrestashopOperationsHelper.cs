using Microsoft.Extensions.Configuration;
using System;

namespace ECommenceSync.Prestashop.Helpers
{
    public interface IPrestashopOperationsHelper
    {
        IConfigurationSection Configuration { get; }
        int GetMaxRetryCount(ECommenceSync.Operations operation);
        int GetSearchTime(ECommenceSync.Operations operation);
        int HoursToAdjust { get; }
    }

    public class PrestashopOperationsHelper: IPrestashopOperationsHelper
    {
        readonly IConfigurationSection _configuration;
        public PrestashopOperationsHelper(IConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _configuration = configuration.GetSection("ECommenceSync").GetSection("Stores").GetSection("Prestashop");
            if(!_configuration.Exists())
            {
                throw new InvalidOperationException("No hay configuraciones para Prestashop");
            }
            if (int.TryParse(_configuration["HoursToAdjust"],out var hours ))
            {
                HoursToAdjust = hours;
            }
            else
            {
                HoursToAdjust = 0;
            }
            
        }

        public IConfigurationSection Configuration { get => _configuration; }

        int IPrestashopOperationsHelper.GetMaxRetryCount(ECommenceSync.Operations operation)
        {
            var opSection = _configuration.GetSection("Operations").GetSection($"{operation}").GetSection("MaxRetryCount");
            return opSection.Exists()  ? int.Parse(opSection.Value)  : 10;
        }
        

        int IPrestashopOperationsHelper.GetSearchTime(ECommenceSync.Operations operation)
        {
            int searchTime = 20000;
            var opSection = _configuration.GetSection("Operations").GetSection($"{operation}");
            if (opSection.Exists() && opSection.GetSection("SearchTime").Exists())
            {
                searchTime = int.Parse(opSection["SearchTime"]);
            }
            return searchTime;
        }

        public int HoursToAdjust { get; }
    }

}

