using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace WakeOnLan
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
            ILogger logger = Log.ForContext(typeof(Program));
            try
            {
                AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;

                Options options = ParseOptions(args);
                if (options == null)
                {
                    logger.Fatal("Invalid command line.");
                    return;
                }

                logger.Information("Application started.");

                await Wol.RunAsync(options);
                logger.Information("Application stopped.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static Options ParseOptions(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            Options options = configuration.Get<Options>();
            if (options?.MacAddress == null)
            {
                return null;
            }

            if (options.Port == 0)
            {
                options.Port = 9;
            }

            if (options.Repeate == 0)
            {
                options.Repeate = 1;
            }

            if (options.Delay == 0)
            {
                options.Delay = 1000;
            }

            return options;
        }

        private static void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Log.Logger.Error(ex, "Domain unhandled exception.");
            }
            else
            {
                Log.Logger.Error("Domain unhandled error: {0}", e.ExceptionObject);
            }
        }
    }
}
