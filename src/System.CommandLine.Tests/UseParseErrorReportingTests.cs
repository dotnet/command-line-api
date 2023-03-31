﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Help;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class UseParseErrorReportingTests
    {
        [Fact] // https://github.com/dotnet/command-line-api/issues/817
        public void Parse_error_reporting_reports_error_when_help_is_used_and_required_subcommand_is_missing()
        {
            var root = new CliRootCommand
            {
                new CliCommand("inner"),
                new HelpOption()
            };

            CliConfiguration config = new (root)
            {
                EnableParseErrorReporting = true
            };

            var parseResult = root.Parse("", config);

            parseResult.Errors.Should().NotBeEmpty();

            var result = config.Invoke("");

            result.Should().Be(1);
        }

        [Fact]
        public void User_can_customize_parse_error_result_code()
        {
            var root = new CliRootCommand
            {
                new CliCommand("inner")
            };

            CliConfiguration config = new (root)
            {
                EnableParseErrorReporting = true
            };

            ParseResult parseResult = root.Parse("", config);

            int result = parseResult.Invoke();

            if (parseResult.Errors.Any())
            {
                result = 42;
            }

            result.Should().Be(42);
        }
    }
}