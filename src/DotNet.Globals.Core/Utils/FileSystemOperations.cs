namespace DotNet.Globals.Core.Utils
{
    using System.IO;
    using System.Linq;

    internal class FileSystemOperations
    {
        public static void RemoveFolder(DirectoryInfo folder)
        {
            var files = folder.GetFiles();
            var directories = folder.GetDirectories();
            files.ToList().ForEach(f => f.Delete());
            directories.ToList().ForEach(d => RemoveFolder(d));
            folder.Delete();
        }
    }
}