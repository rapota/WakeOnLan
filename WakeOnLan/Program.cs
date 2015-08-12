﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using CommandLine;
using NLog;

namespace WakeOnLAN
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
                logger.Error("Domain unhandled error. {0}", e.ExceptionObject);
            }

            LogManager.Flush();
        }

        private static void Run(Options options)
        {
            PhysicalAddress physicalAddress;
            IPAddress ip;

            using (NestedDiagnosticsContext.Push("Parsing parameters."))
            {
                StringBuilder sb = new StringBuilder(options.MacAddress);
                sb.Replace(" ", string.Empty);
                sb.Replace(":", string.Empty);
                physicalAddress = PhysicalAddress.Parse(sb.ToString());
                logger.Info("MAC adress: {0}", physicalAddress);

                if (options.Host == null)
                {
                    ip = IPAddress.Broadcast;
                    logger.Info("Using broadcast IP address: {0}", ip);
                }
                else
                {
                    if (IPAddress.TryParse(options.Host, out ip))
                    {
                        logger.Info("IP address: {0}", ip);
                    }
                    else
                    {
                        IPAddress[] addresses = ResolveAddresses(options.Host);
                        if (addresses != null && addresses.Length > 0)
                        {
                            ip = addresses[0];
                            logger.Info("Host name '{0}' resolved as IP address: {1}", options.Host, ip);
                        }
                        else
                        {
                            logger.Error("Failed to resolve host name '{0}' with DNS.", options.Host);
                            return;
                        }
                    }
                }

                logger.Info("Port number: {0}", options.Port);
                logger.Info("Repeates: {0}", options.Repeate);
                logger.Info("Delay: {0}", options.Delay);
            }

            using (NestedDiagnosticsContext.Push("Sending packages."))
            using (UdpClient udpClient = new UdpClient())
            {
                byte[] package = CreatePackage(physicalAddress.GetAddressBytes());
                IPEndPoint endPoint = new IPEndPoint(ip, options.Port);

                for (int i = 0; i < options.Repeate; i++)
                {
                    logger.Info("Sending package #{0}...", i + 1);
                    udpClient.Send(package, package.Length, endPoint);

                    if ((i + 1) < options.Repeate)
                    {
                        Thread.Sleep(options.Delay);
                    }
                }
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
