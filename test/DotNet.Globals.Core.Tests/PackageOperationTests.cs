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
        private DirectoryInfo _applicationsFolder { get; set; }
        private Mock<ILogger> _mockLogger;
        private string _workspaceRoot;

        public PackageOperationTests()
        {
            this._applicationsFolder = new DirectoryInfo(Path.GetTempPath())
                .CreateSubdirectory(Guid.NewGuid().ToString());
            this._mockLogger = new Mock<ILogger>();
            this._workspaceRoot = Environment.GetEnvironmentVariable("WORKSPACEROOT") ?? string.Empty;
        }

        [Fact]
        public void TestNuGetPackageInstall()
        {
            PackageOperations packageOperations = PackageOperations
                .GetInstance(this._mockLogger.Object, this._applicationsFolder.FullName);

            packageOperations.Install("DotNet.Cleaner.Tools", new Options() { NuGetPackageSource = "https://api.nuget.org/v3/index.json" });

            this._mockLogger.Verify(l => l.LogInformation("Resolving package from NuGet"));
            this._mockLogger.Verify(l => l.LogSuccess("Package has been resolved"));
            this._mockLogger.Verify(l => l.LogInformation("Creating executable"));
        }

        [Fact]
        public void TestNuGetPackageWrongTargetFramework()
        {
            PackageOperations packageOperations = PackageOperations
                .GetInstance(this._mockLogger.Object, this._applicationsFolder.FullName);

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
                .GetInstance(this._mockLogger.Object, this._applicationsFolder.FullName);

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
                .GetInstance(this._mockLogger.Object, this._applicationsFolder.FullName);

            packageOperations.Install(Path.Combine(this._workspaceRoot, "../../src/DotNet.Globals.Cli"), new Options());

            this._mockLogger.Verify(l => l.LogInformation("Resolving package from project folder"));
            this._mockLogger.Verify(l => l.LogSuccess("Package has been resolved"));
            this._mockLogger.Verify(l => l.LogInformation("Creating executable"));
        }

        [Fact]
        public void TestFolderPackageInstallNoEntryPoint()
        {
            PackageOperations packageOperations = PackageOperations
                .GetInstance(this._mockLogger.Object, this._applicationsFolder.FullName);
            
            Exception ex = Assert.Throws<Exception>(() =>
            {
                packageOperations.Install(Path.Combine(this._workspaceRoot, "."), new Options());
            });

            Assert.Equal("Entry point not found in package", ex.Message);
            this._mockLogger.Verify(l => l.LogInformation("Resolving package from project folder"));
        }

        [Fact]
        public void TestGitPackageInstall()
        {
            PackageOperations packageOperations = PackageOperations
                .GetInstance(this._mockLogger.Object, this._applicationsFolder.FullName);

            packageOperations.Install("https://github.com/tsolarin/dotnet-globals", new Options() { Folder = "src/DotNet.Globals.Cli" });

            this._mockLogger.Verify(l => l.LogInformation("Resolving package from git repo"));
            this._mockLogger.Verify(l => l.LogSuccess("Package has been resolved"));
            this._mockLogger.Verify(l => l.LogInformation("Creating executable"));
        }

        [Fact]
        public void TestGitPackageInstallNoEntryPoint()
        {
            PackageOperations packageOperations = PackageOperations
                .GetInstance(this._mockLogger.Object, this._applicationsFolder.FullName);

            Exception ex = Assert.Throws<Exception>(() =>
            {
                packageOperations.Install("https://github.com/tsolarin/dotnet-globals", new Options() { Folder = "src/DotNet.Globals.Core" });
            });

            Assert.Equal("Entry point not found in package", ex.Message);
            this._mockLogger.Verify(l => l.LogInformation("Resolving package from git repo"));
        }

        [Fact]
        public void TestGitPackageInstallNoProjectJson()
        {
            PackageOperations packageOperations = PackageOperations
                .GetInstance(this._mockLogger.Object, this._applicationsFolder.FullName);

            Exception ex = Assert.Throws<Exception>(() =>
            {
                packageOperations.Install("https://github.com/tsolarin/dotnet-globals", new Options());
            });

            Assert.Equal("No project.json found in source folder", ex.Message);
            this._mockLogger.Verify(l => l.LogInformation("Resolving package from git repo"));
        }

        [Fact]
        public void TestListPackagesEmpty()
        {
            PackageOperations packageOperations = PackageOperations
                .GetInstance(this._mockLogger.Object, this._applicationsFolder.FullName);

            Assert.Equal(0, packageOperations.List().Length);
        }

        [Fact]
        public void TestListPackagesNonEmpty()
        {
            PackageOperations packageOperations = PackageOperations
                .GetInstance(this._mockLogger.Object, this._applicationsFolder.FullName);

            packageOperations.Install("DotNet.Cleaner.Tools", new Options() { NuGetPackageSource = "https://api.nuget.org/v3/index.json" });

            Assert.Equal(1, packageOperations.List().Length);
            Assert.Equal("DotNet.Cleaner.Tools", packageOperations.List()[0]);
        }

        [Fact]
        public void TestUninstallPackage()
        {
            PackageOperations packageOperations = PackageOperations
                .GetInstance(this._mockLogger.Object, this._applicationsFolder.FullName);

            packageOperations.Install("DotNet.Cleaner.Tools", new Options() { NuGetPackageSource = "https://api.nuget.org/v3/index.json" });
            packageOperations.Uninstall("DotNet.Cleaner.Tools");

            Assert.Equal(0, packageOperations.List().Length);
            this._mockLogger.Verify(l => l.LogInformation("Removing DotNet.Cleaner.Tools"));
        }

        [Fact]
        public void TestUninstallNonPackage()
        {
            PackageOperations packageOperations = PackageOperations
                .GetInstance(this._mockLogger.Object, this._applicationsFolder.FullName);

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
                .GetInstance(this._mockLogger.Object, this._applicationsFolder.FullName);

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
            this.RemoveFolder(this._applicationsFolder);
        }
    }
}
