// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal static class Process
    {
        public static async Task<int> CompleteAsync(
            this Diagnostics.Process process,
            CancellationToken? cancellationToken = null) =>
            await Task.Run(() =>
            {
                process.WaitForExit();

                return Task.FromResult(process.ExitCode);
            }, cancellationToken ?? CancellationToken.None);

        public static Diagnostics.Process StartProcess(
            string command,
            string args,
            string? workingDir = null,
            Action<string>? stdOut = null,
            Action<string>? stdErr = null,
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

            if (!string.IsNullOrWhiteSpace(workingDir))
            {
                process.StartInfo.WorkingDirectory = workingDir;
            }

            if (environmentVariables.Length > 0)
            {
                for (var i = 0; i < environmentVariables.Length; i++)
                {
                    var (key, value) = environmentVariables[i];
                    process.StartInfo.Environment.Add(key, value);
                }
            }

            if (stdOut is not null)
            {
                process.OutputDataReceived += (sender, eventArgs) =>
                {
                    if (eventArgs.Data is not null)
                    {
                        stdOut(eventArgs.Data);
                    }
                };
            }

            if (stdErr is not null)
            {
                process.ErrorDataReceived += (sender, eventArgs) =>
                {
                    if (eventArgs.Data is not null)
                    {
                        stdErr(eventArgs.Data);
                    }
                };
            }

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return process;
        }
    }
}
