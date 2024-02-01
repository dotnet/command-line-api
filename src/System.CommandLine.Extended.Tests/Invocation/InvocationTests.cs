// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
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

            rootCommand.SetAction(_ => wasCalled = true);

            int result = rootCommand.Parse("").Invoke();

            wasCalled.Should().BeTrue();
            result.Should().Be(0);
        }

        [Fact]
        public async Task RootCommand_InvokeAsync_returns_1_when_handler_throws()
        {
            var wasCalled = false;
            var rootCommand = new CliRootCommand();

            rootCommand.SetAction((_, _) =>
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

            rootCommand.SetAction((_, _) =>
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
            var rootCommand = new CliRootCommand();
            rootCommand.SetAction(_ => 123);

            rootCommand.Parse("").Invoke().Should().Be(123);
        }

        [Fact]
        public async Task Custom_RootCommand_Action_can_set_custom_result_code_via_InvokeAsync()
        {
            var rootCommand = new CliRootCommand();
            rootCommand.SetAction((_, _) => Task.FromResult(456));

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
        
        [Fact]
        public void Terminating_option_action_short_circuits_command_action()
        {
            bool optionActionWasCalled = false;
            SynchronousTestAction optionAction = new(_ => optionActionWasCalled = true, terminating: true);
            bool commandActionWasCalled = false;

            CliOption<bool> option = new("--test")
            {
                Action = optionAction
            };
            CliCommand command = new CliCommand("cmd")
            {
                option
            };
            command.SetAction(_ =>
            {
                commandActionWasCalled = true;
            });

            ParseResult parseResult = command.Parse("cmd --test");

            parseResult.Action.Should().NotBeNull();
            optionActionWasCalled.Should().BeFalse();
            commandActionWasCalled.Should().BeFalse();

            parseResult.Invoke().Should().Be(0);
            optionActionWasCalled.Should().BeTrue();
            commandActionWasCalled.Should().BeFalse();
        }

        [Fact]
        public void Nonterminating_option_action_does_not_short_circuit_command_action()
        {
            bool optionActionWasCalled = false;
            SynchronousTestAction optionAction = new(_ => optionActionWasCalled = true, terminating: false);
            bool commandActionWasCalled = false;

            CliOption<bool> option = new("--test")
            {
                Action = optionAction
            };
            CliCommand command = new CliCommand("cmd")
            {
                option
            };
            command.SetAction(_ => commandActionWasCalled = true);

            ParseResult parseResult = command.Parse("cmd --test");

            parseResult.Invoke().Should().Be(0);
            optionActionWasCalled.Should().BeTrue();
            commandActionWasCalled.Should().BeTrue();
        }

        [Fact]
        public void When_multiple_options_with_actions_are_present_then_only_the_last_one_is_invoked()
        {
            bool optionAction1WasCalled = false;
            bool optionAction2WasCalled = false;
            bool optionAction3WasCalled = false;

            SynchronousTestAction optionAction1 = new(_ => optionAction1WasCalled = true);
            SynchronousTestAction optionAction2 = new(_ => optionAction2WasCalled = true);
            SynchronousTestAction optionAction3 = new(_ => optionAction3WasCalled = true);

            CliCommand command = new CliCommand("cmd")
            {
                new CliOption<bool>("--1") { Action = optionAction1 },
                new CliOption<bool>("--2") { Action = optionAction2 },
                new CliOption<bool>("--3") { Action = optionAction3 }
            };

            ParseResult parseResult = command.Parse("cmd --1 true --3 false --2 true");

            using var _ = new AssertionScope();

            parseResult.Action.Should().Be(optionAction2);
            parseResult.Invoke().Should().Be(0);
            optionAction1WasCalled.Should().BeFalse();
            optionAction2WasCalled.Should().BeTrue();
            optionAction3WasCalled.Should().BeFalse();
        }

        [Fact]
        public void Directive_action_takes_precedence_over_option_action()
        {
            bool optionActionWasCalled = false;
            bool directiveActionWasCalled = false;

            SynchronousTestAction optionAction = new(_ => optionActionWasCalled = true);
            SynchronousTestAction directiveAction = new(_ => directiveActionWasCalled = true);

            var directive = new CliDirective("directive")
            {
                Action = directiveAction
            };

            CliRootCommand command = new("cmd")
            {
                new CliOption<bool>("-x") { Action = optionAction },
                directive
            };

            ParseResult parseResult = command.Parse("[directive] cmd -x", new CliConfiguration(command));

            using var _ = new AssertionScope();

            parseResult.Action.Should().Be(directiveAction);
            parseResult.Invoke().Should().Be(0);
            optionActionWasCalled.Should().BeFalse();
            directiveActionWasCalled.Should().BeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Nontermninating_option_actions_handle_exceptions_and_return_an_error_return_code(bool invokeAsync)
        {
            var nonexclusiveAction = new SynchronousTestAction(_ => throw new Exception("oops!"), terminating: false);

            var command = new CliRootCommand
            {
                new CliOption<bool>("-x")
                {
                    Action = nonexclusiveAction
                }
            };
            command.SetAction(_ => 0);

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
    }
}
