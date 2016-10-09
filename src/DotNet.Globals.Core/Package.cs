namespace DotNet.Globals.Core
{
    using System.IO;
    using Newtonsoft.Json;

    internal class Package
    {
        public string Source { get; set; }
        public string EntryAssemblyFileName { get; set; }
        public Options Options { get; set; }
        [JsonIgnore]
        public DirectoryInfo Folder { get; set; }
    }
}