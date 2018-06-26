using System;
using ContentPublisherLib;
using System.IO;

namespace ContentPublisher
{
    class Program
    {
        static void Main(string[] args)
        {
            PublisherAppOptions options = PublisherAppOptions.GetPublisherAppOptionsFromArgs(args);

            Publisher pub;

            try
            {
                if (options.ShowHelp)
                {
                    Console.WriteLine(PublisherAppOptions.GetUsage());
                }
                if (!String.IsNullOrEmpty(options.ConfigPath) && File.Exists(options.ConfigPath))
                {
                    pub = new Publisher(options.ConfigPath);

                    pub.ActionDone += PrintProgress;

                    pub.Run();
                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[FATAL ERROR]: {e.StackTrace}");
            }
        }

        static void PrintProgress(int current, int all, PublisherAction action)
        {
            if (action.Result.ResultType == PublisherTypes.ActionResultType.Successful)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"[SUCCESS]({current}/{all}): {action.Result.ResultMessage}");
                Console.ResetColor();
            }
            if (action.Result.ResultType == PublisherTypes.ActionResultType.WithWarnings)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"[WARNING]({current}/{all}): {action.Result.ResultMessage}");
                Console.ResetColor();
            }
            if (action.Result.ResultType == PublisherTypes.ActionResultType.Failed)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"[FAILED]({current}/{all}): {action.Result.ResultMessage}");
                Console.ResetColor();
            }
        }
    }


}
