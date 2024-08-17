// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.CommandLine.Directives;
using System.CommandLine.Parsing;
using Xunit;
using static System.CommandLine.Subsystems.Tests.TestData;

namespace System.CommandLine.Subsystems.Tests;

public class ValidationSubsystemTests
{
    // Running exactly the same code is important here because missing a step will result in a false positive. Ask me how I know
    private (CliCommand rootCommand, CliConfiguration configuration) GetCliWithRange<T>(T lowerBound, T upperBound)
        where T: IComparable<T>
    {
        var option = new CliOption<int>("--intOpt");
        option.SetRange(lowerBound, upperBound);
        var rootCommand = new CliRootCommand { option };
        return (rootCommand, new CliConfiguration(rootCommand));
    }

    private PipelineResult ExecutedPipelineResultForRange<T>(T lowerBound, T upperBound, string input)
        where T : IComparable<T>
    {
        (var rootCommand, var configuration) = GetCliWithRange(lowerBound, upperBound);
        var validationSubsystem = ValidationSubsystem.Create();
        var parseResult = CliParser.Parse(rootCommand, input, configuration);
        var pipelineResult = new PipelineResult(parseResult, input, null);
        validationSubsystem.Execute(pipelineResult);
        return pipelineResult;
    }

    [Fact]
    public void Int_values_in_specified_range_do_not_have_errors()
    {
        var pipelineResult = ExecutedPipelineResultForRange(0, 50,"--intOpt 42");

        pipelineResult.Should().NotBeNull();
        pipelineResult.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void Int_values_not_in_specified_range_report_error()
    {
        var pipelineResult = ExecutedPipelineResultForRange(0, 5, "--intOpt 42");

        pipelineResult.Should().NotBeNull();
        pipelineResult.GetErrors().Should().HaveCount(1);
        var error = pipelineResult.GetErrors().First();
        // TODO: Create test mechanism for CliDiagnostics
    }

    [Fact]
    public void Int_values_on_lower_range_bound_do_not_report_error()
    {
        var pipelineResult = ExecutedPipelineResultForRange(42, 50, "--intOpt 42");

        pipelineResult.Should().NotBeNull();
        pipelineResult.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void Int_values_on_upper_range_bound_do_not_report_error()
    {
        var pipelineResult = ExecutedPipelineResultForRange(0, 42, "--intOpt 42");

        pipelineResult.Should().NotBeNull();
        pipelineResult.GetErrors().Should().BeEmpty();
    }


}
