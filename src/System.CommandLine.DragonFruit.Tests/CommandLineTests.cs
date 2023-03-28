// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.DragonFruit.Tests
{
    public class CommandLineTests
    {
        private readonly TestProgram _testProgram;

        public CommandLineTests()
        {
            _testProgram = new TestProgram();
        }

        [Fact]
        public async Task It_executes_method_with_string_option()
        {
            StringWriter output = new();
            int exitCode = await CommandLine.InvokeMethodAsync(
                               new[] { "--name", "Wayne" },
                               TestProgram.TestMainMethodInfoWithoutPara,
                               null,
                               _testProgram,
                               output);
            exitCode.Should().Be(0);
            output.ToString().Should().Be("Wayne");
        }

        [Fact]
        public void It_executes_method_synchronously_with_string_option()
        {
            StringWriter output = new();
            int exitCode = CommandLine.InvokeMethod(
                new[] { "--name", "Wayne" },
                TestProgram.TestMainMethodInfoWithoutPara,
                null,
                _testProgram,
                output);
            exitCode.Should().Be(0);
            output.ToString().Should().Be("Wayne");
        }

        [Fact]
        public async Task It_shows_help_text_based_on_XML_documentation_comments()
        {
            StringWriter output = new();
            int exitCode = await CommandLine.InvokeMethodAsync(
                               new[] { "--help" },
                               TestProgram.TestMainMethodInfoWithoutPara,
                               null,
                               _testProgram, 
                               output);

            exitCode.Should().Be(0);

            var stdOut = output.ToString();

            stdOut.Should()
                .Contain("<args>  These are arguments")
                .And.Contain("Arguments:");
            stdOut.Should()
                .ContainAll("--name", "Specifies the name option")
                .And.Contain("Options:");
            stdOut.Should()
                .Contain($"Description:{Environment.NewLine}  Normal summary");
        }
        
        [Fact]
        public async Task When_XML_documentation_comment_contains_a_para_tag_then_help_is_written_with_a_newline()
        {
            StringWriter output = new();
            int exitCode = await CommandLine.InvokeMethodAsync(
                new[] { "--help" },
                TestProgram.TestMainMethodInfoWithPara,
                null,
                _testProgram, 
                output);

            exitCode.Should().Be(0);

            var stdOut = output.ToString();

            stdOut.Should()
                .Contain("<args>  These are arguments")
                .And.Contain("Arguments:");
            stdOut.Should()
                .ContainAll("--name", "Specifies the name option")
                .And.Contain("Options:");
            stdOut.Should()
                .Contain($"Description:{Environment.NewLine}  Help for the test program{Environment.NewLine}  More help for the test program{Environment.NewLine}");
        }

        [Fact]
        public async Task When_XML_documentation_comment_contains_a_para_tag_and_some_text_then_help_skips_text_outside_para_tag()
        {
            StringWriter output = new();
            int exitCode = await CommandLine.InvokeMethodAsync(
                new[] { "--help" },
                TestProgram.TestMainMethodInfoWithTextAndPara,
                null,
                _testProgram, 
                output);

            exitCode.Should().Be(0);

            var stdOut = output.ToString();

            stdOut.Should()
                .Contain("<args>  These are arguments")
                .And.Contain("Arguments:");
            stdOut.Should()
                .ContainAll("--name", "Specifies the name option")
                .And.Contain("Options:");
            stdOut.Should()
                .Contain($"Description:{Environment.NewLine}  Help for the test program{Environment.NewLine}  More help for the test program{Environment.NewLine}");
        }

        [Fact]
        public void It_synchronously_shows_help_text_based_on_XML_documentation_comments()
        {
            StringWriter output = new();
            int exitCode = CommandLine.InvokeMethod(
                new[] { "--help" },
                TestProgram.TestMainMethodInfoWithDefault,
                null,
                _testProgram,
                output);

            exitCode.Should().Be(0);

            var stdOut = output.ToString();

            stdOut.Should()
                .ContainAll("--name","name [default: Bruce]")
                .And.Contain("Options:");
        }

        [Fact]
        public async Task It_executes_method_with_string_option_with_default()
        {
            StringWriter output = new();
            int exitCode = await CommandLine.InvokeMethodAsync(
                               Array.Empty<string>(),
                               TestProgram.TestMainMethodInfoWithDefault,
                               null,
                               _testProgram, 
                               output);

            exitCode.Should().Be(0);
            output.ToString().Should().Be("Bruce");
        }

        [Fact]
        public void It_executes_method_synchronously_with_string_option_with_default()
        {
            StringWriter output = new();
            int exitCode = CommandLine.InvokeMethod(
                Array.Empty<string>(),
                TestProgram.TestMainMethodInfoWithDefault,
                null,
                _testProgram,
                output);
            
            exitCode.Should().Be(0);
            output.ToString().Should().Be("Bruce");
        }

        private void TestMainThatThrows() => throw new InvalidOperationException("This threw an error");

        [Fact]
        public async Task It_shows_error_without_invoking_method()
        {
            Action action = TestMainThatThrows;

            StringWriter error = new();
            int exitCode = await CommandLine.InvokeMethodAsync(
                               new[] { "--unknown" },
                               action.Method,
                               null,
                               this, 
                               standardError: error);

            exitCode.Should().Be(1);
            error.ToString()
                    .Should().NotBeEmpty()
                    .And
                    .Contain("--unknown");
        }

        [Fact]
        public void It_shows_error_without_invoking_method_synchronously()
        {
            Action action = TestMainThatThrows;

            StringWriter error = new();
            int exitCode = CommandLine.InvokeMethod(
                new[] { "--unknown" },
                action.Method,
                null,
                this,
                standardError: error);

            exitCode.Should().Be(1);
            error.ToString()
                .Should().NotBeEmpty()
                .And
                .Contain("--unknown");
        }

        [Fact]
        public async Task It_handles_uncaught_exceptions()
        {
            Action action = TestMainThatThrows;

            StringWriter error = new();
            int exitCode = await CommandLine.InvokeMethodAsync(
                               Array.Empty<string>(),
                               action.Method,
                               null,
                               this, 
                               standardError: error);

            exitCode.Should().Be(1);
            error.ToString()
                    .Should().NotBeEmpty()
                    .And
                    .Contain("This threw an error");
        }

        [Fact]
        public void It_handles_uncaught_exceptions_synchronously()
        {
            Action action = TestMainThatThrows;

            StringWriter error = new();
            int exitCode = CommandLine.InvokeMethod(
                Array.Empty<string>(),
                action.Method,
                null,
                this,
                standardError: error);

            exitCode.Should().Be(1);
            error.ToString()
                .Should().NotBeEmpty()
                .And
                .Contain("This threw an error");
        }
    }
}
