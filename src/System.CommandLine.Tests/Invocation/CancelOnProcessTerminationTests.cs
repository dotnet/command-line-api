// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.CommandLine.Tests.Utility;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;
using Process = System.Diagnostics.Process;

namespace System.CommandLine.Tests.Invocation
{
    public class CancelOnProcessTerminationTests
    {
        private const int SIGINT = 2;
        private const int SIGTERM = 15;

        [LinuxOnlyTheory]
        [InlineData(SIGINT/*, Skip = "https://github.com/dotnet/command-line-api/issues/1206"*/)]  // Console.CancelKeyPress
        [InlineData(SIGTERM)] // AppDomain.CurrentDomain.ProcessExit
        public async Task CancelOnProcessTermination_provides_CancellationToken_that_signals_termination_when_no_timeout_is_specified(int signo)
        {
            const string ChildProcessWaiting = "Waiting for the command to be cancelled";
            const int CancelledExitCode = 42;

            Func<string[], Task<int>> childProgram = (string[] args) =>
            {
                var command = new Command("the-command");
            
                command.SetHandler(async context =>
                {
                    var cancellationToken = context.GetCancellationToken();

                    try
                    {
                        context.Console.WriteLine(ChildProcessWaiting);
                        await Task.Delay(int.MaxValue, cancellationToken);
                        context.ExitCode = 1;
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

                        context.ExitCode = CancelledExitCode;
                    }

                });

                return new CommandLineBuilder(new RootCommand
                       {
                           command
                       })
                       .CancelOnProcessTermination()
                       .Build()
                       .InvokeAsync("the-command");
            };

            using RemoteExecution program = RemoteExecutor.Execute(childProgram, psi: new ProcessStartInfo { RedirectStandardOutput = true });
            
            Process process = program.Process;

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

        [LinuxOnlyTheory]
        [InlineData(SIGINT)]
        [InlineData(SIGTERM)]
        public async Task CancelOnProcessTermination_provides_CancellationToken_that_signals_termination_when_null_timeout_is_specified(int signo)
        {
            const string ChildProcessWaiting = "Waiting for the command to be cancelled";
            const int CancelledExitCode = 42;

            Func<string[], Task<int>> childProgram = (string[] args) =>
            {
                var command = new Command("the-command");

                command.SetHandler(async context =>
                {
                    var cancellationToken = context.GetCancellationToken();

                    try
                    {
                        context.Console.WriteLine(ChildProcessWaiting);
                        await Task.Delay(int.MaxValue, cancellationToken);
                        context.ExitCode = 1;
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

                        // Exit code gets set here - but then execution continues and is let run till code voluntarily returns
                        //  hence exit code gets overwritten below
                        context.ExitCode = 123;
                    }

                    // This is an example of bad pattern and reason why we need a timeout on termination processing
                    await Task.Delay(TimeSpan.FromMilliseconds(200));

                    context.ExitCode = CancelledExitCode;
                });

                return new CommandLineBuilder(new RootCommand
                       {
                           command
                       })
                       // Unfortunately we cannot use test parameter here - RemoteExecutor currently doesn't capture the closure
                       .CancelOnProcessTermination(null)
                       .Build()
                       .InvokeAsync("the-command");
            };

            using RemoteExecution program = RemoteExecutor.Execute(childProgram, psi: new ProcessStartInfo { RedirectStandardOutput = true });

            Process process = program.Process;

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

        [LinuxOnlyTheory]
        [InlineData(SIGINT)]
        [InlineData(SIGTERM)]
        public async Task CancelOnProcessTermination_provides_CancellationToken_that_signals_termination_and_execution_is_terminated_at_the_specified_timeout(int signo)
        {
            const string ChildProcessWaiting = "Waiting for the command to be cancelled";
            const int CancelledExitCode = 42;
            const int ForceTerminationCode = 130;

            Func<string[], Task<int>> childProgram = (string[] args) =>
            {
                var command = new Command("the-command");

                command.SetHandler(async context =>
                {
                    var cancellationToken = context.GetCancellationToken();

                    try
                    {
                        context.Console.WriteLine(ChildProcessWaiting);
                        await Task.Delay(int.MaxValue, cancellationToken);
                        context.ExitCode = 1;
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

                        context.ExitCode = CancelledExitCode;

                        // This is an example of bad pattern and reason why we need a timeout on termination processing
                        await Task.Delay(TimeSpan.FromMilliseconds(1000));

                        // Execution should newer get here as termination processing has a timeout of 100ms
                        Environment.Exit(123);
                    }

                    // This is an example of bad pattern and reason why we need a timeout on termination processing
                    await Task.Delay(TimeSpan.FromMilliseconds(1000));

                    // Execution should newer get here as termination processing has a timeout of 100ms
                    Environment.Exit(123);
                });

                return new CommandLineBuilder(new RootCommand
                       {
                           command
                       })
                        // Unfortunately we cannot use test parameter here - RemoteExecutor currently doesn't capture the closure
                       .CancelOnProcessTermination(TimeSpan.FromMilliseconds(100))
                       .Build()
                       .InvokeAsync("the-command");
            };

            using RemoteExecution program = RemoteExecutor.Execute(childProgram, psi: new ProcessStartInfo { RedirectStandardOutput = true });

            Process process = program.Process;

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
            process.ExitCode.Should().Be(ForceTerminationCode);
        }

        [DllImport("libc", SetLastError = true)]
        private static extern int kill(int pid, int sig);
    }
}
