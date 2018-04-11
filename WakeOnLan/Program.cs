using System;
using System.Collections.Generic;
using CommandLine;
using NLog;
using WakeOnLan.Standard;

namespace WakeOnLan
{
    class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            Console.WriteLine("Starting.");
            logger.Error("Starting.");

            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(Wol.Run)
                .WithNotParsed(HandleParseError);
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            foreach (Error error in errs)
            {
                logger.Error(error);
            }
        }

        private static void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                logger.Error(ex, "Domain unhandled exception.");
            }
            else
            {
                logger.Error("Domain unhandled error: {0}", e.ExceptionObject);
            }

            LogManager.Flush();
        }
    }
}
