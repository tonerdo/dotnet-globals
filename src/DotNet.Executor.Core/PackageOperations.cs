using System;
using System.IO;
using System.Linq;
using System.Reflection;

using DotNet.Executor.Core.PackageResolvers;
using DotNet.Executor.Core.Utils;

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
            if (package.StartsWith("http") || package.StartsWith("git@"))
            {
                GitPackageResolver gitPackageResolver = new GitPackageResolver(this.PackagesFolder, package);
                gitPackageResolver.Resolve();
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

        public void Uninstall(string package)
        {
            var packageFolder = this.PackagesFolder.GetDirectories().FirstOrDefault(d => d.Name == package);
            if (packageFolder == null)
                throw new Exception("Packge does not exist");

            PackageRemover.RemoveFolder(packageFolder);
        }
    }
}