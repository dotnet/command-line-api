﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.IO;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Parsing
{
    public class CommandLineStringSplitterTests
    {
        private readonly ICommandLineStringSplitter _splitter = CommandLineStringSplitter.Instance;

        [Theory]
        [InlineData("one two three four")]
        [InlineData("one two\tthree   four ")]
        [InlineData(" one two three   four")]
        [InlineData(" one\ntwo\nthree\nfour\n")]
        [InlineData(" one\r\ntwo\r\nthree\r\nfour\r\n")]
        public void It_splits_strings_based_on_whitespace(string commandLine)
        {
            _splitter.Split(commandLine)
                     .Should()
                     .BeEquivalentSequenceTo("one", "two", "three", "four");
        }

        [Fact]
        public void It_does_not_break_up_double_quote_delimited_values()
        {
            var commandLine = @"rm -r ""c:\temp files\""";

            _splitter.Split(commandLine)
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

            _splitter.Split(commandLine)
                     .Should()
                     .BeEquivalentSequenceTo("the-command", optionAndArgument.Replace("\"", ""));
        }

        [Fact]
        public void It_handles_multiple_options_with_quoted_arguments()
        {
            var source = Directory.GetCurrentDirectory();
            var destination = Path.Combine(Directory.GetCurrentDirectory(), ".trash");

            var commandLine = $"move --from \"{source}\" --to \"{destination}\" --verbose";

            var tokenized = _splitter.Split(commandLine);

            tokenized.Should()
                     .BeEquivalentSequenceTo(
                         "move",
                         "--from",
                         source,
                         "--to",
                         destination,
                         "--verbose");
        }
    }
}
