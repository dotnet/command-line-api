// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.CommandLine.Tests.Utility;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Invocation
{
    public class CancelOnProcessTerminationTests
    {
        private const int SIGINT = 2;
        private const int SIGTERM = 15;

        [LinuxOnlyTheory]
        [InlineData(SIGINT, Skip = "https://github.com/dotnet/command-line-api/issues/1206")]  // Console.CancelKeyPress
        [InlineData(SIGTERM)] // AppDomain.CurrentDomain.ProcessExit
        public async Task CancelOnProcessTermination_cancels_on_process_termination(int signo)
        {
            const string ChildProcessWaiting = "Waiting for the command to be cancelled";
            const int CancelledExitCode = 42;

            Func<string[], Task<int>> childProgram = (string[] args) =>
                new CommandLineBuilder()
                    .AddCommand(new Command("the-command")
                    {
                        Handler = CommandHandler.Create<CancellationToken>(async ct =>
                        {
                            try
                            {
                                Console.WriteLine(ChildProcessWaiting);
                                await Task.Delay(int.MaxValue, ct);
                            }
                            catch (OperationCanceledException)
                            {
                                // For Process.Exit handling the event must remain blocked as long as the
                                // command is executed.
                                // We are currently blocking that event because CancellationTokenSource.Cancel
                                // is called from the event handler.
                                // We'll do an async Yield now. This means the Cancel call will return
                                // and we're no longer actively blocking the event.
                                // The event handler is responsible to continue blocking until the command
                                // has finished executing. If it doesn't we won't get the CancelledExitCode.
                                await Task.Yield();

                                return CancelledExitCode;
                            }

                            return 1;
                        })
                    })
                    .CancelOnProcessTermination()
                    .Build()
                    .InvokeAsync("the-command");

            using (RemoteExecution program = RemoteExecutor.Execute(childProgram, psi: new ProcessStartInfo { RedirectStandardOutput = true }))
            {
                System.Diagnostics.Process process = program.Process;

                // Wait for the child to be in the command handler.
                string childState = await process.StandardOutput.ReadLineAsync();
                childState.Should().Be(ChildProcessWaiting);

                // Request termination
                kill(process.Id, signo).Should().Be(0);

                // Verify the process terminates timely
                bool processExited = process.WaitForExit(10000);
                if (!processExited)
                {
                    process.Kill();
                    process.WaitForExit();
                }
                processExited.Should().Be(true);

                // Verify the process exit code
                process.ExitCode.Should().Be(CancelledExitCode);
            }
        }

        [DllImport("libc", SetLastError = true)]
        private static extern int kill(int pid, int sig);
    }
}
