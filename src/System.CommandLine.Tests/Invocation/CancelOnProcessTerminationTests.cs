// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
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

        private static readonly CliOption<bool> InfiniteDelayOption = new("--infiniteDelay");

        public enum Signals
        {
            SIGINT = 2, // Console.CancelKeyPress
            SIGTERM = 15 // AppDomain.CurrentDomain.ProcessExit
        }

        [Fact]
        public async Task CancellableHandler_is_cancelled_on_process_termination()
        {
            // The feature is supported on Windows, but it's simply harder to send SIGINT to test it properly.
            // Same for macOS, where RemoteExecutor does not support getting application arguments.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                await StartKillAndVerify(new[] { "--infiniteDelay", "false" }, Signals.SIGINT, GracefulExitCode);
            }
        }

        [Fact]
        public async Task NonCancellableHandler_is_interrupted_on_process_termination()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                await StartKillAndVerify(new[] { "--infiniteDelay", "true" }, Signals.SIGTERM, SIGTERM_EXIT_CODE);
            }
        }

        private static Task<int> Program(string[] args)
        {
            CliRootCommand command = new ()
            {
                InfiniteDelayOption
            };
            command.Action = new CustomCliAction();

            return new CliConfiguration(command)
            {
                ProcessTerminationTimeout = TimeSpan.FromSeconds(2)
            }.InvokeAsync(args);
        }

        private sealed class CustomCliAction : CliAction
        {
            public override int Invoke(ParseResult context) => throw new NotImplementedException();

            public async override Task<int> InvokeAsync(ParseResult context, CancellationToken cancellationToken = default)
            {
                Console.WriteLine(ChildProcessWaiting);

                bool infiniteDelay = context.GetValue(InfiniteDelayOption);

                try
                {
                    // Passing CancellationToken.None here is an example of bad pattern
                    // and reason why we need a timeout on termination processing.
                    CancellationToken token = infiniteDelay ? CancellationToken.None : cancellationToken;
                    await Task.Delay(Timeout.InfiniteTimeSpan, token);

                    return 0;
                }
                catch (OperationCanceledException)
                {
                    return GracefulExitCode;
                }
            }
        }

        private async Task StartKillAndVerify(string[] args, Signals signal, int expectedExitCode)
        {
            using RemoteExecution program = RemoteExecutor.Execute(
                Program,
                args,
                new ProcessStartInfo { RedirectStandardOutput = true });

            Process process = program.Process;

            // Wait for the child to be in the command handler.
            string childState = await process.StandardOutput.ReadLineAsync();
            childState.Should().Be(ChildProcessWaiting);

            // Request termination
            kill(process.Id, (int)signal).Should().Be(0);

            // Verify the process terminates timely
            if (!process.WaitForExit((int)TimeSpan.FromSeconds(10).TotalMilliseconds))
            {
                process.Kill();
            }

            // Verify the process exit code
            process.ExitCode.Should().Be(expectedExitCode);
            
            [DllImport("libc", SetLastError = true)]
            static extern int kill(int pid, int sig);
        }
    }
}