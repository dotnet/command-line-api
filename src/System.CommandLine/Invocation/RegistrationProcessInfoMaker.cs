using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace System.CommandLine.Invocation
{
    public static class RegistrationProcessInfoMaker
    {
        public static ProcessStartInfo GetProcessStartInfoForRegistration(string executablePath)
        {
            if (!Path.IsPathRooted(executablePath))
            {
                throw new ArgumentException(executablePath + "is not a rooted path.");
            }

            var commandName = Path.GetFileNameWithoutExtension(executablePath);

            var processInfo = new ProcessStartInfo {
                FileName = "dotnet-suggest",
                Arguments = $"register --command-path \"{executablePath}\" --suggestion-command \"{commandName} [suggest]\"",
                UseShellExecute = false
            };

            return processInfo;
        }
    }
}
