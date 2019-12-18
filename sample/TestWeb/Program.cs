using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using NewRelic.LogEnrichers.Serilog;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.NewRelic;

namespace TestWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseSerilog(ConfigureSerilog)
                .UseStartup<Startup>();

        private static void ConfigureSerilog(WebHostBuilderContext hostingContext, LoggerConfiguration loggerConfiguration)
        {
            Serilog.Debugging.SelfLog.Enable(Console.Error);

            loggerConfiguration
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithNewRelicLogsInContext()
                .WriteTo.NewRelic(
                    
                    new NewRelicSinkOptions
                {
                    LicenseKey = hostingContext.Configuration["NewRelic.LicenseKey"] ??
                                 hostingContext.Configuration["NEW_RELIC_LICENSE_KEY"],
                    InsertKey = hostingContext.Configuration["NewRelic.InsertKey"] ??
                                hostingContext.Configuration["NEW_RELIC_INSERT_KEY"],
                });
        }
    }
}
