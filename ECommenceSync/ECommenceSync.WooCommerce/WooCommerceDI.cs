using ECommenceSync.WooCommerce.Helpers;
namespace Microsoft.Extensions.DependencyInjection
{
    public static class WooCommerceDI
    {
        public static void AddWooCommerce(this IServiceCollection services)
        {
            services.AddSingleton(typeof(ECommenceSync.WooCommerce.WooCommerce), typeof(ECommenceSync.WooCommerce.WooCommerce));
            services.AddSingleton(typeof(IWooCommerceDatabaseHelper), typeof(WooCommerceDatabaseHelper));
            services.AddSingleton(typeof(IWooCommerceOperationsHelper), typeof(WooCommerceOperationsHelper));
        }
    }
}
