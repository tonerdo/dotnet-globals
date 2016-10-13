namespace DotNet.Globals.Cli
{
    using System;
    using DotNet.Globals.Core.Logging;

    class ConsoleLogger : ILogger
    {
        private void Log(string data)
        {
            Console.WriteLine(data);
        }

        public void LogSuccess(string data)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Log(data);
            Console.ResetColor();
        }

        public void LogError(string data)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Log(data);
            Console.ResetColor();
        }

        public void LogInformation(string data) => Log(data);

        public void LogVerbose(string data) => Log(data);

        public void LogWarning(string data)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Log(data);
            Console.ResetColor();
        }
    }
}