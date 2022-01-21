// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Tests.Utility;

public static class Process
{
    public static int RunToCompletion(
        string command,
        string args,
        Action<string> stdOut = null,
        Action<string> stdErr = null,
        string workingDirectory = null,
        params (string key, string value)[] environmentVariables)
    {
        args ??= "";

        var process = new Diagnostics.Process
        {
            StartInfo =
            {
                Arguments = args,
                FileName = command,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false
            }
        };

        if (!string.IsNullOrWhiteSpace(workingDirectory))
        {
            process.StartInfo.WorkingDirectory = workingDirectory;
        }

        if (environmentVariables.Length > 0)
        {
            for (var i = 0; i < environmentVariables.Length; i++)
            {
                var (key, value) = environmentVariables[i];
                process.StartInfo.Environment.Add(key, value);
            }
        }

        if (stdOut != null)
        {
            process.OutputDataReceived += (sender, eventArgs) =>
            {
                if (eventArgs.Data != null)
                {
                    stdOut(eventArgs.Data);
                }
            };
        }

        if (stdErr != null)
        {
            process.ErrorDataReceived += (sender, eventArgs) =>
            {
                if (eventArgs.Data != null)
                {
                    stdErr(eventArgs.Data);
                }
            };
        }

        process.Start();

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        process.WaitForExit();

        return process.ExitCode;
    }
}