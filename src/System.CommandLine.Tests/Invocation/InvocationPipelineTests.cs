// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Invocation
{
    public class InvocationPipelineTests
    {
        private readonly TestConsole _console = new();

        [Fact]
        public async Task InvokeAsync_chooses_the_appropriate_command()
        {
            var firstWasCalled = false;
            var secondWasCalled = false;

            var first = new Command("first");
            first.SetAction((_) => firstWasCalled = true);

            var second = new Command("second");
            second.SetAction((_) => secondWasCalled = true);

            var config = new CommandLineBuilder(new RootCommand
                         {
                             first,
                             second
                         })
                         .Build();

            await config.InvokeAsync("first", _console);

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

            var config = new CommandLineBuilder(new RootCommand
                         {
                             first,
                             second
                         })
                .Build();

            config.Invoke("first", _console);

            firstWasCalled.Should().BeTrue();
            secondWasCalled.Should().BeFalse();
        }

        [Fact]
        public void When_command_handler_throws_then_InvokeAsync_does_not_handle_the_exception()
        {
            var command = new Command("the-command");
            command.SetAction((_, __) =>
            {
                throw new Exception("oops!");
                // Help the compiler pick a CommandHandler.Create overload.
#pragma warning disable CS0162 // Unreachable code detected
                return Task.FromResult(0);
#pragma warning restore CS0162
            });

            var config = new CommandLineBuilder(new RootCommand
                         {
                             command
                         })
                         .Build();

            Func<Task> invoke = async () => await config.InvokeAsync("the-command", _console);

            invoke.Should()
                  .Throw<Exception>()
                  .Which
                  .Message
                  .Should()
                  .Be("oops!");
        }

        [Fact]
        public void When_command_handler_throws_then_Invoke_does_not_handle_the_exception()
        {
            var command = new Command("the-command");
            command.SetAction((_, __) =>
            {
                throw new Exception("oops!");
                // Help the compiler pick a CommandHandler.Create overload.
#pragma warning disable CS0162 // Unreachable code detected
                return Task.FromResult(0);
#pragma warning restore CS0162
            });

            var config = new CommandLineBuilder(new RootCommand
                         {
                             command
                         })
                .Build();

            Func<int> invoke = () => config.Invoke("the-command", _console);

            invoke.Should()
                .Throw<Exception>()
                .Which
                .Message
                .Should()
                .Be("oops!");
        }

        [Fact]
        public async Task When_no_help_builder_is_specified_it_uses_default_implementation()
        {
            bool handlerWasCalled = false;

            var command = new Command("help-command");
            command.SetAction((context, cancellationToken) =>
            {
                handlerWasCalled = true;
                context.HelpBuilder.Should().NotBeNull();
                return Task.FromResult(0);
            });

            var config = new CommandLineBuilder(new RootCommand
                         {
                             command
                         })
                         .Build();

            await config.InvokeAsync("help-command", new TestConsole());

            handlerWasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task When_help_builder_factory_is_specified_it_is_used_to_create_the_help_builder()
        {
            bool handlerWasCalled = false;
            bool factoryWasCalled = false;

            HelpBuilder createdHelpBuilder = null;

            var command = new Command("help-command");
            command.SetAction((context, cancellationToken) =>
            {
                handlerWasCalled = true;
                context.HelpBuilder.Should().Be(createdHelpBuilder);
                createdHelpBuilder.Should().NotBeNull();
                return Task.FromResult(0);
            });

            var config = new CommandLineBuilder(new RootCommand
                         {
                             command
                         })
                         .UseHelpBuilder(context =>
                         {
                             factoryWasCalled = true;
                             return createdHelpBuilder = new HelpBuilder();
                         })
                         .Build();

            await config.InvokeAsync("help-command", new TestConsole());

            handlerWasCalled.Should().BeTrue();
            factoryWasCalled.Should().BeTrue();
        }
    }
}
