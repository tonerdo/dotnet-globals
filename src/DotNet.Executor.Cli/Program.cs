﻿using System;
using Microsoft.Extensions.CommandLineUtils;
using DotNet.Executor.Core;

namespace DotNet.Executor.Cli
{
    class Program
    {
        public static int Main(string[] args)
        {
            PackageOperations packageOperations = PackageOperations.GetInstance();
            var app = new CommandLineApplication();
            app.Name = "dotnet exec";
            app.FullName = ".NET Executor";
            app.Description = "Install and use command line tools built on .NET Core";
            app.HelpOption("-h|--help");
            app.VersionOption("-v|--version", "1.0.0");

            app.Command("install", c =>
            {
                c.Description = "Installs a package";
                c.HelpOption("-h|--help");

                var packageArgument = c.Argument("<PACKAGE>",
                    "The package to install. Can be a NuGet package, a git repo or folder path of a .NET Core project");

                var sourceOption = c.Option("-s|--source", "Specifies a NuGet package source", CommandOptionType.MultipleValue);

                c.OnExecute(() =>
                {
                    if (string.IsNullOrEmpty(packageArgument.Value))
                    {
                        Console.Error.WriteLine("<PACKAGE> argument is required. Use -h|--help to see help");
                        return 1;
                    }

                    try
                    {
                        packageOperations.Install(packageArgument.Value);
                        return 0;
                    }
                    catch (System.Exception ex)
                    {
                        Console.Error.WriteLine(ex.Message);
                        return 1;
                    }
                });
            });

            app.Command("uninstall", c =>
            {
                c.Description = "Uninstall a package";
                c.HelpOption("-h|--help");

                var packageArgument = c.Argument("<PACKAGE>", "The package to uninstall");

                c.OnExecute(() =>
                {
                    if (string.IsNullOrEmpty(packageArgument.Value))
                    {
                        Console.Error.WriteLine("<PACKAGE> argument is required. Use -h|--help to see help");
                        return 1;
                    }

                    try
                    {
                        packageOperations.Uninstall(packageArgument.Value);
                        Console.WriteLine("Package removed successfully");
                        return 0;
                    }
                    catch (System.Exception ex)
                    {
                        Console.Error.WriteLine(ex.Message);
                        return 1;
                    }
                });
            });

            app.Command("update", c =>
            {
                c.Description = "Updates a package";
                c.HelpOption("-h|--help");

                var packageArgument = c.Argument("<PACKAGE>", "The package to update");

                c.OnExecute(() =>
                {
                    if (string.IsNullOrEmpty(packageArgument.Value))
                    {
                        Console.Error.WriteLine("<PACKAGE> argument is required. Use -h|--help to see help");
                        return 1;
                    }

                    return 0;
                });
            });

            app.Command("list", c =>
            {
                c.Description = "Lists all installed packages";

                c.OnExecute(() =>
                {
                    string[] packages = packageOperations.List();
                    Console.WriteLine(packageOperations.PackagesFolder.FullName);

                    if (packages.Length == 0)
                        Console.WriteLine("No packages installed");
                    else
                        foreach (string package in packages)
                            Console.WriteLine("|-- {0}", package);

                    return 0;
                });
            });

            if (args.Length == 0)
                app.ShowHelp();

            try
            {
                return app.Execute(args);
            }
            catch (System.Exception)
            {
                return 1;
            }
        }
    }
}
