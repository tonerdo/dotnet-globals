using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using DotNet.Globals.Core.PackageResolvers;
using DotNet.Globals.Core.Utils;

namespace DotNet.Globals.Core
{
    public class PackageOperations
    {
        public DirectoryInfo PackagesFolder { get; }
        public DirectoryInfo BinFolder { get; }

        private PackageOperations(string packagesFolder, string binFolder)
        {
            Assembly asm = Assembly.GetEntryAssembly();
            packagesFolder = Path.Combine(Path.GetDirectoryName(asm.Location), packagesFolder);
            binFolder = Path.Combine(Path.GetDirectoryName(asm.Location), binFolder);

            if (!Directory.Exists(packagesFolder))
                Directory.CreateDirectory(packagesFolder);

            if (!Directory.Exists(binFolder))
                Directory.CreateDirectory(binFolder);

            this.PackagesFolder = new DirectoryInfo(packagesFolder);
            this.BinFolder = new DirectoryInfo(binFolder);
        }

        public static PackageOperations GetInstance(string packagesFolder = "packages", string binFolder = "bin")
        {
            return new PackageOperations(packagesFolder, binFolder);
        }

        public void Install(string source, Options options)
        {
            Package package = new Package();

            if (source.StartsWith("http") || source.StartsWith("git"))
            {
                GitPackageResolver gitPackageResolver = new GitPackageResolver(this.PackagesFolder, source, options);
                package = gitPackageResolver.Resolve();
            }
            else if (!source.Contains("/") && !source.Contains(@"\"))
            {
                string[] packageParts = source.Split('@');
                source = packageParts[0];

                if (packageParts.Length > 1)
                    options.Version = packageParts[1];

                NugetPackageResolver nugetPackageResolver = new NugetPackageResolver(this.PackagesFolder, source, options);
                package = nugetPackageResolver.Resolve();
            }
            else
            {
                FolderPackageResolver folderPackageResolver = new FolderPackageResolver(this.PackagesFolder, Path.GetFullPath(source), options);
                package = folderPackageResolver.Resolve();
            }

            string executablePath = GetExecutablePath(package);
            File.WriteAllText(executablePath, $"dotnet {Path.Combine(package.Folder.FullName, package.EntryAssemblyFileName)}");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                ProcessRunner.RunProcess("chmod", "+x", executablePath);
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

        private string GetExecutablePath(Package package)
        {
            string executableName = Path.GetFileNameWithoutExtension(package.EntryAssemblyFileName);
            executableName += RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".cmd" : "";
            return Path.Combine(this.BinFolder.FullName, executableName);
        }
    }
}