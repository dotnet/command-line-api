// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests;

public class ParseResultValueTests
{
    [Fact]
    public void Symbol_found_by_name()
    {
        var option1 = new CliOption<string>("--opt1");
        var option2 = new CliOption<string>("--opt2");

        var rootCommand = new CliRootCommand
                    {
                        option1,
                        option2
                    };

        var parseResult = CliParser.Parse(rootCommand, "--opt1 Kirk");

        var symbol1 = parseResult.GetSymbolByName("--opt1");
        var symbol2 = parseResult.GetSymbolByName("--opt2");
        symbol1.Should().Be(option1);
        symbol2.Should().Be(option2);
    }

    [Fact]
    public void Nearest_symbol_found_when_multiple()
    {
        var option1 = new CliOption<string>("--opt1", "-1");
        var option2 = new CliOption<string>("--opt1", "-2");

        var command = new CliCommand("subcommand")
                    {
                        option2
                    };

        var rootCommand = new CliRootCommand
                    {
                        command,
                        option1
                    };

        var parseResult = CliParser.Parse(rootCommand, "subcommand --opt2 Spock");

        var symbol = parseResult.GetSymbolByName("--opt1");
        symbol.Should().Be(option2);
    }
}