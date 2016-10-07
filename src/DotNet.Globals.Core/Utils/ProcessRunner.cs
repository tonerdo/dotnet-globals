namespace DotNet.Globals.Core.Utils
{
    using System.Diagnostics;

    internal class ProcessRunner
    {
        public static bool RunProcess(string processName, params string[] arguments)
        {
            Process process = new Process();
            process.StartInfo.FileName = processName;
            process.StartInfo.Arguments = string.Join(" ", arguments);
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            while (!process.HasExited);
            return process.ExitCode == 0;
        }
    }
}