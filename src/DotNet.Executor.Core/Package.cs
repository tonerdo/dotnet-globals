namespace DotNet.Executor.Core
{
    using System.IO;

    internal class Package
    {
        public DirectoryInfo Directory { get; set; }
        public string EntryAssemblyFileName { get; set; }
    }
}