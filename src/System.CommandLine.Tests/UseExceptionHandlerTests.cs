// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class UseExceptionHandlerTests
    {
        [Fact]
        public async Task UseExceptionHandler_catches_command_handler_exceptions_and_sets_result_code_to_1()
        {
            var command = new CliCommand("the-command");
            command.SetAction((_, __) => Task.FromException<int>(new Exception("oops!")));

            CliConfiguration config = new(command)
            {
                Error = new StringWriter(),
            };

            var resultCode = await config.InvokeAsync("the-command");

            resultCode.Should().Be(1);
        }

        [Fact]
        public async Task UseExceptionHandler_catches_command_handler_exceptions_and_writes_details_to_standard_error()
        {
            var command = new CliCommand("the-command");
            command.SetAction((_, __) => Task.FromException<int>(new Exception("oops!")));

            CliConfiguration config = new(command)
            {
                Error = new StringWriter(),
            };

            await config.InvokeAsync("the-command");

            config.Error.ToString().Should().Contain("System.Exception: oops!");
        }

        [Fact]
        public async Task When_thrown_exception_is_from_cancelation_no_output_is_generated()
        {
            CliCommand command = new("the-command");
            command.SetAction((_, __) => throw new OperationCanceledException());

            CliConfiguration config = new(command)
            {
                Output = new StringWriter(),
                Error = new StringWriter()
            };

            int resultCode = await config
                                   .InvokeAsync("the-command");

            config.Output.ToString().Should().BeEmpty();
            resultCode.Should().NotBe(0);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Exception_output_can_be_customized(bool async)
        {
            Exception expectedException = new ("oops!");
            CliCommand command = new("the-command");
            command.SetAction((_, __) => throw expectedException);

            CliConfiguration config = new(command)
            {
                Error = new StringWriter(),
                EnableDefaultExceptionHandler = false
            };

            ParseResult parseResult = command.Parse("the-command", config);

            int resultCode = 0;

            try
            {
                resultCode = async ? await parseResult.InvokeAsync() : parseResult.Invoke();
            }
            catch (Exception ex)
            {
                ex.Should().Be(expectedException);
                parseResult.Configuration.Error.Write("Well that's awkward.");
                resultCode = 22;
            }

            config.Error.ToString().Should().Be("Well that's awkward.");
            resultCode.Should().Be(22);
        }
    }
}