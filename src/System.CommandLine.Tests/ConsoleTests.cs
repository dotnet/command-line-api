// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public abstract class ConsoleTests
    {
        protected abstract ITerminal GetConsole();

        [Fact]
        public void Setting_CursorLeft_below_zero_throws()
        {
            var console = GetConsole();

            console.Invoking(c => c.CursorLeft = -1)
                   .Should()
                   .Throw<ArgumentOutOfRangeException>()
                   .WithMessage($"The value must be greater than or equal to zero and less than the console's buffer size in that dimension.{Environment.NewLine}Parameter name: left{Environment.NewLine}Actual value was -1.");
        }

        [Fact]
        public void Setting_CursorTop_below_zero_throws()
        {
            var console = GetConsole();

            console.Invoking(c => c.CursorTop = -1)
                   .Should()
                   .Throw<ArgumentOutOfRangeException>()
                   .WithMessage($"The value must be greater than or equal to zero and less than the console's buffer size in that dimension.{Environment.NewLine}Parameter name: top{Environment.NewLine}Actual value was -1.");
        }

        [Fact]
        public void Virtual_terminal_mode_cannot_be_enabled_when_output_is_redirected()
        {
            var console = GetConsole();

            console.SetOut(new StringWriter());

            console.TryEnableVirtualTerminal();

            console.IsVirtualTerminal.Should().BeFalse();
        }
    }
}
