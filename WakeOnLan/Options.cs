using CommandLine;

namespace WakeOnLan
{
    public class Options
    {
        [Option('m', "mac", Required = true, HelpText = "MAC addres.")]
        public string MacAddress { get; set; }

        [Option('n', "host", HelpText = "Host name or IP address.")]
        public string Host { get; set; }

        [Option('p', "port", Default = 9, HelpText = "Port number.")]
        public int Port { get; set; }

        [Option('r', "repeate", Default = 1, HelpText = "Repeat request N times.")]
        public int Repeate { get; set; }

        [Option('d', "delay", Default = 1000, HelpText = "Delay between requests.")]
        public int Delay { get; set; }
    }
}