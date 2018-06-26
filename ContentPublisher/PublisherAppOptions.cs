using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentPublisher
{
    public class PublisherAppOptions
    {
        public string ConfigPath { get; set; }
        public bool ShowHelp { get; set; }

        /// <summary>
        /// Creates a PublisherAppOptions from the command line arguments
        /// </summary>
        /// <param name="args">Array of command line arguments</param>
        /// <returns>PublisherAppOptions with values from command line arguments</returns>
        public static PublisherAppOptions GetPublisherAppOptionsFromArgs(string[] args)
        {
            PublisherAppOptions options = new PublisherAppOptions();

            if (args.Length == 0)
            {
                options.ShowHelp = true;
            }
            for (int i = 0; i != args.Length; ++i)
            {
                if (args[i].Equals(String.Empty)) continue;

                if (args[i].Equals("-help"))
                {
                    options.ShowHelp = true;
                    continue;
                }

                if ((args[i] == "-config-path") && (i + 1 <= (args.Length - 1)))
                {
                    options.ConfigPath = args[++i];
                    continue;
                }
            }

            return options;
        }

        /// <summary>
        /// Gets the usage text to be printed for users
        /// </summary>
        /// <returns>Program usage as string</returns>
        public static string GetUsage()
        {
            StringBuilder usage = new StringBuilder();

            usage.AppendLine("ContentPublisherApp.exe - Copies and archives files and directories.");
            usage.AppendLine("Usage:");
            usage.AppendLine("ContentPublisherApp.exe");
            usage.AppendLine("    -config-path <configFile>    : Path to the config file");
            usage.AppendLine("    -help                        : Prints the usage");

            return usage.ToString();
        }
    }

    
}
