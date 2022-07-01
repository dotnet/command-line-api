// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class UseExceptionHandlerTests
    {
        private readonly TestConsole _console = new();

        [Fact]
        public async Task Declaration_of_UseExceptionHandler_can_come_after_other_middleware()
        {
            await new CommandLineBuilder(new RootCommand
                  {
                      new Command("the-command")
                  })
                  .AddMiddleware(_ => throw new Exception("oops!"))
                  .UseExceptionHandler()
                  .Build()
                  .InvokeAsync("the-command", _console);

            _console.Error
                    .ToString()
                    .Should()
                    .Contain("oops!");
        }

        [Fact]
        public async Task UseExceptionHandler_catches_middleware_exceptions_and_writes_details_to_standard_error()
        {
            var parser = new CommandLineBuilder(new RootCommand
                         {
                             new Command("the-command")
                         })
                         .AddMiddleware(_ => throw new Exception("oops!"))
                         .UseExceptionHandler()
                         .Build();

            var resultCode = await parser.InvokeAsync("the-command", _console);

            _console.Error.ToString().Should().Contain("Unhandled exception: System.Exception: oops!");

            resultCode.Should().Be(1);
        }

        [Fact]
        public async Task UseExceptionHandler_catches_command_handler_exceptions_and_sets_result_code_to_1()
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
                         .UseExceptionHandler()
                         .Build();

            var resultCode = await parser.InvokeAsync("the-command", _console);

            resultCode.Should().Be(1);
        }

        [Fact]
        public async Task UseExceptionHandler_catches_command_handler_exceptions_and_writes_details_to_standard_error()
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
                         .UseExceptionHandler()
                         .Build();

            await parser.InvokeAsync("the-command", _console);

            _console.Error.ToString().Should().Contain("System.Exception: oops!");
        }

        [Fact]
        public async Task Declaration_of_UseExceptionHandler_can_come_before_other_middleware()
        {
            await new CommandLineBuilder(new RootCommand
                  {
                      new Command("the-command")
                  })
                  .UseExceptionHandler()
                  .AddMiddleware(_ => throw new Exception("oops!"))
                  .Build()
                  .InvokeAsync("the-command", _console);

            _console.Error
                    .ToString()
                    .Should()
                    .Contain("oops!");
        }

        [Fact]
        public async Task When_thrown_exception_is_from_cancelation_no_output_is_generated()
        {
            int resultCode = await new CommandLineBuilder(new RootCommand
                                   {
                                       new Command("the-command")
                                   })
                                   .UseExceptionHandler()
                                   .AddMiddleware(_ => throw new OperationCanceledException())
                                   .Build()
                                   .InvokeAsync("the-command", _console);

            _console.Out.ToString().Should().BeEmpty();
            resultCode.Should().NotBe(0);
        }

        [Fact]
        public async Task UseExceptionHandler_output_can_be_customized()
        {
            int resultCode = await new CommandLineBuilder(new RootCommand
                                   {
                                       new Command("the-command")
                                   })
                                   .UseExceptionHandler((exception, context) =>
                                   {
                                       context.Console.Out.Write("Well that's awkward.");
                                       context.ExitCode = 22;
                                   })
                                   .AddMiddleware(_ => throw new Exception("oops!"))
                                   .Build()
                                   .InvokeAsync("the-command", _console);

            _console.Out.ToString().Should().Be("Well that's awkward.");
            resultCode.Should().Be(22);
        }

        [Fact]
        public async Task UseExceptionHandler_set_custom_result_code()
        {
            int resultCode = await new CommandLineBuilder(new RootCommand
                                   {
                                       new Command("the-command")
                                   })
                                   .UseExceptionHandler(errorExitCode: 42)
                                   .AddMiddleware(_ => throw new Exception("oops!"))
                                   .Build()
                                   .InvokeAsync("the-command", _console);

            resultCode.Should().Be(42);
        }
    }
}