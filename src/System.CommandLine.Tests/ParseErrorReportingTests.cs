// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.IO;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests;

public class ParseErrorReportingTests
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
    public void Help_display_can_be_disabled()
    {
        CliRootCommand rootCommand = new()
        {
            new CliOption<bool>("--verbose")
        };

        CliConfiguration config = new(rootCommand)
        {
            Output = new StringWriter()
        };

        var result = rootCommand.Parse("oops", config);

        if (result.Action is ParseErrorAction parseError)
        {
            parseError.ShowHelp = false;
        }

        result.Invoke();

        config.Output.ToString().Should().NotContain("--verbose");
    }
}