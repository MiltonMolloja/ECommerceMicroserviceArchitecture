using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Cart.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Enable legacy timestamp behavior for Npgsql (allows DateTime without timezone)
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
