// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.CommandLine.Directives;
using System.CommandLine.Parsing;
using Xunit;

namespace System.CommandLine.Subsystems.Tests;

public class ResponseSubsystemTests
{

    [Fact]
    // TODO: Not sure why these tests are passing
    public void Simple_response_file_contributes_to_parsing()
    {
        var option = new CliOption<string>("--hello");
        var rootCommand = new CliRootCommand { option };
        var configuration = new CliConfiguration(rootCommand);
        var subsystem = new ResponseSubsystem();
        subsystem.Enabled = true;
        string[] args = ["@Response_1.rsp"];

        Subsystem.Initialize(subsystem, configuration, args);

        var parseResult = CliParser.Parse(rootCommand, args, configuration);
        var value = parseResult.GetValue(option);

        value.Should().Be("world");
    }
}
