// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class InvocationExtensionsTests
    {
        [Fact]
        public async Task Command_InvokeAsync_uses_default_pipeline_by_default()
        {
            var command = new Command("the-command");
            var theHelpText = "the help text";
            command.Description = theHelpText;

            var console = new TestConsole();

            await command.InvokeAsync("-h", console);

            console.Out
                   .ToString()
                   .Should()
                   .Contain(theHelpText);
        }

        [Fact]
        public async Task RootCommand_InvokeAsync_returns_0_when_handler_is_successful()
        {
            var wasCalled = false;
            var rootCommand = new RootCommand();

            rootCommand.Handler = CommandHandler.Create(() => wasCalled = true);

            var result = await rootCommand.InvokeAsync("");

            wasCalled.Should().BeTrue();
            result.Should().Be(0);
        }

        [Fact]
        public async Task RootCommand_InvokeAsync_returns_1_when_handler_throws()
        {
            var wasCalled = false;
            var rootCommand = new RootCommand();

            rootCommand.Handler = CommandHandler.Create(() =>
            {
                wasCalled = true;
                throw new Exception("oops!");
            });

            var resultCode = await rootCommand.InvokeAsync("");

            wasCalled.Should().BeTrue();
            resultCode.Should().Be(1);
        }

        [Fact]
        public async Task RootCommand_InvokeAsync_can_set_custom_result_code()
        {
            var rootCommand = new RootCommand();

            rootCommand.Handler = CommandHandler.Create<InvocationContext>(context =>
            {
                context.ResultCode = 123;
            });

            var resultCode = await rootCommand.InvokeAsync("");

            resultCode.Should().Be(123);
        }
    }
}
