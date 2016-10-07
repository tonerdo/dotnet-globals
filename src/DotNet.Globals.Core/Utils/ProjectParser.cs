namespace DotNet.Globals.Core.Utils
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.IO;

    internal class ProjectParser
    {
        public static string GetPackageName(FileInfo projectJson)
        {
            JObject projectObject = JObject.Parse(File.ReadAllText(projectJson.FullName));
            JToken buildOptions;
            bool hasBuildOptions = projectObject.TryGetValue("buildOptions", out buildOptions);
            if (!hasBuildOptions)
                throw new Exception("Missing buildOptions property in project.json");
            else
            {
                bool hasEntryPoint = buildOptions.Value<bool>("emitEntryPoint");
                if (!hasEntryPoint)
                    throw new Exception("Entry point not found in package");

                string outputName = buildOptions.Value<string>("outputName");
                return outputName ?? projectJson.Directory.Name;
            }
        }
    }
}