// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Tests;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Rendering.Tests
{
    public class TerminalModeTests
    {
        [Fact(Skip = "How to test?")]
        public void Sets_outputMode_to_file_when_output_is_redirected()
        {
            var terminal = new TestTerminal();

            // terminal.SetOut(new StringWriter());

            var outputMode = terminal.DetectOutputMode();

            outputMode.Should().Be(OutputMode.File);
        }

        [WindowsOnlyFact(Skip = "How to test?")]
        public void Sets_outputMode_to_ansi_when_windows_and_virtual_terminal()
        {
            var console = new TestConsole();

            var outputMode = console.DetectOutputMode();

            outputMode.Should().Be(OutputMode.Ansi);
        }

        [WindowsOnlyFact(Skip = "How to test?")]
        public void Sets_outputMode_to_nonansi_when_windows_and_no_virtual_terminal()
        {
            var console = new TestConsole();

            var outputMode = console.DetectOutputMode();

            outputMode.Should().Be(OutputMode.NonAnsi);
        }

        [NonWindowsOnlyFact(Skip = "How to test?")]
        public void Sets_outputMode_to_ansi_when_not_windows_and_xterm()
        {
            var console = new TestConsole();

            var outputMode = console.DetectOutputMode();

            outputMode.Should().Be(OutputMode.Ansi);
        }

        [NonWindowsOnlyFact(Skip = "How to test?")]
        public void Sets_outputMode_to_nonansi_when_not_windows_and_no_xterm()
        {
            var console = new TestConsole();

            var outputMode = console.DetectOutputMode();

            outputMode.Should().Be(OutputMode.NonAnsi);
        }
    }
}
