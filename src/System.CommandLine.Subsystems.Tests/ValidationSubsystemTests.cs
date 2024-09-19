// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.CommandLine.Parsing;
using System.CommandLine.ValueSources;
using Xunit;

namespace System.CommandLine.Subsystems.Tests;

public class ValidationSubsystemTests
{
    // Running exactly the same code is important here because missing a step will result in a false positive. Ask me how I know
    private CliOption<T> GetOptionWithSimpleRange<T>(string name, T lowerBound, T upperBound)
        where T : IComparable<T>
    {
        var option = new CliOption<T>(name);
        option.SetRange(lowerBound, upperBound);
        return option;
    }

    private CliOption<T> GetOptionWithRangeBounds<T>(string name, ValueSource<T> lowerBound, ValueSource<T> upperBound)
        where T : IComparable<T>
    {
        var option = new CliOption<T>(name);
        option.SetRange(lowerBound, upperBound);
        return option;
    }

    private PipelineResult ExecutedPipelineResultForRangeOption(CliOption option, string input)
    {
        var command = new CliRootCommand { option };
        return ExecutedPipelineResultForCommand(command, input);
    }

    private PipelineResult ExecutedPipelineResultForCommand(CliCommand command, string input)
    {
        var validationSubsystem = ValidationSubsystem.Create();
        var parseResult = CliParser.Parse(command, input, new CliConfiguration(command));
        var pipelineResult = new PipelineResult(parseResult, input, Pipeline.CreateEmpty());
        validationSubsystem.Execute(pipelineResult);
        return pipelineResult;
    }

    [Fact]
    public void Int_values_in_specified_range_do_not_have_errors()
    {
        var option = GetOptionWithSimpleRange("--intOpt", 0, 50);

        var pipelineResult = ExecutedPipelineResultForRangeOption(option, "--intOpt 42");

        pipelineResult.Should().NotBeNull();
        pipelineResult.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void Int_values_above_upper_bound_report_error()
    {
        var option = GetOptionWithSimpleRange("--intOpt", 0, 5);

        var pipelineResult = ExecutedPipelineResultForRangeOption(option, "--intOpt 42");

        pipelineResult.Should().NotBeNull();
        pipelineResult.GetErrors().Should().HaveCount(1);
        var error = pipelineResult.GetErrors().First();
        // TODO: Create test mechanism for CliDiagnostics
    }

    [Fact]
    public void Int_below_lower_bound_report_error()
    {
        var option = GetOptionWithSimpleRange("--intOpt", 0, 5);

        var pipelineResult = ExecutedPipelineResultForRangeOption(option, "--intOpt -42");

        pipelineResult.Should().NotBeNull();
        pipelineResult.GetErrors().Should().HaveCount(1);
        var error = pipelineResult.GetErrors().First();
        // TODO: Create test mechanism for CliDiagnostics
    }

    [Fact]
    public void Int_values_on_lower_range_bound_do_not_report_error()
    {
        var option = GetOptionWithSimpleRange("--intOpt", 42, 50);

        var pipelineResult = ExecutedPipelineResultForRangeOption(option, "--intOpt 42");

        pipelineResult.Should().NotBeNull();
        pipelineResult.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void Int_values_on_upper_range_bound_do_not_report_error()
    {
        var option = GetOptionWithSimpleRange("--intOpt", 0, 42);

        var pipelineResult = ExecutedPipelineResultForRangeOption(option, "--intOpt 42");

        pipelineResult.Should().NotBeNull();
        pipelineResult.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void Values_below_calculated_lower_bound_report_error()
    {
        var option = GetOptionWithRangeBounds<int>("--intOpt", ValueSource.Create(() => (true, 1)), 50);

        var pipelineResult = ExecutedPipelineResultForRangeOption(option, "--intOpt 0");

        pipelineResult.Should().NotBeNull();
        pipelineResult.GetErrors().Should().HaveCount(1);
        var error = pipelineResult.GetErrors().First();
        // TODO: Create test mechanism for CliDiagnostics
    }


    [Fact]
    public void Values_within_calculated_range_do_not_report_error()
    {
        var option = GetOptionWithRangeBounds("--intOpt", ValueSource.Create(() => (true, 1)), ValueSource.Create(() => (true, 50)));

        var pipelineResult = ExecutedPipelineResultForRangeOption(option, "--intOpt 42");

        pipelineResult.Should().NotBeNull();
        pipelineResult.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void Values_above_calculated_upper_bound_report_error()
    {
        var option = GetOptionWithRangeBounds("--intOpt", 0, ValueSource.Create(() => (true, 40)));

        var pipelineResult = ExecutedPipelineResultForRangeOption(option, "--intOpt 42");

        pipelineResult.Should().NotBeNull();
        pipelineResult.GetErrors().Should().HaveCount(1);
        var error = pipelineResult.GetErrors().First();
        // TODO: Create test mechanism for CliDiagnostics
    }

    [Fact]
    public void Values_below_relative_lower_bound_report_error()
    {
        var otherOption = new CliOption<int>("-a");
        var option = GetOptionWithRangeBounds("--intOpt", ValueSource.Create(otherOption, o => (true, (int)o + 1)), 50);
        var command = new CliCommand("cmd") { option, otherOption };

        var pipelineResult = ExecutedPipelineResultForCommand(command, "--intOpt 0 -a 0");

        pipelineResult.Should().NotBeNull();
        pipelineResult.GetErrors().Should().HaveCount(1);
        var error = pipelineResult.GetErrors().First();
        // TODO: Create test mechanism for CliDiagnostics
    }


    [Fact]
    public void Values_within_relative_range_do_not_report_error()
    {
        var otherOption = new CliOption<int>("-a");
        var option = GetOptionWithRangeBounds("--intOpt", ValueSource.Create(otherOption, o => (true, (int)o + 1)), ValueSource.Create(otherOption, o => (true, (int)o + 10)));
        var command = new CliCommand("cmd") { option, otherOption };

        var pipelineResult = ExecutedPipelineResultForCommand(command, "--intOpt 11 -a 3");

        pipelineResult.Should().NotBeNull();
        pipelineResult.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void Values_above_relative_upper_bound_report_error()
    {
        var otherOption = new CliOption<int>("-a");
        var option = GetOptionWithRangeBounds("--intOpt", 0, ValueSource.Create(otherOption, o => (true, (int)o + 10)));
        var command = new CliCommand("cmd") { option, otherOption };

        var pipelineResult = ExecutedPipelineResultForCommand(command, "--intOpt 9 -a -2");

        pipelineResult.Should().NotBeNull();
        pipelineResult.GetErrors().Should().HaveCount(1);
        var error = pipelineResult.GetErrors().First();
        // TODO: Create test mechanism for CliDiagnostics
    }


}
