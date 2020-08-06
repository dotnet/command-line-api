// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;

namespace System.CommandLine.Rendering.Tests
{
    public abstract class TerminalTests
    {
        protected abstract ITerminal GetTerminal();

        [Fact]
        public void Setting_CursorLeft_below_zero_throws()
        {
            var console = GetTerminal();

            if (console.IsOutputRedirected)
            {
                return;
            }

            console.Invoking(c => c.CursorLeft = -1)
                   .Should()
                   .Throw<ArgumentOutOfRangeException>()
                   .WithMessage(
                       $"The value must be greater than or equal to zero and less than the console's buffer size in that dimension. (Parameter 'left'){Environment.NewLine}Actual value was -1.");
        }

        [Fact]
        public void Setting_CursorTop_below_zero_throws()
        {
            var console = GetTerminal();

            if (console.IsOutputRedirected)
            {
                return;
            }

            console.Invoking(c => c.CursorTop = -1)
                   .Should()
                   .Throw<ArgumentOutOfRangeException>()
                   .WithMessage(
                       $"The value must be greater than or equal to zero and less than the console's buffer size in that dimension. (Parameter 'top'){Environment.NewLine}Actual value was -1.");
        }

        [Fact(Skip = "How to test?")]
        public void When_output_is_redirected_then_there_is_no_terminal()
        {
            // var console = GetTerminal();

            // SetOut(new StringWriter());

            // var terminal = console.GetTerminal(true);

            // terminal.Should().BeNull();
        }
    }
}
