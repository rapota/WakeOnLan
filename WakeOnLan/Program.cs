using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using CommandLine;
using NLog;
using NLog.Fluent;

namespace WakeOnLan
{
    class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;

            Options options = new Options();
            if (!Parser.Default.ParseArguments(args, options))
            {
                string help = options.GetUsage();
                logger.Error(help);
            }
            else
            {
                Run(options);
            }
        }

        private static void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                logger.Error(ex, "Domain unhandled exception.");
            }
            else
            {
                logger.Error("Domain unhandled error: {0}", e.ExceptionObject);
            }

            LogManager.Flush();
        }

        private static void Run(Options options)
        {
            PhysicalAddress physicalAddress;
            IPAddress ip;

            using (NestedDiagnosticsContext.Push("Parsing parameters."))
            {
                physicalAddress = ParseAddress(options.MacAddress);
                if (physicalAddress == null)
                {
                    return;
                }

                if (options.Host == null)
                {
                    ip = IPAddress.Broadcast;
                }
                else
                {
                    if (!IPAddress.TryParse(options.Host, out ip))
                    {
                        IPAddress[] addresses = ResolveAddresses(options.Host);
                        if (addresses == null || addresses.Length == 0)
                        {
                            return;
                        }

                        ip = addresses[0];
                    }
                }
            }

            if (logger.IsInfoEnabled)
            {
                StringBuilder message = new StringBuilder();
                message.AppendLine("Initial parameters:");
                message.AppendFormat("MAC address: {0}", physicalAddress);
                message.AppendLine();
                message.AppendFormat("IP address: {0}", ip);
                message.AppendLine();
                message.AppendFormat("Port number: {0}", options.Port);
                message.AppendLine();
                message.AppendFormat("Repeates: {0}", options.Repeate);
                message.AppendLine();
                message.AppendFormat("Delay: {0}", options.Delay);

                logger.Info(message.ToString());
            }

            using (NestedDiagnosticsContext.Push("Sending packages."))
            using (UdpClient udpClient = new UdpClient())
            {
                byte[] package = CreatePackage(physicalAddress.GetAddressBytes());
                IPEndPoint endPoint = new IPEndPoint(ip, options.Port);

                try
                {
                    for (int i = 0; i < options.Repeate; i++)
                    {
                        udpClient.Send(package, package.Length, endPoint);
                        logger.Info("The package #{0} was send.", i + 1);

                        if ((i + 1) < options.Repeate)
                        {
                            Thread.Sleep(options.Delay);
                        }
                    }
                }
                catch (SocketException ex)
                {
                    logger.Error(ex, "Faile dto send pakage.");
                }
            }
        }

        private static PhysicalAddress ParseAddress(string macAddress)
        {
            try
            {
                StringBuilder sb = new StringBuilder(macAddress);
                sb.Replace(" ", string.Empty);
                sb.Replace(":", string.Empty);
                return PhysicalAddress.Parse(sb.ToString());
            }
            catch (FormatException ex)
            {
                logger.Error(ex, "Invalid MAC adress.");
                return null;
            }
        }

        private static IPAddress[] ResolveAddresses(string host)
        {
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(host);
                if (hostEntry.AddressList.Length > 0)
                {
                    return hostEntry.AddressList;
                }
            }
            catch (SocketException exception)
            {
                logger.Error(exception, "Failed to resolve host name '{0}' with DNS.", host);
            }
            catch (ArgumentException exception)
            {
                logger.Error(exception, "Failed to resolve host name '{0}' with DNS.", host);
            }

            return null;
        }

        private static byte[] CreatePackage(byte[] addressBytes)
        {
            List<byte> package = new List<byte>(102);

            for (int i = 0; i < 6; i++)
            {
                package.Add(0xff);
            }

            for (int j = 0; j < 16; j++)
            {
                package.AddRange(addressBytes);
            }

            return package.ToArray();
        }
    }
}
