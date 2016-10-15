using System;
using System.IO;
using System.Linq;

using DotNet.Globals.Core.Logging;
using Moq;
using Xunit;

namespace DotNet.Globals.Core.Tests
{
    public class PackageOperationTests : IDisposable
    {
        public DirectoryInfo PackagesFolder { get; set; }
        public DirectoryInfo ExecutablesFolder { get; set; }
        private Mock<ILogger> _mockLogger;

        public PackageOperationTests()
        {
            this.PackagesFolder = new DirectoryInfo(Path.GetTempPath())
                .CreateSubdirectory(Guid.NewGuid().ToString());
            this.ExecutablesFolder = new DirectoryInfo(Path.GetTempPath())
                .CreateSubdirectory(Guid.NewGuid().ToString());
            _mockLogger = new Mock<ILogger>();
        }

        [Fact]
        public void TestNuGetPackageInstall()
        {
            PackageOperations packageOperations = PackageOperations
                .GetInstance(this._mockLogger.Object, this.PackagesFolder.FullName, this.ExecutablesFolder.FullName);

            packageOperations.Install("DotNet.Cleaner.Tools", new Options() { NuGetPackageSource = "https://api.nuget.org/v3/index.json" });

            this._mockLogger.Verify(l => l.LogInformation("Resolving package from NuGet"));
            this._mockLogger.Verify(l => l.LogSuccess("Package has been resolved"));
            this._mockLogger.Verify(l => l.LogInformation("Creating executable"));
        }

        [Fact]
        public void TestNuGetPackageWrongTargetFramework()
        {
            PackageOperations packageOperations = PackageOperations
                .GetInstance(this._mockLogger.Object, this.PackagesFolder.FullName, this.ExecutablesFolder.FullName);

            Assert.Throws<AggregateException>(() =>
            {
                packageOperations.Install("DotNetEnv", new Options() { NuGetPackageSource = "https://api.nuget.org/v3/index.json" });
            });

            this._mockLogger.Verify(l => l.LogInformation("Resolving package from NuGet"));
        }

        [Fact]
        public void TestNonExistingNuGetPackage()
        {
            PackageOperations packageOperations = PackageOperations
                .GetInstance(this._mockLogger.Object, this.PackagesFolder.FullName, this.ExecutablesFolder.FullName);

            Assert.Throws<AggregateException>(() =>
            {
                packageOperations.Install("", new Options() { NuGetPackageSource = "https://api.nuget.org/v3/index.json" });
            });

            this._mockLogger.Verify(l => l.LogInformation("Resolving package from NuGet"));
        }

        [Fact]
        public void TestFolderPackageInstall()
        {
            PackageOperations packageOperations = PackageOperations
                .GetInstance(this._mockLogger.Object, this.PackagesFolder.FullName, this.ExecutablesFolder.FullName);

            packageOperations.Install("../../src/DotNet.Globals.Cli", new Options());

            this._mockLogger.Verify(l => l.LogInformation("Resolving package from project folder"));
            this._mockLogger.Verify(l => l.LogSuccess("Package has been resolved"));
            this._mockLogger.Verify(l => l.LogInformation("Creating executable"));
        }

        [Fact]
        public void TestFolderPackageInstallNoEntryPoint()
        {
            PackageOperations packageOperations = PackageOperations
                .GetInstance(this._mockLogger.Object, this.PackagesFolder.FullName, this.ExecutablesFolder.FullName);
            
            Exception ex = Assert.Throws<Exception>(() =>
            {
                packageOperations.Install(".", new Options());
            });

            Assert.Equal("Entry point not found in package", ex.Message);
            this._mockLogger.Verify(l => l.LogInformation("Resolving package from project folder"));
        }

        [Fact]
        public void TestGitPackageInstall()
        {
            PackageOperations packageOperations = PackageOperations
                .GetInstance(this._mockLogger.Object, this.PackagesFolder.FullName, this.ExecutablesFolder.FullName);

            packageOperations.Install("git@github.com:tsolarin/dotnet-globals.git", new Options() { Folder = "src/DotNet.Globals.Cli" });

            this._mockLogger.Verify(l => l.LogInformation("Resolving package from git repo"));
            this._mockLogger.Verify(l => l.LogSuccess("Package has been resolved"));
            this._mockLogger.Verify(l => l.LogInformation("Creating executable"));
        }

        [Fact]
        public void TestGitPackageInstallNoEntryPoint()
        {
            PackageOperations packageOperations = PackageOperations
                .GetInstance(this._mockLogger.Object, this.PackagesFolder.FullName, this.ExecutablesFolder.FullName);

            Exception ex = Assert.Throws<Exception>(() =>
            {
                packageOperations.Install("git@github.com:tsolarin/dotnet-globals.git", new Options() { Folder = "src/DotNet.Globals.Core" });
            });

            Assert.Equal("Entry point not found in package", ex.Message);
            this._mockLogger.Verify(l => l.LogInformation("Resolving package from git repo"));
        }

        [Fact]
        public void TestGitPackageInstallNoProjectJson()
        {
            PackageOperations packageOperations = PackageOperations
                .GetInstance(this._mockLogger.Object, this.PackagesFolder.FullName, this.ExecutablesFolder.FullName);

            Exception ex = Assert.Throws<Exception>(() =>
            {
                packageOperations.Install("git@github.com:tsolarin/dotnet-globals.git", new Options());
            });

            Assert.Equal("No project.json found in source folder", ex.Message);
            this._mockLogger.Verify(l => l.LogInformation("Resolving package from git repo"));
        }

        [Fact]
        public void TestListPackagesEmpty()
        {
            PackageOperations packageOperations = PackageOperations
                .GetInstance(this._mockLogger.Object, this.PackagesFolder.FullName, this.ExecutablesFolder.FullName);

            Assert.Equal(0, packageOperations.List().Length);
        }

        [Fact]
        public void TestListPackagesNonEmpty()
        {
            PackageOperations packageOperations = PackageOperations
                .GetInstance(this._mockLogger.Object, this.PackagesFolder.FullName, this.ExecutablesFolder.FullName);

            packageOperations.Install("DotNet.Cleaner.Tools", new Options() { NuGetPackageSource = "https://api.nuget.org/v3/index.json" });

            Assert.Equal(1, packageOperations.List().Length);
            Assert.Equal("DotNet.Cleaner.Tools", packageOperations.List()[0]);
        }

        [Fact]
        public void TestUninstallPackage()
        {
            PackageOperations packageOperations = PackageOperations
                .GetInstance(this._mockLogger.Object, this.PackagesFolder.FullName, this.ExecutablesFolder.FullName);

            packageOperations.Install("DotNet.Cleaner.Tools", new Options() { NuGetPackageSource = "https://api.nuget.org/v3/index.json" });
            packageOperations.Uninstall("DotNet.Cleaner.Tools");

            Assert.Equal(0, packageOperations.List().Length);
            this._mockLogger.Verify(l => l.LogInformation("Removing DotNet.Cleaner.Tools"));
        }

        [Fact]
        public void TestUninstallNonPackage()
        {
            PackageOperations packageOperations = PackageOperations
                .GetInstance(this._mockLogger.Object, this.PackagesFolder.FullName, this.ExecutablesFolder.FullName);

            Exception ex = Assert.Throws<Exception>(() =>
            {
                packageOperations.Uninstall("DotNet.Cleaner.Tools");
            });

            Assert.Equal("Packge does not exist", ex.Message);
        }

        [Fact]
        public void TestUpdateNonPackage()
        {
            PackageOperations packageOperations = PackageOperations
                .GetInstance(this._mockLogger.Object, this.PackagesFolder.FullName, this.ExecutablesFolder.FullName);

            Exception ex = Assert.Throws<Exception>(() =>
            {
                packageOperations.Update("DotNet.Cleaner.Tools");
            });

            Assert.Equal("Packge does not exist", ex.Message);
        }

        private void RemoveFolder(DirectoryInfo folder)
        {
            var files = folder.GetFiles();
            var directories = folder.GetDirectories();
            files.ToList().ForEach(f => f.Delete());
            directories.ToList().ForEach(d => RemoveFolder(d));
            folder.Delete();
        }

        public void Dispose()
        {
            this.RemoveFolder(this.PackagesFolder);
            this.RemoveFolder(this.ExecutablesFolder);
        }
    }
}
