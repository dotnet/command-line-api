// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Help;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Invocation
{
    public class InvocationPipelineTests
    {
        [Fact]
        public async Task InvokeAsync_chooses_the_appropriate_command()
        {
            var firstWasCalled = false;
            var secondWasCalled = false;

            var first = new Command("first");
            first.SetAction((_) => firstWasCalled = true);

            var second = new Command("second");
            second.SetAction((_) => secondWasCalled = true);

            var config = new CommandLineConfiguration(new RootCommand
                         {
                             first,
                             second
                         });

            await config.InvokeAsync("first");

            firstWasCalled.Should().BeTrue();
            secondWasCalled.Should().BeFalse();
        }

        [Fact]
        public void Invoke_chooses_the_appropriate_command()
        {
            var firstWasCalled = false;
            var secondWasCalled = false;

            var first = new Command("first");
            first.SetAction((_) => firstWasCalled = true);

            var second = new Command("second");
            second.SetAction((_) => secondWasCalled = true);

            var config = new CommandLineConfiguration(new RootCommand
                         {
                             first,
                             second
                         });

            config.Invoke("first");

            firstWasCalled.Should().BeTrue();
            secondWasCalled.Should().BeFalse();
        }

        [Fact]
        public void When_default_exception_handler_is_disabled_InvokeAsync_does_not_swallow_action_exceptions()
        {
            var command = new Command("the-command");
            command.SetAction((_, __) => Task.FromException(new Exception("oops!")));

            CommandLineConfiguration config = new (command)
            {
                EnableDefaultExceptionHandler = false
            };

            Func<Task> invoke = async () => await config.InvokeAsync("the-command");

            invoke.Should()
                  .Throw<Exception>()
                  .Which
                  .Message
                  .Should()
                  .Be("oops!");
        }

        [Fact]
        public void When_default_exception_handler_is_disabled_command_handler_exceptions_are_propagated()
        {
            var command = new Command("the-command");
            command.SetAction((_, __) => Task.FromException(new Exception("oops!")));

            CommandLineConfiguration config = new (command)
            {
                EnableDefaultExceptionHandler = false
            };

            Func<int> invoke = () => command.Parse("the-command", config).Invoke();

            invoke.Should()
                .Throw<Exception>()
                .Which
                .Message
                .Should()
                .Be("oops!");
        }

        [Fact]
        public void When_no_help_builder_is_specified_it_uses_default_implementation()
        {
            HelpOption helpOption = new();

            helpOption.Action.Should().NotBeNull();
            (helpOption.Action as HelpAction).Builder.Should().NotBeNull();
        }
    }
}
