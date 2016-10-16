using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using DotNet.Globals.Core.Logging;
using DotNet.Globals.Core.PackageResolvers;
using DotNet.Globals.Core.Utils;

using Newtonsoft.Json;

namespace DotNet.Globals.Core
{
    public class PackageOperations
    {
        public DirectoryInfo PackagesFolder { get; }
        public DirectoryInfo BinFolder { get; }

        private PackageOperations(string applicationFolder)
        {
            string packagesFolder = Path.Combine(applicationFolder, "packages");
            string binFolder = Path.Combine(applicationFolder, "bin");

            if (!Directory.Exists(packagesFolder))
                Directory.CreateDirectory(packagesFolder);

            if (!Directory.Exists(binFolder))
                Directory.CreateDirectory(binFolder);

            this.PackagesFolder = new DirectoryInfo(packagesFolder);
            this.BinFolder = new DirectoryInfo(binFolder);
        }

        private static void SetGlobalLogger(ILogger logger) => Reporter.Logger = logger;

        public static PackageOperations GetInstance(ILogger logger, string applicationFolder = ".")
        {
            SetGlobalLogger(logger);
            return new PackageOperations(applicationFolder);
        }

        public void Install(string source, Options options)
        {
            Package package = new Package();

            if (source.StartsWith("http") || source.StartsWith("git"))
            {
                Reporter.Logger.LogInformation("Resolving package from git repo");
                GitPackageResolver gitPackageResolver = new GitPackageResolver(this.PackagesFolder, source, options);
                package = gitPackageResolver.Resolve();
            }
            else if (!source.Contains("/") && !source.Contains(@"\") && !source.StartsWith("."))
            {
                string[] packageParts = source.Split('@');
                source = packageParts[0];

                if (packageParts.Length > 1)
                    options.Version = packageParts[1];

                Reporter.Logger.LogInformation("Resolving package from NuGet");
                NugetPackageResolver nugetPackageResolver = new NugetPackageResolver(this.PackagesFolder, source, options);
                package = nugetPackageResolver.Resolve();
            }
            else
            {
                Reporter.Logger.LogInformation("Resolving package from project folder");
                FolderPackageResolver folderPackageResolver = new FolderPackageResolver(this.PackagesFolder, Path.GetFullPath(source), options);
                package = folderPackageResolver.Resolve();
            }

            Reporter.Logger.LogSuccess("Package has been resolved");
            Reporter.Logger.LogInformation("Creating executable");

            string executablePath = GetExecutablePath(package);
            File.WriteAllText(executablePath, $"dotnet {Path.Combine(package.Folder.FullName, package.EntryAssemblyFileName)}");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                ProcessRunner.RunProcess("chmod", "+x", executablePath);

            this.CleanUpTemp();
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

            Reporter.Logger.LogInformation($"Removing {package}");
            Package p = JsonConvert.DeserializeObject<Package>(File.ReadAllText(Path.Combine(packageFolder.FullName, "globals.json")));
            string executablePath = GetExecutablePath(p);
            FileSystemOperations.RemoveFolder(packageFolder);
            File.Delete(executablePath);
        }

        public void Update(string package)
        {
            var packageFolder = this.PackagesFolder.GetDirectories().FirstOrDefault(d => d.Name == package);
            if (packageFolder == null)
                throw new Exception("Packge does not exist");

            Package p = JsonConvert.DeserializeObject<Package>(File.ReadAllText(Path.Combine(packageFolder.FullName, "globals.json")));
            p.Options.Version = null;

            this.Uninstall(package);
            this.Install(p.Source, p.Options);
        }

        private string GetExecutablePath(Package package)
        {
            string executableName = Path.GetFileNameWithoutExtension(package.EntryAssemblyFileName);
            executableName += RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".cmd" : "";
            return Path.Combine(this.BinFolder.FullName, executableName);
        }

        private void CleanUpTemp()
        {
            try
            {
                DirectoryInfo tempFolder = new DirectoryInfo(Path.GetTempPath());
                var applicationFolders = tempFolder.GetDirectories().Where(d => d.Name.StartsWith("dotnet-globals"));
                foreach (var folder in applicationFolders)
                    FileSystemOperations.RemoveFolder(folder);
            }
            catch (System.Exception)
            {
            }
        }
    }
}