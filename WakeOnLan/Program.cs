using System;
using System.Collections.Generic;
using CommandLine;
using Serilog;

namespace WakeOnLan
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
            ILogger logger = Log.ForContext(typeof(Program));
            logger.Information("Starting.");

            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(Wol.Run)
                .WithNotParsed(HandleParseError);
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            foreach (Error error in errs)
            {
                Log.Logger.Error(error.ToString());
            }
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
