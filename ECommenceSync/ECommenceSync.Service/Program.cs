using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ECommenceSync.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
#if !DEBUG
                .UseWindowsService()
#endif
        .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
#if DEBUG
                    webBuilder.UseUrls("http://localhost:5010", "https://localhost:5020");
#endif
                });
    }
}
