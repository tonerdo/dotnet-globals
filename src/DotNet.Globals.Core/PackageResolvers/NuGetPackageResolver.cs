using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

using DotNet.Globals.Core.Utils;

using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace DotNet.Globals.Core.PackageResolvers
{
    internal class NugetPackageResolver : PackageResolver
    {
        public NugetPackageResolver(DirectoryInfo packagesFolder, string source, Options options) : base(packagesFolder, source, options) { }

        private async Task<PackageIdentity> GetPackageIdentity()
        {
            if (string.IsNullOrEmpty(this.Options.NuGetPackageSource))
                throw new Exception("No NuGet package source specified");

            string nugetSource = this.Options.NuGetPackageSource;
            string framework = ".NETCoreApp,Version=v1.0";
            Logger logger = new Logger();

            List<Lazy<INuGetResourceProvider>> providers = new List<Lazy<INuGetResourceProvider>>();
            providers.AddRange(Repository.Provider.GetCoreV3());

            PackageSource packageSource = new PackageSource(nugetSource);
            SourceRepository sourceRepository = new SourceRepository(packageSource, providers);
            PackageMetadataResource packageMetadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>();

            IEnumerable<IPackageSearchMetadata> searchMetadata = await packageMetadataResource.GetMetadataAsync(this.Source, this.Options.PreRelease, false, logger, CancellationToken.None);
            if (searchMetadata.Count() == 0)
                throw new Exception($"Unable to resolve {this.Source}");

            var identities = searchMetadata.Select(p => p.Identity);
            PackageIdentity identity = identities.FirstOrDefault(i => i.Version.ToFullString() == this.Options.Version);
            if (identity == null)
                identity = identities.LastOrDefault();

            string version = identity.Version.ToFullString();

            bool supportsNetCoreApp = searchMetadata
                .FirstOrDefault(p => p.Identity.Version.ToFullString() == version)
                .DependencySets.Select(d => d.TargetFramework.ToString()).Contains(framework);

            if (!supportsNetCoreApp)
                throw new Exception($"Unable to resolve '{this.Source} (>= {version})' for '{framework}'");

            return identity;
        }

        private DirectoryInfo GetNuGetPackageAssembliesFolder(PackageIdentity packageIdentity)
        {
            string json = "{ \"dependencies\": { \"" + packageIdentity.Id + "\": \"" + packageIdentity.Version.ToFullString() + "\" }, \"frameworks\": { \"netcoreapp1.0\": {} } }";
            DirectoryInfo tempFolder = new DirectoryInfo(Path.GetTempPath())
                .CreateSubdirectory("dotnet-globals-" + Guid.NewGuid().ToString());

            string projectPath = Path.Combine(tempFolder.FullName, "project.json");
            File.AppendAllText(projectPath, json);

            bool restore = ProcessRunner.RunProcess("dotnet", "restore", projectPath, $"-s {this.Options.NuGetPackageSource}");

            string nugetAssemblies = Environment.GetEnvironmentVariable("HOME") ?? Environment.GetEnvironmentVariable("USERPROFILE");
            nugetAssemblies = Path.Combine(nugetAssemblies, ".nuget", "packages", packageIdentity.Id, packageIdentity.Version.ToFullString(),
                "lib", "netcoreapp1.0");

            return new DirectoryInfo(nugetAssemblies);
        }

        private FileInfo GetEntryAssemblyFile(DirectoryInfo nugetPackageAssemblies)
        {
            FileInfo[] assemblyFiles = nugetPackageAssemblies.GetFiles("*.dll", SearchOption.TopDirectoryOnly);
            AssemblyLoadContext assemblyLoadContext = AssemblyLoadContext.Default;
            foreach (var assemblyFile in assemblyFiles)
            {
                if (assemblyLoadContext.LoadFromAssemblyPath(assemblyFile.FullName).EntryPoint != null)
                    return assemblyFile;
            }

            throw new Exception("Entry point not found in package");
        }

        protected override void Acquire()
        {
            Task<PackageIdentity> task = this.GetPackageIdentity();
            task.Wait();

            PackageIdentity packageIdentity = task.Result;
            DirectoryInfo nugetPackageAssembliesFolder = GetNuGetPackageAssembliesFolder(packageIdentity);
            FileInfo entryAssemblyFile = GetEntryAssemblyFile(nugetPackageAssembliesFolder);

            string packageName = Path.GetFileNameWithoutExtension(entryAssemblyFile.FullName);
            this.PackageFolder = this.PackagesFolder.GetDirectories().FirstOrDefault(d => d.Name == packageName);
            if (this.PackageFolder != null)
                throw new Exception("A package with the same name already exists");

            this.PackageFolder = this.PackagesFolder.CreateSubdirectory(packageName);
            FileInfo[] frameworkFiles = nugetPackageAssembliesFolder.GetFiles();
            foreach (var frameworkFile in frameworkFiles)
            {
                frameworkFile.CopyTo(Path.Combine(this.PackageFolder.FullName, frameworkFile.Name));
            }
        }

        private class Logger : ILogger
        {
            public void LogDebug(string data) => Console.WriteLine($"DEBUG: {data}");
            public void LogVerbose(string data) => Console.WriteLine($"VERBOSE: {data}");
            public void LogInformation(string data) => Console.WriteLine($"INFORMATION: {data}");
            public void LogMinimal(string data) => Console.WriteLine($"MINIMAL: {data}");
            public void LogWarning(string data) => Console.WriteLine($"WARNING: {data}");
            public void LogError(string data) => Console.WriteLine($"ERROR: {data}");
            public void LogSummary(string data) => Console.WriteLine($"SUMMARY: {data}");
            public void LogInformationSummary(string data) => Console.WriteLine($"INFORMATION SUMMARY: {data}");
            public void LogErrorSummary(string data) => Console.WriteLine($"ERROR SUMMARY: {data}");
        }
    }
}