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
        public async Task General_invocation_middleware_can_be_specified_in_the_CommandLineBuilder()
        {
            var wasCalled = false;

            var parser =
                new CommandLineBuilder(new RootCommand { new Command("command") })
                    .AddMiddleware(_ => wasCalled = true)
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
            first.SetHandler(() => firstWasCalled = true);

            var second = new Command("second");
            second.SetHandler(() => secondWasCalled = true);

            var parser = new CommandLineBuilder(new RootCommand
                         {
                             first,
                             second
                         })
                         .Build();

            await parser.InvokeAsync("first", _console);

            firstWasCalled.Should().BeTrue();
            secondWasCalled.Should().BeFalse();
        }

        [Fact]
        public void Invoke_chooses_the_appropriate_command()
        {
            var firstWasCalled = false;
            var secondWasCalled = false;

            var first = new Command("first");
            first.SetHandler(() => firstWasCalled = true);

            var second = new Command("second");
            second.SetHandler(() => secondWasCalled = true);

            var parser = new CommandLineBuilder(new RootCommand
                         {
                             first,
                             second
                         })
                .Build();

            parser.Invoke("first", _console);

            firstWasCalled.Should().BeTrue();
            secondWasCalled.Should().BeFalse();
        }

        [Fact]
        public void When_middleware_throws_then_InvokeAsync_does_not_handle_the_exception()
        {
            var parser = new CommandLineBuilder(new RootCommand
                         {
                             new Command("the-command")
                         })
                         .AddMiddleware(_ => throw new Exception("oops!"))
                         .Build();

            Func<Task> invoke = async () => await parser.InvokeAsync("the-command", _console);

            invoke.Should()
                  .Throw<Exception>()
                  .WithMessage("oops!");
        }

        [Fact]
        public void When_middleware_throws_then_Invoke_does_not_handle_the_exception()
        {
            var parser = new CommandLineBuilder(new RootCommand
                         {
                             new Command("the-command")
                         })
                         .AddMiddleware(_ => throw new Exception("oops!"))
                         .Build();

            Func<int> invoke = () => parser.Invoke("the-command", _console);

            invoke.Should()
                .Throw<Exception>()
                .WithMessage("oops!");
        }

        [Fact]
        public void When_command_handler_throws_then_InvokeAsync_does_not_handle_the_exception()
        {
            var command = new Command("the-command");
            command.SetHandler(() =>
                {
                    throw new Exception("oops!");
                    // Help the compiler pick a CommandHandler.Create overload.
#pragma warning disable CS0162 // Unreachable code detected
                    return Task.FromResult(0);
#pragma warning restore CS0162
                });

            var parser = new CommandLineBuilder(new RootCommand
                         {
                             command
                         })
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
        public void When_command_handler_throws_then_Invoke_does_not_handle_the_exception()
        {
            var command = new Command("the-command");
            command.SetHandler(() =>
            {
                throw new Exception("oops!");
                // Help the compiler pick a CommandHandler.Create overload.
#pragma warning disable CS0162 // Unreachable code detected
                return Task.FromResult(0);
#pragma warning restore CS0162
            });

            var parser = new CommandLineBuilder(new RootCommand
                         {
                             command
                         })
                .Build();

            Func<int> invoke = () => parser.Invoke("the-command", _console);

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
            implicitInnerCommand.SetHandler(context =>
            {
                wasCalled = true;
                context.ParseResult.Errors.Should().BeEmpty();
                return Task.FromResult(0);
            });

            var parser = new CommandLineBuilder(new RootCommand
                         {
                            command
                         })
                         .AddMiddleware(async (context, next) =>
                         {
                             var tokens = context.ParseResult
                                                 .Tokens
                                                 .Select(t => t.Value)
                                                 .Concat(new[] { "implicit-inner-command" })
                                                 .ToArray();

                             context.ParseResult = context.Parser.Parse(tokens);
                             await next(context);
                         })
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
            command.SetHandler(context =>
            {
                handlerWasCalled = true;
                context.ParseResult.Errors.Should().BeEmpty();
                return Task.FromResult(0);
            });

            var parser = new CommandLineBuilder(new RootCommand
                         {
                             command
                         })
                         .AddMiddleware(async (_, _) =>
                         {
                             middlewareWasCalled = true;
                             await Task.Yield();
                         })
                         .Build();

            await parser.InvokeAsync("the-command", new TestConsole());

            middlewareWasCalled.Should().BeTrue();
            handlerWasCalled.Should().BeFalse();
        }

        [Fact]
        public void Synchronous_invocation_can_be_short_circuited_by_async_middleware_by_not_calling_next()
        {
            var middlewareWasCalled = false;
            var handlerWasCalled = false;

            var command = new Command("the-command");
            command.SetHandler(context =>
            {
                handlerWasCalled = true;
                context.ParseResult.Errors.Should().BeEmpty();
                return Task.FromResult(0);
            });

            var parser = new CommandLineBuilder(new RootCommand
                         {
                             command
                         })
                         .AddMiddleware(async (context, next) =>
                         {
                             middlewareWasCalled = true;
                             await Task.Yield();
                         })
                         .Build();

            parser.Invoke("the-command", new TestConsole());

            middlewareWasCalled.Should().BeTrue();
            handlerWasCalled.Should().BeFalse();
        }

        [Fact]
        public async Task When_no_help_builder_is_specified_it_uses_default_implementation()
        {
            bool handlerWasCalled = false;

            var command = new Command("help-command");
            command.SetHandler(context =>
            {
                handlerWasCalled = true;
                context.HelpBuilder.Should().NotBeNull();
                return Task.FromResult(0);
            });

            var parser = new CommandLineBuilder(new RootCommand
                         {
                             command
                         })
                         .Build();

            await parser.InvokeAsync("help-command", new TestConsole());

            handlerWasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task When_help_builder_factory_is_specified_it_is_used_to_create_the_help_builder()
        {
            bool handlerWasCalled = false;
            bool factoryWasCalled = false;

            HelpBuilder createdHelpBuilder = null;

            var command = new Command("help-command");
            command.SetHandler(context =>
            {
                handlerWasCalled = true;
                context.HelpBuilder.Should().Be(createdHelpBuilder);
                createdHelpBuilder.Should().NotBeNull();
                return Task.FromResult(0);
            });

            var parser = new CommandLineBuilder(new RootCommand
                         {
                             command
                         })
                         .UseHelpBuilder(context =>
                         {
                             factoryWasCalled = true;
                             return createdHelpBuilder = new HelpBuilder(context.ParseResult.Parser.Configuration.LocalizationResources);
                         })
                         .Build();

            await parser.InvokeAsync("help-command", new TestConsole());

            handlerWasCalled.Should().BeTrue();
            factoryWasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Command_InvokeAsync_can_cancel_from_middleware()
        {
            var handlerWasCalled = false;
            var isCancelRequested = false;

            var command = new Command("the-command");
            command.SetHandler((InvocationContext context) =>
            {
                handlerWasCalled = true;
                isCancelRequested = context.GetCancellationToken().IsCancellationRequested;
                return Task.FromResult(0);
            });


            using CancellationTokenSource cts = new();
            var parser = new CommandLineBuilder(new RootCommand
                         {
                             command
                         })
                         .AddMiddleware(async (context, next) =>
                         {
                             context.LinkToken(cts.Token);
                             cts.Cancel();
                             await next(context);
                         })
                         .Build();

            await parser.InvokeAsync("the-command");

            handlerWasCalled.Should().BeTrue();
            isCancelRequested.Should().BeTrue();
        }
    }
}
