namespace DotNet.Executor.Core.Utils
{
    using System.IO;
    using System.Linq;

    internal class PackageRemover
    {
        public static void RemoveFolder(DirectoryInfo directory)
        {
            var files = directory.GetFiles();
            var directories = directory.GetDirectories();
            files.ToList().ForEach(f => f.Delete());
            directories.ToList().ForEach(d => RemoveFolder(d));
            directory.Delete();
        }
    }
}