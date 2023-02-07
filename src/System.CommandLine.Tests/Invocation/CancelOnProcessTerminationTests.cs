// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.CommandLine.Parsing;
using System.CommandLine.Tests.Utility;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Process = System.Diagnostics.Process;

namespace System.CommandLine.Tests.Invocation
{
    public class CancelOnProcessTerminationTests
    {
        private const string ChildProcessWaiting = "Waiting for the command to be cancelled";
        private const int SIGINT_EXIT_CODE = 130;
        private const int SIGTERM_EXIT_CODE = 143;
        private const int GracefulExitCode = 42;

        public enum Signals
        {
            SIGINT = 2, // Console.CancelKeyPress
            SIGTERM = 15 // AppDomain.CurrentDomain.ProcessExit
        }

        [LinuxOnlyTheory]
        [InlineData(Signals.SIGINT)]  
        [InlineData(Signals.SIGTERM)]
        public Task CancellableHandler_is_cancelled_on_process_termination_when_no_timeout_is_specified(Signals signal)
            => StartKillAndVerify(args =>
                    Program(args, infiniteDelay: false, processTerminationTimeout: null),
                    signal,
                    GracefulExitCode);

        [LinuxOnlyTheory]
        [InlineData(Signals.SIGINT)]
        [InlineData(Signals.SIGTERM)]
        public Task CancellableHandler_is_cancelled_on_process_termination_when_explicit_timeout_is_specified(Signals signo)
            => StartKillAndVerify(args => 
                    Program(args, infiniteDelay: false, processTerminationTimeout: TimeSpan.FromSeconds(1)),
                    signo,
                    GracefulExitCode);
        
        [LinuxOnlyTheory]
        [InlineData(Signals.SIGINT, SIGINT_EXIT_CODE)]
        [InlineData(Signals.SIGTERM, SIGTERM_EXIT_CODE)]
        public Task NonCancellableHandler_is_interrupted_on_process_termination_when_no_timeout_is_specified(Signals signo, int expectedExitCode)
            => StartKillAndVerify(args =>
                    Program(args, infiniteDelay: true, processTerminationTimeout: null),
                    signo,
                    expectedExitCode);
        
        [LinuxOnlyTheory]
        [InlineData(Signals.SIGINT, SIGINT_EXIT_CODE)]
        [InlineData(Signals.SIGTERM, SIGTERM_EXIT_CODE)]
        public Task NonCancellableHandler_is_interrupted_on_process_termination_when_explicit_timeout_is_specified(Signals signo, int expectedExitCode)
            => StartKillAndVerify(args =>
                    Program(args, infiniteDelay: true, processTerminationTimeout: TimeSpan.FromMilliseconds(100)),
                    signo,
                    expectedExitCode);

        private static async Task<int> Program(string[] args, bool infiniteDelay, TimeSpan? processTerminationTimeout)
        {
            var command = new RootCommand();

            command.SetHandler(async (context, cancellationToken) =>
            {
                context.Console.WriteLine(ChildProcessWaiting);

                try
                {
                    // Passing CancellationToken.None here is an example of bad pattern
                    // and reason why we need a timeout on termination processing.
                    CancellationToken token = infiniteDelay ? CancellationToken.None : cancellationToken;
                    await Task.Delay(Timeout.InfiniteTimeSpan, token);
                }
                catch (OperationCanceledException)
                {
                    context.ExitCode = GracefulExitCode;
                }
            });

            int result = await new CommandLineBuilder(command)
                .CancelOnProcessTermination(processTerminationTimeout)
                .Build()
                .InvokeAsync("the-command");
            
            return result;
        }

        private static async Task StartKillAndVerify(Func<string[], Task<int>> childProgram, Signals signo, int expectedExitCode)
        {
            using RemoteExecution program = RemoteExecutor.Execute(childProgram, psi: new ProcessStartInfo { RedirectStandardOutput = true, RedirectStandardInput = true });

            Process process = program.Process;

            // Wait for the child to be in the command handler.
            string childState = await process.StandardOutput.ReadLineAsync();
            childState.Should().Be(ChildProcessWaiting);

            // Request termination
            kill(process.Id, (int)signo).Should().Be(0);

            // Verify the process terminates timely
            try
            {
                using CancellationTokenSource cts = new (TimeSpan.FromSeconds(10));
                await process.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                process.Kill();
                
                throw;
            }

            // Verify the process exit code
            process.ExitCode.Should().Be(expectedExitCode);
            
            [DllImport("libc", SetLastError = true)]
            static extern int kill(int pid, int sig);
        }
    }
}