using System;
using System.IO;
using System.Threading.Tasks;

namespace JackFruit
{
    public static class ToolActions
    {
        public static async Task<int> InstallAsync(bool global, DirectoryInfo toolPath, string version, FileInfo configFile, string addSource,
            string framework, StandardVerbosity verbosity)
        {
            Console.WriteLine(
            $@"Tool/Install(
        Global: {global}
        Tool Path: {toolPath}
        Version: {version}
        ConfigFile: {configFile}
        Add Source: {addSource}
        Framework: {framework}
        Verbosity: {verbosity}
    )");
            return await Task.FromResult(0);
        }

        public static async Task<int> List(bool global, DirectoryInfo toolPath)
        {
            Console.WriteLine(
           $@"Tool/List(
        Global: {global}
        Tool Path: {toolPath}
    )");
            return await Task.FromResult(0);
        }

        public static async Task<int> Uninstall(bool global, DirectoryInfo toolPath)
        {
            Console.WriteLine(
           $@"Tool/Uninstall(
        Global: {global}
        Tool Path: {toolPath}
    )");
            return await Task.FromResult(0);
        }

        public static async Task<int> Update(bool global, DirectoryInfo toolPath, FileInfo configFile, string addSource, string framework,
            StandardVerbosity verbosity)
        {
            Console.WriteLine(
           $@"Tool/Update(
        Global: {global}
        Tool Path: {toolPath}
        ConfigFile: {configFile}
        Add Source: {addSource}
        Framework: {framework}
        Verbosity: {verbosity}
    )");
            return await Task.FromResult(0);
        }
    }
}
