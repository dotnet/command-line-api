// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using FluentAssertions;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.CommandLine.Tests
{
    public class InvocationPipelineTests
    {
        private readonly TestConsole _console = new TestConsole();

        [Fact]
        public async Task General_invocation_middleware_can_be_specified_in_the_CommandLineBuilder()
        {
            var wasCalled = false;

            var parser =
                new CommandLineBuilder()
                    .AddCommand(new Command("command"))
                    .UseMiddleware(_ => wasCalled = true)
                    .Build();

            await parser.InvokeAsync("command", _console);

            wasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task InvokeAsync_chooses_the_appropriate_command()
        {
            var firstWasCalled = false;
            var secondWasCalled = false;

            var first = new Command("first");
            first.Handler = CommandHandler.Create(() => firstWasCalled = true);

            var second = new Command("second");
            second.Handler = CommandHandler.Create(() => secondWasCalled = true);

            var parser = new CommandLineBuilder()
                         .AddCommand(first)
                         .AddCommand(second)
                         .Build();

            await parser.InvokeAsync("first", _console);

            firstWasCalled.Should().BeTrue();
            secondWasCalled.Should().BeFalse();
        }

        [Fact]
        public async Task Common_InvokeAsync_chooses_the_appropriate_command_with_args()
        {
            var firstWasCalled = false;
            var secondWasCalled = false;

            var builder = new CommandLineBuilder()
                         .AddCommand("first", "",
                                     cmd => cmd.OnExecute(() => firstWasCalled = true))
                         .AddCommand("second", "",
                                     cmd => cmd.OnExecute(() => secondWasCalled = true));
            string[] args = { "first" };
            await builder.InvokeAsync(args , _console);

            firstWasCalled.Should().BeTrue();
            secondWasCalled.Should().BeFalse();
        }


        [Fact]
        public async Task Common_InvokeAsync_chooses_the_appropriate_command()
        {
            var firstWasCalled = false;
            var secondWasCalled = false;

            var builder = new CommandLineBuilder()
                         .AddCommand("first", "",
                                     cmd => cmd.OnExecute(() => firstWasCalled = true))
                         .AddCommand("second", "",
                                     cmd => cmd.OnExecute(() => secondWasCalled = true));

            await builder.InvokeAsync("first", _console);

            firstWasCalled.Should().BeTrue();
            secondWasCalled.Should().BeFalse();
        }

        [Fact]
        public void When_middleware_throws_then_InvokeAsync_does_not_handle_the_exception()
        {
            var parser = new CommandLineBuilder()
                         .AddCommand(new Command("the-command"))
                         .UseMiddleware(_ => throw new Exception("oops!"))
                         .Build();

            Func<Task> invoke = async () => await parser.InvokeAsync("the-command", _console);

            invoke.Should()
                  .Throw<Exception>()
                  .WithMessage("oops!");
        }

        [Fact]
        public void When_command_handler_throws_then_InvokeAsync_does_not_handle_the_exception()
        {
            var command = new Command("the-command");
            command.Handler = CommandHandler.Create(() =>
                {
                    throw new Exception("oops!");
                // Help the compiler pick a CommandHandler.Create overload.
#pragma warning disable CS0162 // Unreachable code detected
                return 0;
#pragma warning restore CS0162
                });

            var parser = new CommandLineBuilder()
                         .AddCommand(command)
                         .Build();

            Func<Task> invoke = async () => await parser.InvokeAsync("the-command", _console);

            invoke.Should()
                  .Throw<Exception>()
                  .Which
                  .Message
                  .Should()
                  .Be("oops!");
        }

        [Fact]
        public async Task ParseResult_can_be_replaced_by_middleware()
        {
            var wasCalled = false;
            var command = new Command("the-command");
            var implicitInnerCommand = new Command("implicit-inner-command");
            command.AddCommand(implicitInnerCommand);
            implicitInnerCommand.Handler = CommandHandler.Create((ParseResult result) =>
            {
                wasCalled = true;
                result.Errors.Should().BeEmpty();
            });

            var parser = new CommandLineBuilder()
                         .UseMiddleware(async (context, next) =>
                         {
                             var tokens = context.ParseResult.Tokens.Concat(new[] { "implicit-inner-command" }).ToArray();
                             context.ParseResult = context.Parser.Parse(tokens);
                             await next(context);
                         })
                         .AddCommand(command)
                         .Build();

            await parser.InvokeAsync("the-command", new TestConsole());

            wasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Invocation_can_be_short_circuited_by_middleware_by_not_calling_next()
        {
            var middlewareWasCalled = false;
            var handlerWasCalled = false;

            var command = new Command("the-command");
            command.Handler = CommandHandler.Create((ParseResult result) =>
            {
                handlerWasCalled = true;
                result.Errors.Should().BeEmpty();
            });

            var parser = new CommandLineBuilder()
                         .UseMiddleware(async (context, next) =>
                         {
                             middlewareWasCalled = true;
                             await Task.Yield();
                         })
                         .AddCommand(command)
                         .Build();

            await parser.InvokeAsync("the-command", new TestConsole());

            middlewareWasCalled.Should().BeTrue();
            handlerWasCalled.Should().BeFalse();
        }
    }
}
