using ECommenceSync.Prestashop.Helpers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class PrestashopDI
    {
        public static void AddPrestashop(this IServiceCollection services)
        {
            services.AddSingleton(typeof(ECommenceSync.Prestashop.Prestashop), typeof(ECommenceSync.Prestashop.Prestashop));
            services.AddSingleton(typeof(IPrestashopDatabaseHelper), typeof(PrestashopDatabaseHelper));
            services.AddSingleton(typeof(IPrestashopOperationsHelper), typeof(PrestashopOperationsHelper));
        }
    }
}
