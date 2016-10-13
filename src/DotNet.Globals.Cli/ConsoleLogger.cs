namespace DotNet.Globals.Cli
{
    using System;
    using DotNet.Globals.Core.Logging;

    class ConsoleLogger : ILogger
    {
        private void Log(string data) => Console.WriteLine(data);

        public void LogSuccess(string data) => Log($"LOG: {data}");

        public void LogError(string data)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Log(data);
            Console.ResetColor();
        }

        public void LogInformation(string data) => Log($"INFO: {data}");

        public void LogVerbose(string data) { }

        public void LogWarning(string data) => Log($"WARNING: {data}");
    }
}