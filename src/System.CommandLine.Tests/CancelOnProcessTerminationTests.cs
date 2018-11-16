// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.CommandLine.Tests
{
    public class CancelOnProcessTerminationTests
    {
        private const int SIGINT = 2;
        private const int SIGTERM = 15;

        [LinuxOnlyTheory]
        [InlineData(SIGINT)]  // Console.CancelKeyPress
        [InlineData(SIGTERM)] // AppDomain.CurrentDomain.ProcessExit
        public async Task CancelOnProcessTermination_cancels_on_process_termination(int signo)
        {
            const string ChildProcessWaiting = "Waiting for the command to be cancelled";
            const string ChildProcessCancelling = "Gracefully handling cancellation";
            const int ExpectedExitCode = 42;

            Func<string[], Task<int>> childProgram = (string[] args) =>
                new CommandLineBuilder()
                    .AddCommand(new Command("the-command",
                    handler: CommandHandler.Create<CancellationToken>(async ct =>
                    {
                        const int FailTimeoutMilliseconds = 10000;

                        try
                        {
                            Console.WriteLine(ChildProcessWaiting);
                            await Task.Delay(FailTimeoutMilliseconds, ct);
                        }
                        catch (OperationCanceledException)
                        {
                            // Sleep a little to ensure we are really blocking the ProcessExit event.
                            Thread.Sleep(500);
                            Console.WriteLine(ChildProcessCancelling);

                            return ExpectedExitCode;
                        }

                        Assert.True(false, "The operation was not cancelled.");
                        return 1;
                    })))
                  .CancelOnProcessTermination()
                  .Build()
                  .InvokeAsync("the-command");

            using (RemoteExecution program = RemoteExecutor.Execute(childProgram, psi: new ProcessStartInfo { RedirectStandardOutput = true }))
            {
                System.Diagnostics.Process process = program.Process;

                string childState = await process.StandardOutput.ReadLineAsync();
                Assert.Equal(ChildProcessWaiting, childState);

                Assert.Equal(0, kill(process.Id, signo));

                childState = await process.StandardOutput.ReadLineAsync();
                Assert.Equal(ChildProcessCancelling, childState);

                process.WaitForExit();
                Assert.Equal(ExpectedExitCode, process.ExitCode);
            }
        }

        [LinuxOnlyTheory]
        [InlineData(SIGINT)]  // Console.CancelKeyPress
        [InlineData(SIGTERM)] // AppDomain.CurrentDomain.ProcessExit
        public async Task CancelOnProcessTermination_non_cancellable_invocation_doesnt_block_termination_and_returns_non_zero_exit_code(int signo)
        {
            const string ChildProcessSleeping = "Sleeping";

            Func<string[], Task<int>> childProgram = (string[] args) =>
                new CommandLineBuilder()
                    .AddCommand(new Command("the-command",
                    handler: CommandHandler.Create(() =>
                    {
                        const int FailTimeoutMilliseconds = 10000;

                        Console.WriteLine(ChildProcessSleeping);
                        Thread.Sleep(FailTimeoutMilliseconds);

                        Assert.True(false, "The operation was not cancelled.");
                        return 1;
                    })))
                  .CancelOnProcessTermination()
                  .Build()
                  .InvokeAsync("the-command");

            using (RemoteExecution program = RemoteExecutor.Execute(childProgram, psi: new ProcessStartInfo { RedirectStandardOutput = true }))
            {
                System.Diagnostics.Process process = program.Process;

                string childState = await process.StandardOutput.ReadLineAsync();
                Assert.Equal(ChildProcessSleeping, childState);

                Assert.Equal(0, kill(process.Id, signo));

                process.WaitForExit();
                Assert.NotEqual(0, process.ExitCode);
            }
        }

        [DllImport("libc", SetLastError = true)]
        private static extern int kill(int pid, int sig);
    }
}