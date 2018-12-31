// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Reflection;

namespace System.CommandLine.Suggest
{
    public static class SuggestionShellScriptHandler
    {
        public static void Handle(IConsole console, string shellType)
        {
            if (shellType.Equals("bash", StringComparison.OrdinalIgnoreCase))
            {
                PrintToConsoleFrom(console, "dotnet-suggest-shim.bash");
            }
            else if (shellType.Equals("powershell", StringComparison.OrdinalIgnoreCase))
            {
                PrintToConsoleFrom(console, "dotnet-suggest-shim.ps1");
            }
            else
            {
                throw new SuggestionShellScriptException($"{shellType} shell is not supported.");
            }
        }

        private static void PrintToConsoleFrom(IConsole console, string scriptName)
        {
            var assemblyLocation = Assembly.GetAssembly(typeof(SuggestionShellScriptHandler)).Location;
            var directory = Path.GetDirectoryName(assemblyLocation);
            console.Out.Write(File.ReadAllText(Path.Combine(directory, scriptName)));
        }
    }

    public class SuggestionShellScriptException : Exception
    {
        public SuggestionShellScriptException()
        {
        }

        public SuggestionShellScriptException(string message) : base(message)
        {
        }

        public SuggestionShellScriptException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
