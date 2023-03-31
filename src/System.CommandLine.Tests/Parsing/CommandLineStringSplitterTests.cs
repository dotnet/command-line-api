// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.CommandLine.Tests.Utility;
using System.IO;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Parsing
{
    public class CommandLineStringSplitterTests
    {
        [Theory]
        [InlineData("one two three four")]
        [InlineData("one two\tthree   four ")]
        [InlineData(" one two three   four")]
        [InlineData(" one\ntwo\nthree\nfour\n")]
        [InlineData(" one\r\ntwo\r\nthree\r\nfour\r\n")]
        public void It_splits_strings_based_on_whitespace(string commandLine)
        {
            CliParser.SplitCommandLine(commandLine)
                     .Should()
                     .BeEquivalentSequenceTo("one", "two", "three", "four");
        }

        [Fact]
        public void It_does_not_break_up_double_quote_delimited_values()
        {
            var commandLine = @"rm -r ""c:\temp files\""";

            CliParser.SplitCommandLine(commandLine)
                     .Should()
                     .BeEquivalentSequenceTo("rm", "-r", @"c:\temp files\");
        }

        [Theory]
        [InlineData("-", '=')]
        [InlineData("-", ':')]
        [InlineData("--", '=')]
        [InlineData("--", ':')]
        [InlineData("/", '=')]
        [InlineData("/", ':')]
        public void It_does_not_split_double_quote_delimited_values_when_a_non_whitespace_argument_delimiter_is_used(
            string prefix,
            char delimiter)
        {
            var optionAndArgument = $@"{prefix}the-option{delimiter}""c:\temp files\""";

            var commandLine = $"the-command {optionAndArgument}";

            CliParser.SplitCommandLine(commandLine)
                     .Should()
                     .BeEquivalentSequenceTo("the-command", optionAndArgument.Replace("\"", ""));
        }

        [Fact]
        public void It_handles_multiple_options_with_quoted_arguments()
        {
            var source = Directory.GetCurrentDirectory();
            var destination = Path.Combine(Directory.GetCurrentDirectory(), ".trash");

            var commandLine = $"move --from \"{source}\" --to \"{destination}\" --verbose";

            var tokenized = CliParser.SplitCommandLine(commandLine);

            tokenized.Should()
                     .BeEquivalentSequenceTo(
                         "move",
                         "--from",
                         source,
                         "--to",
                         destination,
                         "--verbose");
        }

        [Fact]
        public void Internal_quotes_do_not_cause_string_to_be_split()
        {
            var commandLine = @"POST --raw='{""Id"":1,""Name"":""Alice""}'";

            CliParser.SplitCommandLine(commandLine)
                     .Should()
                     .BeEquivalentTo("POST", "--raw='{Id:1,Name:Alice}'");
        }

        [Fact]
        public void Internal_whitespaces_are_preserved_and_do_not_cause_string_to_be_split()
        {
            var commandLine = @"command --raw='{""Id"":1,""Movie Name"":""The Three Musketeers""}'";

            CliParser.SplitCommandLine(commandLine)
                     .Should()
                     .BeEquivalentTo("command", "--raw='{Id:1,Movie Name:The Three Musketeers}'");
        }
    }
}
