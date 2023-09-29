using Microsoft.Extensions.Configuration;

namespace ECommenceSync.AutomatizerSQL.Helpers
{
    public interface IAutomatizerSQLOperationsHelper
    {
        int GetSearchTime(ECommenceSync.Operations operation);
        IConfiguration GetConfiguration();
    }

    public class AutomatizerSQLOperationsHelper : IAutomatizerSQLOperationsHelper
    {
        private readonly IConfigurationSection _automatizerSection;

        //private ConcurrentDictionary<ECommenceSync.Operations, int> searchTImes = new ConcurrentDictionary<ECommenceSync.Operations, int>();

        public AutomatizerSQLOperationsHelper(IConfiguration configuration)
        {
            _automatizerSection = configuration.GetSection("ECommenceSync") .GetSection("Erps").GetSection("AutomatizerSQL");
        }

        public IConfiguration GetConfiguration()
        {
            return _automatizerSection;
        }

        public int GetSearchTime(ECommenceSync.Operations operation)
        {
            int searchTime = 0;
            var opSection = _automatizerSection.GetSection("Operations").GetSection($"{operation.ToString()}");
            if (opSection.Exists() && opSection.GetSection("SearchTime").Exists())
            {
                searchTime = int.Parse(opSection["SearchTime"]);
            }
            else
            {
                searchTime = 5000;
            }
            
            return searchTime;
        }
    }
}
