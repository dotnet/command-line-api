// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class InvocationPipelineTests
    {
        private readonly TestConsole _console = new TestConsole();

        [Fact]
        public async Task General_invocation_middleware_can_be_specified_in_the_parser()
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

            var parser = new CommandLineBuilder()
                         .AddCommand("first", "",
                                     cmd => cmd.OnExecute(() => firstWasCalled = true))
                         .AddCommand("second", "",
                                     cmd => cmd.OnExecute(() => secondWasCalled = true))
                         .Build();

            await parser.InvokeAsync("first", _console);

            firstWasCalled.Should().BeTrue();
            secondWasCalled.Should().BeFalse();
        }

        [Fact]
        public void When_middleware_throws_then_InvokeAsync_does_not_handle_the_exception()
        {
            var parser = new CommandLineBuilder()
                         .AddCommand("the-command", "")
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
            var parser = new CommandLineBuilder()
                         .AddCommand("the-command", "",
                                     cmd => cmd.OnExecute(() => throw new Exception("oops!")))
                         .Build();

            Func<Task> invoke = async () => await parser.InvokeAsync("the-command", _console);

            invoke.Should()
                  .Throw<TargetInvocationException>()
                  .Which
                  .InnerException
                  .Message
                  .Should()
                  .Be("oops!");
        }

        [Fact]
        public async Task UseExceptionHandler_catches_middleware_exceptions_and_writes_details_to_standard_error()
        {
            var parser = new CommandLineBuilder()
                         .AddCommand("the-command", "")
                         .UseMiddleware(_ => throw new Exception("oops!"))
                         .UseExceptionHandler()
                         .Build();

            var resultCode = await parser.InvokeAsync("the-command", _console);

            _console.Error.ToString().Should().Contain("Unhandled exception: System.Exception: oops!");

            resultCode.Should().Be(1);
        }

        [Fact]
        public async Task UseExceptionHandler_catches_command_handler_exceptions_and_sets_result_code_to_1()
        {
            var parser = new CommandLineBuilder()
                         .AddCommand("the-command", "",
                                     cmd => cmd.OnExecute(() => throw new Exception("oops!")))
                         .UseExceptionHandler()
                         .Build();

            var resultCode = await parser.InvokeAsync("the-command", _console);

            resultCode.Should().Be(1);
        }

        [Fact]
        public async Task UseExceptionHandler_catches_command_handler_exceptions_and_writes_details_to_standard_error()
        {
            var parser = new CommandLineBuilder()
                         .AddCommand("the-command", "",
                                     cmd => cmd.OnExecute(() => throw new Exception("oops!")))
                         .UseExceptionHandler()
                         .Build();

            await parser.InvokeAsync("the-command", _console);

            _console.Error.ToString().Should().Contain("System.Exception: oops!");
        }

        [Fact]
        public async Task Declaration_of_UseExceptionHandler_can_come_before_other_middleware()
        {
            await new CommandLineBuilder()
                  .AddCommand("the-command", "")
                  .UseExceptionHandler()
                  .UseMiddleware(_ => throw new Exception("oops!"))
                  .Build()
                  .InvokeAsync("the-command", _console);

            _console.Error
                    .ToString()
                    .Should()
                    .Contain("oops!");
        }

        [Fact]
        public async Task Declaration_of_UseExceptionHandler_can_come_after_other_middleware()
        {
            await new CommandLineBuilder()
                  .AddCommand("the-command", "")
                  .UseMiddleware(_ => throw new Exception("oops!"))
                  .UseExceptionHandler()
                  .Build()
                  .InvokeAsync("the-command", _console);

            _console.Error
                    .ToString()
                    .Should()
                    .Contain("oops!");
        }

        [Fact]
        public async Task ParseResult_can_be_replaced_by_middleware()
        {
            var wasCalled = false;

            var parser = new CommandLineBuilder()
                         .UseMiddleware(context => {
                             var tokensAfterFirst = context.ParseResult.Tokens.Skip(1).ToArray();
                             var reparsed = context.Parser.Parse(tokensAfterFirst);
                             context.ParseResult = reparsed;
                         })
                         .AddCommand("the-command", "",
                                     cmd => cmd.OnExecute<ParseResult>(result => {
                                         wasCalled = true;
                                         result.Errors.Should().BeEmpty();
                                     }))
                         .Build();

            await parser.InvokeAsync("!my-directive the-command", new TestConsole());

            wasCalled.Should().BeTrue();
        }
    }
}
