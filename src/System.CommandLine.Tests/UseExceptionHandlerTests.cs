// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.CommandLine.Tests
{
    public class UseExceptionHandlerTests
    {
        [Fact]
        public async Task UseExceptionHandler_catches_command_handler_exceptions_and_sets_result_code_to_1()
        {
            var command = new Command("the-command");
            command.SetAction((_, __) => Task.FromException<int>(new Exception("oops!")));

            var resultCode = await command.Parse("the-command").InvokeAsync(new() { Error = new StringWriter() },CancellationToken.None);

            resultCode.Should().Be(1);
        }

        [Fact]
        public async Task UseExceptionHandler_catches_command_handler_exceptions_and_writes_details_to_standard_error()
        {
            var command = new Command("the-command");
            command.SetAction((_, __) => Task.FromException<int>(new Exception("oops!")));

            var error = new StringWriter();

            await command.Parse("the-command").InvokeAsync(new() { Error = error }, CancellationToken.None);

            error.ToString().Should().Contain("System.Exception: oops!");
        }

        [Fact]
        public async Task When_thrown_exception_is_from_cancelation_no_output_is_generated()
        {
            Command command = new("the-command");
            command.SetAction((_, __) => throw new OperationCanceledException());

            var output = new StringWriter();
            var error = new StringWriter();

            int resultCode = await command.Parse("the-command").InvokeAsync(new() { Output = output, Error = error }, CancellationToken.None);

            output.ToString().Should().BeEmpty();
            resultCode.Should().NotBe(0);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Exception_output_can_be_customized(bool async)
        {
            Exception expectedException = new ("oops!");
            Command command = new("the-command");
            command.SetAction((_, __) => throw expectedException);

            InvocationConfiguration config = new()
            {
                Error = new StringWriter(),
                EnableDefaultExceptionHandler = false
            };

            ParseResult parseResult = command.Parse("the-command");

            int resultCode = 0;

            try
            {
                resultCode = async 
                                 ? await parseResult.InvokeAsync(config) 
                                 : parseResult.Invoke(config);
            }
            catch (Exception ex)
            {
                ex.Should().Be(expectedException);
                parseResult.InvocationConfiguration.Error.Write("Well that's awkward.");
                resultCode = 22;
            }

            config.Error.ToString().Should().Be("Well that's awkward.");
            resultCode.Should().Be(22);
        }
    }
}