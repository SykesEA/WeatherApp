using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Serilog;
using Serilog.Formatting.Json;

namespace WeatherApplicationProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
          
                .WriteTo.Console()
                .WriteTo.File(new JsonFormatter(),"logs/log.txt" , 
                 restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                 rollingInterval: RollingInterval.Day)
                .WriteTo.File("logs/errorlog.txt",
                 restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning)
                .CreateLogger();
            try
            {
                Log.Information("Starting our service");
                var host = CreateHostBuilder(args).Build();
                
                var weather = ActivatorUtilities.CreateInstance<Weather>(host.Services);
                weather.Run();

            }
            catch(Exception ex)
            {
                Log.Fatal(ex, "Exception in application");
            }
            finally
            {
                Log.Information("Exiting Service");
                Log.CloseAndFlush();
            }

        }
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseSerilog()   
                .ConfigureAppConfiguration((context, configuration) =>
                {
                    configuration.Sources.Clear();
                    configuration.AddJsonFile("appsettings.json", optional:true,reloadOnChange: true);
                    configuration.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true);

                });
        }
       
    }

}