// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Tests.Utility;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace System.CommandLine.Tests
{
    public partial class ParserTests
    {
        public class CaptureRemainingTokens
        {
            [Fact]
            public void Option_like_tokens_after_capturing_argument_are_captured()
            {
                var option = new Option<string>("--source");
                var toolName = new Argument<string>("toolName");
                var toolArgs = new Argument<string[]>("toolArgs") { CaptureRemainingTokens = true };
                var command = new RootCommand
                {
                    option,
                    toolName,
                    toolArgs
                };

                var result = command.Parse("--source https://nuget.org myTool -a 1 --help");

                using var _ = new AssertionScope();
                result.GetValue(option).Should().Be("https://nuget.org");
                result.GetValue(toolName).Should().Be("myTool");
                result.GetValue(toolArgs).Should().BeEquivalentSequenceTo("-a", "1", "--help");
                result.Errors.Should().BeEmpty();
                result.UnmatchedTokens.Should().BeEmpty();
            }

            [Fact]
            public void Known_options_after_capturing_argument_are_captured()
            {
                var verbose = new Option<bool>("--verbose");
                var toolArgs = new Argument<string[]>("toolArgs") { CaptureRemainingTokens = true };
                var command = new RootCommand
                {
                    verbose,
                    toolArgs
                };

                var result = command.Parse("foo --verbose bar");

                using var _ = new AssertionScope();
                result.GetValue(toolArgs).Should().BeEquivalentSequenceTo("foo", "--verbose", "bar");
                result.GetValue(verbose).Should().BeFalse();
                result.Errors.Should().BeEmpty();
            }

            [Fact]
            public void Tokens_matching_subcommand_names_are_captured()
            {
                var sub = new Command("sub");
                var toolName = new Argument<string>("toolName");
                var toolArgs = new Argument<string[]>("toolArgs") { CaptureRemainingTokens = true };
                var command = new RootCommand
                {
                    sub,
                    toolName,
                    toolArgs
                };

                var result = command.Parse("myTool sub --flag");

                using var _ = new AssertionScope();
                result.GetValue(toolName).Should().Be("myTool");
                result.GetValue(toolArgs).Should().BeEquivalentSequenceTo("sub", "--flag");
                result.UnmatchedTokens.Should().BeEmpty();
            }

            [Fact]
            public void Options_before_capturing_argument_are_parsed_normally()
            {
                var source = new Option<string>("--source");
                var toolName = new Argument<string>("toolName");
                var toolArgs = new Argument<string[]>("toolArgs") { CaptureRemainingTokens = true };
                var command = new RootCommand
                {
                    source,
                    toolName,
                    toolArgs
                };

                var result = command.Parse("--source https://nuget.org myTool --help");

                using var _ = new AssertionScope();
                result.GetValue(source).Should().Be("https://nuget.org");
                result.GetValue(toolName).Should().Be("myTool");
                result.GetValue(toolArgs).Should().BeEquivalentSequenceTo("--help");
                result.Errors.Should().BeEmpty();
            }

            [Fact]
            public void Double_dash_before_capturing_argument_works()
            {
                var option = new Option<string>("--source");
                var toolArgs = new Argument<string[]>("toolArgs") { CaptureRemainingTokens = true };
                var command = new RootCommand
                {
                    option,
                    toolArgs
                };

                var result = command.Parse("--source foo -- --help --version");

                using var _ = new AssertionScope();
                result.GetValue(option).Should().Be("foo");
                result.GetValue(toolArgs).Should().BeEquivalentSequenceTo("--help", "--version");
                result.Errors.Should().BeEmpty();
            }

            [Fact]
            public void Double_dash_during_capture_is_captured()
            {
                var toolArgs = new Argument<string[]>("toolArgs") { CaptureRemainingTokens = true };
                var command = new RootCommand
                {
                    toolArgs
                };

                var result = command.Parse("foo -- --help");

                using var _ = new AssertionScope();
                result.GetValue(toolArgs).Should().BeEquivalentSequenceTo("foo", "--", "--help");
                result.Errors.Should().BeEmpty();
            }

            [Fact]
            public void Non_capturing_arguments_are_unaffected()
            {
                var option = new Option<bool>("--verbose");
                var arg = new Argument<string[]>("arg");
                var command = new RootCommand
                {
                    option,
                    arg
                };

                var result = command.Parse("foo --verbose bar");

                using var _ = new AssertionScope();
                result.GetValue(option).Should().Be(true);
                result.GetValue(arg).Should().BeEquivalentSequenceTo("foo", "bar");
            }

            [Fact]
            public void Arity_limits_are_still_respected()
            {
                var toolArg = new Argument<string>("toolArg") { CaptureRemainingTokens = true };
                var command = new RootCommand
                {
                    toolArg
                };

                var result = command.Parse("first --extra");

                using var _ = new AssertionScope();
                result.GetValue(toolArg).Should().Be("first");
                result.UnmatchedTokens.Should().BeEquivalentTo("--extra");
            }

            [Fact]
            public void Empty_input_with_capturing_argument_produces_no_errors_when_arity_allows()
            {
                var toolArgs = new Argument<string[]>("toolArgs") { CaptureRemainingTokens = true };
                var command = new RootCommand
                {
                    toolArgs
                };

                var result = command.Parse("");

                using var _ = new AssertionScope();
                result.GetValue(toolArgs).Should().BeEmpty();
                result.Errors.Should().BeEmpty();
            }

            [Fact]
            public void Option_with_value_syntax_is_captured_as_single_token()
            {
                var toolArgs = new Argument<string[]>("toolArgs") { CaptureRemainingTokens = true };
                var command = new RootCommand
                {
                    toolArgs
                };

                var result = command.Parse("--key=value -x:y");

                using var _ = new AssertionScope();
                result.GetValue(toolArgs).Should().BeEquivalentSequenceTo("--key=value", "-x:y");
                result.Errors.Should().BeEmpty();
            }

            [Fact]
            public void Capturing_argument_on_subcommand_works()
            {
                var toolName = new Argument<string>("toolName");
                var toolArgs = new Argument<string[]>("toolArgs") { CaptureRemainingTokens = true };
                var exec = new Command("exec")
                {
                    toolName,
                    toolArgs
                };
                exec.Options.Add(new Option<bool>("--verbose"));
                var command = new RootCommand
                {
                    exec
                };

                var result = command.Parse("exec foo --verbose bar");

                using var _ = new AssertionScope();
                result.GetValue(toolName).Should().Be("foo");
                result.GetValue(toolArgs).Should().BeEquivalentSequenceTo("--verbose", "bar");
                result.Errors.Should().BeEmpty();
            }

            [Fact]
            public void Trailing_double_dash_is_captured()
            {
                var toolArgs = new Argument<string[]>("toolArgs") { CaptureRemainingTokens = true };
                var command = new RootCommand
                {
                    toolArgs
                };

                var result = command.Parse("foo --");

                using var _ = new AssertionScope();
                result.GetValue(toolArgs).Should().BeEquivalentSequenceTo("foo", "--");
                result.Errors.Should().BeEmpty();
            }

            [Fact]
            public void Leading_known_option_is_parsed_normally_when_capture_is_first_argument()
            {
                var verbose = new Option<bool>("--verbose");
                var toolArgs = new Argument<string[]>("toolArgs") { CaptureRemainingTokens = true };
                var command = new RootCommand
                {
                    verbose,
                    toolArgs
                };

                var result = command.Parse("--verbose foo --unknown");

                using var _ = new AssertionScope();
                result.GetValue(verbose).Should().BeTrue();
                result.GetValue(toolArgs).Should().BeEquivalentSequenceTo("foo", "--unknown");
                result.Errors.Should().BeEmpty();
            }

            [Fact]
            public void Scalar_capture_escapes_one_token_then_resumes_normal_parsing()
            {
                var verbose = new Option<bool>("--verbose");
                var first = new Argument<string>("first");
                var target = new Argument<string>("target") { CaptureRemainingTokens = true };
                var command = new RootCommand
                {
                    verbose,
                    first,
                    target
                };

                var result = command.Parse("foo --verbose --verbose");

                using var _ = new AssertionScope();
                result.GetValue(first).Should().Be("foo");
                result.GetValue(target).Should().Be("--verbose");
                result.GetValue(verbose).Should().BeTrue();
                result.Errors.Should().BeEmpty();
            }

            [Fact]
            public void Options_after_scalar_capture_overflow_are_parsed_normally()
            {
                var verbose = new Option<bool>("--verbose");
                var target = new Argument<string>("target") { CaptureRemainingTokens = true };
                var command = new RootCommand
                {
                    verbose,
                    target
                };

                var result = command.Parse("hello --verbose");

                using var _ = new AssertionScope();
                result.GetValue(target).Should().Be("hello");
                result.GetValue(verbose).Should().BeTrue();
                result.Errors.Should().BeEmpty();
            }
        }
    }
}
