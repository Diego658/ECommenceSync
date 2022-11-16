using ECommenceSync.AutomatizerSQL;
using ECommenceSync.Interfaces;
using Microsoft.Data.SqlClient;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AutomatizerSQLDI
    {
        public static void AddAutomatizerSQL(this IServiceCollection services)
        {
            services.AddSingleton(typeof(ECommenceSync.AutomatizerSQL.Helpers.IAutomatizerDataHelper), typeof(ECommenceSync.AutomatizerSQL.Helpers.AutomatizerDatabaseHelper));
            services.AddSingleton(typeof(ECommenceSync.AutomatizerSQL.Helpers.IAutomatizerSQLOperationsHelper), typeof(ECommenceSync.AutomatizerSQL.Helpers.AutomatizerSQLOperationsHelper));
            services.AddSingleton(typeof(IErp), typeof(AutomatizerSQL));
            services.AddSingleton(typeof(IDatabaseHelper<SqlConnection, int>), typeof(DatabaseHelper));
        }
    }
}
