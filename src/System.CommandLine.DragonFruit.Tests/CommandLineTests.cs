// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.DragonFruit.Tests
{
    public class CommandLineTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TestConsole _console;

        public CommandLineTests(ITestOutputHelper output)
        {
            _output = output;
            _console = new TestConsole(_output);
        }

        private string _captured;

        private void TestMain(string name) => _captured = name;

        [Fact]
        public async Task It_executes_method_with_string_option()
        {
            Action<string> action = TestMain;
            int exitCode = await CommandLine.InvokeMethodAsync(
                new[] { "--name", "Wayne" },
                _console,
                this,
                action.Method);
            exitCode.Should().Be(0);
            _captured.Should().Be("Wayne");
        }

        [Fact]
        public async Task It_shows_help_text()
        {
            var stdout = new StringBuilder();
            _console.Out = new StringWriter(stdout);
            Action<string> action = TestMain;

            int exitCode = await CommandLine.InvokeMethodAsync(
                new[] { "--help" },
                _console,
                this,
                action.Method);

            exitCode.Should().Be(CommandLine.OkExitCode);
            stdout.ToString().Should()
                .Contain("--name")
                .And.Contain("Options:");
        }

        private void TestMainWithDefault(string name = "Bruce") => _captured = name;

        [Fact]
        public async Task It_executes_method_with_string_option_with_default()
        {
            Action<string> action = TestMainWithDefault;

            int exitCode = await CommandLine.InvokeMethodAsync(
                new[] { "--name", "Wayne" },
                _console,
                this,
                action.Method);

            exitCode.Should().Be(0);
            _captured.Should().Be("Wayne");

            exitCode = await CommandLine.InvokeMethodAsync(
                Array.Empty<string>(),
                _console,
                this,
                action.Method);

            exitCode.Should().Be(0);
            _captured.Should().Be("Bruce");
        }

        private void TestMainThatThrows() => throw new InvalidOperationException("This threw an error");

        [Fact]
        public async Task It_shows_error_without_invoking_method()
        {
            Action action = TestMainThatThrows;

            var stderr = new StringBuilder();
            _console.Error = new StringWriter(stderr);

            int exitCode = await CommandLine.InvokeMethodAsync(
                new[] { "--unknown" },
                _console,
                this,
                action.Method);

            exitCode.Should().Be(CommandLine.ErrorExitCode);
            stderr.ToString()
                .Should().NotBeEmpty()
                .And
                .Contain("--unknown");
            _console.ForegroundColor.Should().Be(ConsoleColor.Red);
        }

        [Fact]
        public async Task It_handles_uncaught_exceptions()
        {
            Action action = TestMainThatThrows;

            var stderr = new StringBuilder();
            _console.Error = new StringWriter(stderr);

            int exitCode = await CommandLine.InvokeMethodAsync(
                Array.Empty<string>(),
                _console,
                this,
                action.Method);

            exitCode.Should().Be(CommandLine.ErrorExitCode);
            stderr.ToString()
                .Should().NotBeEmpty()
                .And
                .Contain("This threw an error");
            _console.ForegroundColor.Should().Be(ConsoleColor.Red);
        }
    }
}
