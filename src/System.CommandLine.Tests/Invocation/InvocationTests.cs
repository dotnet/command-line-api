// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Help;
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
        public async Task Command_InvokeAsync_displays_help_when_HelpOption_is_present()
        {
            var command = new Command("the-command")
            {
                new HelpOption()
            };
            var theHelpText = "the help text";
            command.Description = theHelpText;

            StringWriter output = new();
          
            await command.Parse("-h").InvokeAsync(new() { Output = output }, CancellationToken.None);

            output.ToString()
                  .Should()
                  .Contain(theHelpText);
        }

        [Fact]
        public void Command_Invoke_displays_help_when_HelpOption_is_present()
        {
            var command = new Command("the-command")
            {
                new HelpOption()
            };
            var theHelpText = "the help text";
            command.Description = theHelpText;

            StringWriter output = new ();
         
            command.Parse("-h").Invoke(new() { Output = output });

            output.ToString()
                  .Should()
                  .Contain(theHelpText);
        }

        [Fact]
        public async Task RootCommand_InvokeAsync_returns_0_when_handler_is_successful()
        {
            var wasCalled = false;
            var rootCommand = new RootCommand();

            rootCommand.SetAction((_) => wasCalled = true);

            var result = await rootCommand.Parse("").InvokeAsync();

            wasCalled.Should().BeTrue();
            result.Should().Be(0);
        }

        [Fact]
        public void RootCommand_Invoke_returns_0_when_handler_is_successful()
        {
            var wasCalled = false;
            var rootCommand = new RootCommand();

            rootCommand.SetAction(_ => wasCalled = true);

            int result = rootCommand.Parse("").Invoke();

            wasCalled.Should().BeTrue();
            result.Should().Be(0);
        }

        [Fact]
        public async Task RootCommand_InvokeAsync_returns_1_when_handler_throws()
        {
            var wasCalled = false;
            var rootCommand = new RootCommand();

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
            var rootCommand = new RootCommand();

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
            var rootCommand = new RootCommand();
            rootCommand.SetAction(_ => 123);

            rootCommand.Parse("").Invoke().Should().Be(123);
        }

        [Fact]
        public async Task Custom_RootCommand_Action_can_set_custom_result_code_via_InvokeAsync()
        {
            var rootCommand = new RootCommand();
            rootCommand.SetAction((_, _) => Task.FromResult(456));

            (await rootCommand.Parse("").InvokeAsync()).Should().Be(456);
        }

        [Fact]
        public void Anonymous_RootCommand_Task_returning_Action_can_set_custom_result_code_via_Invoke()
        {
            var rootCommand = new RootCommand();

            rootCommand.SetAction((_, _) => Task.FromResult(123));

            rootCommand.Parse("").Invoke().Should().Be(123);
        }

        [Fact]
        public async Task Anonymous_RootCommand_Task_returning_Action_can_set_custom_result_code_via_InvokeAsync()
        {
            var rootCommand = new RootCommand();

            rootCommand.SetAction((_, _) => Task.FromResult(123));

            (await rootCommand.Parse("").InvokeAsync()).Should().Be(123);
        }

        [Fact]
        public void Anonymous_RootCommand_int_returning_Action_can_set_custom_result_code_via_Invoke()
        {
            var rootCommand = new RootCommand();

            rootCommand.SetAction(_ => 123);

            rootCommand.Parse("").Invoke().Should().Be(123);
        }

        [Fact]
        public async Task Anonymous_RootCommand_int_returning_Action_can_set_custom_result_code_via_InvokeAsync()
        {
            var rootCommand = new RootCommand();

            rootCommand.SetAction(_ => 123);

            (await rootCommand.Parse("").InvokeAsync()).Should().Be(123);
        }

        [Fact] // https://github.com/dotnet/command-line-api/issues/2562
        public void Anonymous_async_action_is_not_mapped_into_sync_void_with_fire_and_forget()
        {
            RootCommand rootCommand = new();
            using CancellationTokenSource cts = new();
            Task delay = Task.Delay(TimeSpan.FromHours(1), cts.Token);

            rootCommand.SetAction(async parseResult =>
            {
                await delay;
            });

            Task started = rootCommand.Parse("").InvokeAsync();

            // The action is supposed to wait for an hour, so it should not complete immediately.
            started.IsCompleted.Should().BeFalse();

            cts.Cancel();
        }

        [Fact]
        public void Terminating_option_action_short_circuits_command_action()
        {
            bool optionActionWasCalled = false;
            SynchronousTestAction optionAction = new(_ => optionActionWasCalled = true, terminating: true);
            bool commandActionWasCalled = false;

            Option<bool> option = new("--test")
            {
                Action = optionAction
            };
            Command command = new Command("cmd")
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

            Option<bool> option = new("--test")
            {
                Action = optionAction
            };
            Command command = new Command("cmd")
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

            Command command = new Command("cmd")
            {
                new Option<bool>("--1") { Action = optionAction1 },
                new Option<bool>("--2") { Action = optionAction2 },
                new Option<bool>("--3") { Action = optionAction3 }
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

            var directive = new Directive("directive")
            {
                Action = directiveAction
            };

            RootCommand command = new("cmd")
            {
                new Option<bool>("-x") { Action = optionAction },
                directive
            };

            ParseResult parseResult = command.Parse("[directive] cmd -x");

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

            var command = new RootCommand
            {
                new Option<bool>("-x")
                {
                    Action = nonexclusiveAction
                }
            };
            command.SetAction(_ => 0);

            int returnCode;

            var parseResult = CommandLineParser.Parse(command, "-x");

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
            var command = new Command("test");
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
