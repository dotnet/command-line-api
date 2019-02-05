// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests
{
    public class TokenizeTests
    {
        private ITestOutputHelper _output;

        public TokenizeTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Tokenize_splits_strings_based_on_whitespace()
        {
            var commandLine = "one two\tthree   four ";

            commandLine.Tokenize()
                       .Should()
                       .BeEquivalentTo("one", "two", "three", "four");
        }

        [Fact]
        public void Tokenize_does_not_break_up_double_quote_delimited_values()
        {
            var commandLine = @"rm -r ""c:\temp files\""";

            commandLine.Tokenize()
                       .Should()
                       .BeEquivalentTo("rm", "-r", @"c:\temp files\");
        }

        [Theory]
        [InlineData("-", '=')]
        [InlineData("-", ':')]
        [InlineData("--", '=')]
        [InlineData("--", ':')]
        [InlineData("/", '=')]
        [InlineData("/", ':')]
        public void Tokenize_does_not_split_double_quote_delimited_values_when_a_non_whitespace_argument_delimiter_is_used(
            string prefix,
            char delimiter)
        {
            var optionAndArgument = $@"{prefix}the-option{delimiter}""c:\temp files\""";

            var commandLine = $"the-command {optionAndArgument}";

            commandLine.Tokenize()
                       .Should()
                       .BeEquivalentTo("the-command", optionAndArgument.Replace("\"", ""));
        }

        [Fact]
        public void Tokenize_handles_multiple_options_with_quoted_arguments()
        {
            var source = Directory.GetCurrentDirectory();
            var destination = Path.Combine(Directory.GetCurrentDirectory(), ".trash");

            var commandLine = $"move --from \"{source}\" --to \"{destination}\"";

            var tokenized = commandLine.Tokenize();

            _output.WriteLine(commandLine);

            foreach (var token in tokenized)
            {
                _output.WriteLine("         " + token);
            }

            tokenized.Should()
                     .BeEquivalentTo("move", "--from", source, "--to", destination);

        }
    }
}
