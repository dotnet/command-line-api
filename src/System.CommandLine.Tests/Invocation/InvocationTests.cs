// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Help;
using System.CommandLine.Parsing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Invocation
{
    public class InvocationTests
    {
        [Fact]
        public async Task Command_InvokeAsync_enables_help_by_default()
        {
            var command = new CliCommand("the-command")
            {
                new HelpOption()
            };
            var theHelpText = "the help text";
            command.Description = theHelpText;

            StringWriter output = new();
            CliConfiguration config = new(command)
            {
                Output = output
            };

            await command.Parse("-h", config).InvokeAsync();

            output.ToString()
                  .Should()
                  .Contain(theHelpText);
        }

        [Fact]
        public void Command_Invoke_enables_help_by_default()
        {
            var command = new CliCommand("the-command")
            {
                new HelpOption()
            };
            var theHelpText = "the help text";
            command.Description = theHelpText;

            StringWriter output = new ();
            CliConfiguration config = new(command)
            {
                Output = output
            };

            command.Parse("-h", config).Invoke();

            output.ToString()
                  .Should()
                  .Contain(theHelpText);
        }

        [Fact]
        public async Task RootCommand_InvokeAsync_returns_0_when_handler_is_successful()
        {
            var wasCalled = false;
            var rootCommand = new CliRootCommand();

            rootCommand.SetAction((_) => wasCalled = true);

            var result = await rootCommand.Parse("").InvokeAsync();

            wasCalled.Should().BeTrue();
            result.Should().Be(0);
        }

        [Fact]
        public void RootCommand_Invoke_returns_0_when_handler_is_successful()
        {
            var wasCalled = false;
            var rootCommand = new CliRootCommand();

            rootCommand.SetAction((_) => wasCalled = true);

            int result = rootCommand.Parse("").Invoke();

            wasCalled.Should().BeTrue();
            result.Should().Be(0);
        }

        [Fact]
        public async Task RootCommand_InvokeAsync_returns_1_when_handler_throws()
        {
            var wasCalled = false;
            var rootCommand = new CliRootCommand();

            rootCommand.SetAction((_, __) =>
            {
                wasCalled = true;

                return Task.FromException(new Exception("oops!"));
            });

            var resultCode = await rootCommand.Parse("").InvokeAsync();

            wasCalled.Should().BeTrue();
            resultCode.Should().Be(1);
        }

        [Fact]
        public void RootCommand_Invoke_returns_1_when_handler_throws()
        {
            var wasCalled = false;
            var rootCommand = new CliRootCommand();

            rootCommand.SetAction((_, __) =>
            {
                wasCalled = true;
                throw new Exception("oops!");

                // Help the compiler pick a CommandHandler.Create overload.
#pragma warning disable CS0162 // Unreachable code detected
                return Task.FromResult(0);
#pragma warning restore CS0162
            });

            var resultCode = rootCommand.Parse("").Invoke();

            wasCalled.Should().BeTrue();
            resultCode.Should().Be(1);
        }

        [Fact]
        public void Custom_RootCommand_Action_can_set_custom_result_code_via_Invoke()
        {
            var rootCommand = new CliRootCommand
            {
                Action = new CustomExitCodeAction()
            };

            rootCommand.Parse("").Invoke().Should().Be(123);
        }

        [Fact]
        public async Task Custom_RootCommand_Action_can_set_custom_result_code_via_InvokeAsync()
        {
            var rootCommand = new CliRootCommand
            {
                Action = new CustomExitCodeAction()
            };

            (await rootCommand.Parse("").InvokeAsync()).Should().Be(456);
        }

        [Fact]
        public void Anonymous_RootCommand_Task_returning_Action_can_set_custom_result_code_via_Invoke()
        {
            var rootCommand = new CliRootCommand();

            rootCommand.SetAction((_, _) => Task.FromResult(123));

            rootCommand.Parse("").Invoke().Should().Be(123);
        }

        [Fact]
        public async Task Anonymous_RootCommand_Task_returning_Action_can_set_custom_result_code_via_InvokeAsync()
        {
            var rootCommand = new CliRootCommand();

            rootCommand.SetAction((_, _) => Task.FromResult(123));

            (await rootCommand.Parse("").InvokeAsync()).Should().Be(123);
        }
        [Fact]
        public void Anonymous_RootCommand_int_returning_Action_can_set_custom_result_code_via_Invoke()
        {
            var rootCommand = new CliRootCommand();

            rootCommand.SetAction(_ => 123);

            rootCommand.Parse("").Invoke().Should().Be(123);
        }

        [Fact]
        public async Task Anonymous_RootCommand_int_returning_Action_can_set_custom_result_code_via_InvokeAsync()
        {
            var rootCommand = new CliRootCommand();

            rootCommand.SetAction(_ => 123);

            (await rootCommand.Parse("").InvokeAsync()).Should().Be(123);
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Nonexclusive_actions_handle_exceptions_and_return_an_error_return_code(bool invokeAsync)
        {
            var nonexclusiveAction = new NonexclusiveTestAction
            {
                ThrowOnInvoke = true
            };

            var command = new CliRootCommand
            {
                new CliOption<bool>("-x")
                {
                    Action = nonexclusiveAction
                }
            };

            int returnCode;

            var parseResult = CliParser.Parse(command, "-x");

            if (invokeAsync)
            {
                returnCode = await parseResult.InvokeAsync();
            }
            else
            {
                returnCode = parseResult.Invoke();
            }

            returnCode.Should().Be(1);
        }

        [Fact]
        public async Task Command_InvokeAsync_with_cancelation_token_invokes_command_handler()
        {
            using CancellationTokenSource cts = new();
            var command = new CliCommand("test");
            command.SetAction((_, cancellationToken) =>
            {
                cancellationToken.Should().Be(cts.Token);
                return Task.CompletedTask;
            });

            cts.Cancel();
            await command.Parse("test").InvokeAsync(cancellationToken: cts.Token);
        }

        private class CustomExitCodeAction : CliAction
        {
            public override int Invoke(ParseResult context)
                => 123;

            public override Task<int> InvokeAsync(ParseResult context, CancellationToken cancellationToken = default)
                => Task.FromResult(456);
        }

        private class NonexclusiveTestAction : CliAction
        {
            public NonexclusiveTestAction()
            {
                Exclusive = false;
            }

            public bool ThrowOnInvoke { get; set; }

            public bool HasBeenInvoked { get; private set; }

            public override int Invoke(ParseResult parseResult)
            {
                HasBeenInvoked = true;
                if (ThrowOnInvoke)
                {
                    throw new Exception("oops!");
                }

                return 0;
            }

            public override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
            {
                HasBeenInvoked = true;
                if (ThrowOnInvoke)
                {
                    throw new Exception("oops!");
                }

                return Task.FromResult(0);
            }
        }
    }
}
