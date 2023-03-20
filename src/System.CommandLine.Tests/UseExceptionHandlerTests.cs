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
        public async Task UseExceptionHandler_catches_command_handler_exceptions_and_sets_result_code_to_1()
        {
            var command = new Command("the-command");
            command.SetAction((_, __) => Task.FromException<int>(new Exception("oops!")));

            var config = new CommandLineBuilder(new RootCommand
                         {
                             command
                         })
                         .UseExceptionHandler()
                         .Build();

            var resultCode = await config.InvokeAsync("the-command", _console);

            resultCode.Should().Be(1);
        }

        [Fact]
        public async Task UseExceptionHandler_catches_command_handler_exceptions_and_writes_details_to_standard_error()
        {
            var command = new Command("the-command");
            command.SetAction((_, __) => Task.FromException<int>(new Exception("oops!")));

            var config = new CommandLineBuilder(new RootCommand
                         {
                             command
                         })
                         .UseExceptionHandler()
                         .Build();

            await config.InvokeAsync("the-command", _console);

            _console.Error.ToString().Should().Contain("System.Exception: oops!");
        }

        [Fact]
        public async Task When_thrown_exception_is_from_cancelation_no_output_is_generated()
        {
            Command command = new("the-command");
            command.SetAction((_, __) => throw new OperationCanceledException());

            int resultCode = await new CommandLineBuilder(command)
                                   .UseExceptionHandler()
                                   .Build()
                                   .InvokeAsync("the-command", _console);

            _console.Out.ToString().Should().BeEmpty();
            resultCode.Should().NotBe(0);
        }

        [Fact]
        public async Task UseExceptionHandler_output_can_be_customized()
        {
            Command command = new("the-command");
            command.SetAction((_, __) => throw new Exception("oops!"));

            int resultCode = await new CommandLineBuilder(command)
                                   .UseExceptionHandler((exception, context) =>
                                   {
                                       context.Console.Out.Write("Well that's awkward.");
                                       return 22;
                                   })
                                   .Build()
                                   .InvokeAsync("the-command", _console);

            _console.Out.ToString().Should().Be("Well that's awkward.");
            resultCode.Should().Be(22);
        }

        [Fact]
        public async Task UseExceptionHandler_set_custom_result_code()
        {
            Command command = new("the-command");
            command.SetAction((_, __) => throw new Exception("oops!"));

            int resultCode = await new CommandLineBuilder(command)
                                   .UseExceptionHandler(errorExitCode: 42)
                                   .Build()
                                   .InvokeAsync("the-command", _console);

            resultCode.Should().Be(42);
        }
    }
}