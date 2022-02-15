// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Reflection;

namespace System.CommandLine.Suggest
{
    internal static class SuggestionShellScriptHandler
    {
        public static void Handle(IConsole console, ShellType shellType)
        {
            switch (shellType)
            {
                case ShellType.Bash:
                    PrintToConsoleFrom(console, "dotnet-suggest-shim.bash");
                    break;
                case ShellType.PowerShell:
                    PrintToConsoleFrom(console, "dotnet-suggest-shim.ps1");
                    break;
                case ShellType.Zsh:
                    PrintToConsoleFrom(console, "dotnet-suggest-shim.zsh");
                    break;
                default:
                    throw new SuggestionShellScriptException($"Shell '{shellType}' is not supported.");
            }
        }

        private static void PrintToConsoleFrom(IConsole console, string scriptName)
        {
            var assemblyLocation = Assembly.GetAssembly(typeof(SuggestionShellScriptHandler)).Location;
            var directory = Path.GetDirectoryName(assemblyLocation);
            console.Out.Write(File.ReadAllText(Path.Combine(directory, scriptName)));
        }
    }
}
