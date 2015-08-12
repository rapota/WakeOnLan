using CommandLine;
using CommandLine.Text;

namespace WakeOnLAN
{
    public class Options
    {
        [Option('m', "mac", Required = true, HelpText = "MAC addres.")]
        public string MacAddress { get; set; }

        [Option('n', "host", HelpText = "Host name or IP address.")]
        public string Host { get; set; }

        [Option('p', "port", DefaultValue = 9, HelpText = "Port number.")]
        public int Port { get; set; }

        [Option('r', "repeate", DefaultValue = 1, HelpText = "Repeat request N times.")]
        public int Repeate { get; set; }

        [Option('d', "delay", DefaultValue = 1000, HelpText = "Delay between requests.")]
        public int Delay { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}