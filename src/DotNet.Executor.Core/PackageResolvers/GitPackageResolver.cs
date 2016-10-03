using System;
using System.IO;
using System.Linq;
using DotNet.Executor.Core.Utils;

namespace DotNet.Executor.Core.PackageResolvers
{
    internal class GitPackageResolver : FolderPackageResolver
    {
        public GitPackageResolver(DirectoryInfo packagesFolder, string source) : base(packagesFolder, source) { }

        protected override void Acquire()
        {
            var packageName = this.Source.Split('/').Last().Replace(".git", "");
            DirectoryInfo tempFolder = new DirectoryInfo(Path.GetTempPath())
                .CreateSubdirectory("dotnet-exec-" + Guid.NewGuid().ToString())
                .CreateSubdirectory(packageName);

            bool clone = ProcessRunner.RunProcess("git", "clone", this.Source, tempFolder.FullName);
            if (!clone)
                throw new Exception("Unable to clone repository");

            this.Source = tempFolder.FullName;
            base.Acquire();
        }
    }
}