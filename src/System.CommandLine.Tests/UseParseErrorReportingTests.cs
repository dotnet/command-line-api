// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class UseParseErrorReportingTests
    {
        [Fact] // https://github.com/dotnet/command-line-api/issues/817
        public void Parse_error_reporting_reports_error_when_help_is_used_and_required_subcommand_is_missing()
        {
            var root = new RootCommand
            {
                new Command("inner")
            };

            var parser = new CommandLineBuilder(root)
                         .UseParseErrorReporting()
                         .UseHelp()
                         .Build();

            var parseResult = parser.Parse("");

            parseResult.Errors.Should().NotBeEmpty();

            var result = parser.Invoke("");

            result.Should().Be(1);
        }

        [Fact]
        public void Parse_error_uses_custom_error_result_code()
        {
            var root = new RootCommand
            {
                new Command("inner")
            };

            var parser = new CommandLineBuilder(root)
                         .UseParseErrorReporting(errorExitCode: 42)
                         .Build();

            int result = parser.Invoke("");

            result.Should().Be(42);
        }
    }
}