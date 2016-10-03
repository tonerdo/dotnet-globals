using System.IO;
using System.Linq;
using System.Reflection;

using DotNet.Executor.Core.PackageResolvers;

namespace DotNet.Executor.Core
{
    public class PackageOperations
    {
        public DirectoryInfo PackagesFolder { get; }

        private PackageOperations(string packagesFolder)
        {
            Assembly asm = Assembly.GetEntryAssembly();
            packagesFolder = Path.Combine(Path.GetDirectoryName(asm.Location), packagesFolder);

            if (!Directory.Exists(packagesFolder))
                Directory.CreateDirectory(packagesFolder);

            this.PackagesFolder = new DirectoryInfo(packagesFolder);
        }

        public static PackageOperations GetInstance(string packagesFolder = "packages")
        {
            return new PackageOperations(packagesFolder);
        }

        public void Install(string package)
        {
            if (package.StartsWith("http"))
            {
                // TODO: Git repo
            }
            else if (!package.Contains("/") && package.Length > 2)
            {
                // TODO: Nuget package
            }
            else
            {
                FolderPackageResolver folderPackageResolver = new FolderPackageResolver(this.PackagesFolder, Path.GetFullPath(package));
                folderPackageResolver.Resolve();
            }
        }

        public string[] List()
        {
            return this.PackagesFolder.EnumerateDirectories().Select(d => d.Name).ToArray();
        }
    }
}