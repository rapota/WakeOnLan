using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Serilog;
using Serilog.Events;

namespace WakeOnLan
{
    public static class Wol
    {
        private static readonly ILogger logger = Log.ForContext(typeof(Wol));

        public static void Run(Options options)
        {
            PhysicalAddress physicalAddress;
            IPAddress ip;

            //TODO: using (NestedDiagnosticsContext.Push("Parsing parameters."))
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

            if (logger.IsEnabled(LogEventLevel.Information))
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

                logger.Information(message.ToString());
            }

            //TODO: using (NestedDiagnosticsContext.Push("Sending packages."))
            using (UdpClient udpClient = new UdpClient())
            {
                byte[] package = CreatePackage(physicalAddress.GetAddressBytes());
                IPEndPoint endPoint = new IPEndPoint(ip, options.Port);

                try
                {
                    for (int i = 0; i < options.Repeate; i++)
                    {
                        udpClient.Send(package, package.Length, endPoint);
                        logger.Information("The package #{0} was send.", i + 1);

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
                StringBuilder sb = new StringBuilder(macAddress)
                    .Replace(" ", string.Empty)
                    .Replace(":", string.Empty)
                    .Replace("-", string.Empty);
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
