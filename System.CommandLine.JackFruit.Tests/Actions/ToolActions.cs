using System;
using System.IO;
using System.Threading.Tasks;

namespace System.CommandLine.JackFruit
{
    internal static class ToolActions
    {
        public static string Captured;

        public static async Task<int> InstallAsync(string packageId, bool global, DirectoryInfo toolPath, string version, FileInfo configFile, string addSource,
            string framework, StandardVerbosity verbosity)
        {
            Captured =
            $@"Tool/Install(
        Package Id: {packageId}
        Global: {global}
        Tool Path: {toolPath}
        Version: {version}
        ConfigFile: {configFile}
        Add Source: {addSource}
        Framework: {framework}
        Verbosity: {verbosity.ToString()}
    )";
            return await Task.FromResult(3);
        }

        public static async Task<int> ListAsync(bool global, DirectoryInfo toolPath)
        {
            Captured =
           $@"Tool/List(
        Global: {global}
        Tool Path: {toolPath}
    )";
            return await Task.FromResult(0);
        }

        public static async Task<int> UninstallAsync(string packageId, bool global, DirectoryInfo toolPath)
        {
            Captured =
           $@"Tool/Uninstall(
        Package Id: {packageId}
        Global: {global}
        Tool Path: {toolPath}
    )";
            return await Task.FromResult(0);
        }


        public static async Task<int> UpdateAsync(string packageId, bool global, DirectoryInfo toolPath, FileInfo configFile, string addSource, string framework,
            StandardVerbosity verbosity)
        {
            Captured =
           $@"Tool/Update(
        Package Id: {packageId}
        Global: {global}
        Tool Path: {toolPath}
        ConfigFile: {configFile}
        Add Source: {addSource}
        Framework: {framework}
        Verbosity: {verbosity.ToString()}
    )";
            return await Task.FromResult(0);
        }
    }
}
