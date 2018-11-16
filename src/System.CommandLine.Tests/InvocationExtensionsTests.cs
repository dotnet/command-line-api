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
            command.Help.Description = theHelpText;

            var console = new TestConsole();

            await command.InvokeAsync("-h", console);

            console.Out
                   .ToString()
                   .Should()
                   .Contain(theHelpText);
        }
    }
}
