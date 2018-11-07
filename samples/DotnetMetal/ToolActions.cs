using System;
using System.IO;

namespace DotMetal
{
    public static class ToolActions 
    {
        public static void Install(bool global, DirectoryInfo toolPath, string version, FileInfo file, string addSource, 
            string framework, StandardVerbosity verbosity) 
            => Console.WriteLine(
$@"Tool/Install(
        Global: {global}
        Tool Path: {toolPath}
        Version: {version}
        File: {file}
        Add Source: {addSource}
        Framework: {framework}
        Verbosity: {verbosity}
    )");
        public static void List(bool global, DirectoryInfo toolPath)
            => Console.WriteLine(
$@"Tool/List(
        Global: {global}
        Tool Path: {toolPath}
    )");
        public static void Uninstall(bool global, DirectoryInfo toolPath)
            => Console.WriteLine(
$@"Tool/Uninstall(
        Global: {global}
        Tool Path: {toolPath}
    )");
        public static void Update(bool global, DirectoryInfo toolPath, FileInfo file, string addSource, string framework, 
            StandardVerbosity verbosity)
            => Console.WriteLine(
$@"Tool/Update(
        Global: {global}
        Tool Path: {toolPath}
        File: {file}
        Add Source: {addSource}
        Framework: {framework}
        Verbosity: {verbosity}
    )");
    }
}
