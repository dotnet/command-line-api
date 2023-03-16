// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Invocation
{
    public class InvocationExtensionsTests
    {
        [Fact]
        public async Task Command_InvokeAsync_uses_default_pipeline_by_default()
        {
            var command = new Command("the-command");
            var theHelpText = "the help text";
            command.Description = theHelpText;

            var console = new TestConsole();

            await command.InvokeAsync("-h", console);

            console.Out
                   .ToString()
                   .Should()
                   .Contain(theHelpText);
        }

        [Fact]
        public void Command_Invoke_uses_default_pipeline_by_default()
        {
            var command = new Command("the-command");
            var theHelpText = "the help text";
            command.Description = theHelpText;

            var console = new TestConsole();

            command.Invoke("-h", console);

            console.Out
                   .ToString()
                   .Should()
                   .Contain(theHelpText);
        }

        [Fact]
        public async Task RootCommand_InvokeAsync_returns_0_when_handler_is_successful()
        {
            var wasCalled = false;
            var rootCommand = new RootCommand();

            rootCommand.SetAction((_) => wasCalled = true);

            var result = await rootCommand.InvokeAsync("");

            wasCalled.Should().BeTrue();
            result.Should().Be(0);
        }

        [Fact]
        public void RootCommand_Invoke_returns_0_when_handler_is_successful()
        {
            var wasCalled = false;
            var rootCommand = new RootCommand();

            rootCommand.SetAction((_) => wasCalled = true);

            int result = rootCommand.Invoke("");

            wasCalled.Should().BeTrue();
            result.Should().Be(0);
        }

        [Fact]
        public async Task RootCommand_InvokeAsync_returns_1_when_handler_throws()
        {
            var wasCalled = false;
            var rootCommand = new RootCommand();

            rootCommand.SetAction((_, __) =>
            {
                wasCalled = true;

                return Task.FromException(new Exception("oops!"));
            });

            var resultCode = await rootCommand.InvokeAsync("");

            wasCalled.Should().BeTrue();
            resultCode.Should().Be(1);
        }

        [Fact]
        public void RootCommand_Invoke_returns_1_when_handler_throws()
        {
            var wasCalled = false;
            var rootCommand = new RootCommand();

            rootCommand.SetAction((_, __) =>
            {
                wasCalled = true;
                throw new Exception("oops!");

                // Help the compiler pick a CommandHandler.Create overload.
#pragma warning disable CS0162 // Unreachable code detected
                return Task.FromResult(0);
#pragma warning restore CS0162
            });

            var resultCode = rootCommand.Invoke("");

            wasCalled.Should().BeTrue();
            resultCode.Should().Be(1);
        }

        [Fact]
        public async Task RootCommand_Action_can_set_custom_result_code()
        {
            var rootCommand = new RootCommand()
            {
                Action = new CustomExitCodeAction()
            };

            rootCommand.Invoke("").Should().Be(123);
            (await rootCommand.InvokeAsync("")).Should().Be(456);
        }

        internal sealed class CustomExitCodeAction : CliAction
        {
            public override int Invoke(InvocationContext context)
                => 123;

            public override Task<int> InvokeAsync(InvocationContext context, CancellationToken cancellationToken = default)
                => Task.FromResult(456);
        }

        [Fact]
        public async Task Command_InvokeAsync_with_cancelation_token_invokes_command_handler()
        {
            using CancellationTokenSource cts = new();
            var command = new Command("test");
            command.SetAction((InvocationContext context, CancellationToken cancellationToken) =>
            {
                cancellationToken.Should().Be(cts.Token);
                return Task.CompletedTask;
            });

            cts.Cancel();
            await command.InvokeAsync("test", cancellationToken: cts.Token);
        }
    }
}
