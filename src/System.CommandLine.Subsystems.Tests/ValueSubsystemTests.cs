// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.CommandLine.Directives;
using System.CommandLine.Parsing;
using Xunit;
using static System.CommandLine.Subsystems.Tests.TestData;

namespace System.CommandLine.Subsystems.Tests;

public class ValueSubsystemTests
{
    [Fact]
    public void Values_that_are_entered_are_retrieved()
    {
        var option = new CliOption<int>("--intOpt");
        var rootCommand = new CliRootCommand { option };
        var configuration = new CliConfiguration(rootCommand);
        var pipeline = Pipeline.Create();
        var input = "--intOpt 42";

        var parseResult = CliParser.Parse(rootCommand, input, configuration);
        var pipelineResult = new PipelineResult(parseResult, input, pipeline);

        pipelineResult.Should().NotBeNull();
        var optionValueResult = pipelineResult.GetValueResult(option);
        var optionValue = pipelineResult.GetValue<int>(option);
        optionValueResult.Should().NotBeNull();
        optionValue.Should().Be(42);
    }

    [Fact]
    public void Values_that_are_not_entered_are_type_default_with_no_default_values()
    {
        var stringOption = new CliOption<string>("--stringOption");
        var intOption = new CliOption<int>("--intOption");
        var dateOption = new CliOption<DateTime>("--dateOption");
        var nullableIntOption = new CliOption<int?>("--nullableIntOption");
        var guidOption = new CliOption<Guid>("--guidOption");
        var rootCommand = new CliRootCommand { stringOption, intOption, dateOption, nullableIntOption, guidOption };
        var configuration = new CliConfiguration(rootCommand);
        var pipeline = Pipeline.Create();
        var input = "";

        var parseResult = CliParser.Parse(rootCommand, input, configuration);
        var pipelineResult = new PipelineResult(parseResult, input, pipeline);

        pipelineResult.Should().NotBeNull();
        var stringOptionValue = pipelineResult.GetValue<string>(stringOption);
        var intOptionValue = pipelineResult.GetValue<int>(intOption);
        var dateOptionValue = pipelineResult.GetValue<DateTime>(dateOption);
        var nullableIntOptionValue = pipelineResult.GetValue<int?>(nullableIntOption);
        var guidOptionValue = pipelineResult.GetValue<Guid>(guidOption);
        stringOptionValue.Should().BeNull();
        intOptionValue.Should().Be(0);
        dateOptionValue.Should().Be(DateTime.MinValue);
        nullableIntOptionValue.Should().BeNull();
        guidOptionValue.Should().Be(Guid.Empty);
    }

    // TODO: Add various default value tests

    /* Hold these tests until we determine if ValueSubsystem is replaceable
    [Fact]
    public void ValueSubsystem_returns_values_that_are_entered()
    {
        var consoleHack = new ConsoleHack().RedirectToBuffer(true);
        CliOption<int> option = new CliOption<int>("--intValue");
        CliRootCommand rootCommand = [
            new CliCommand("x")
            {
                option
            }];
        var configuration = new CliConfiguration(rootCommand);
        var pipeline = Pipeline.CreateEmpty();
        pipeline.Value = new ValueSubsystem();
        const int expected = 42;
        var input = $"x --intValue {expected}";

        var parseResult = pipeline.Parse(configuration, input); // assigned for debugging
        pipeline.Execute(configuration, input, consoleHack);

        pipeline.Value.GetValue<int>(option).Should().Be(expected);
    }

    [Fact]
    public void ValueSubsystem_returns_default_value_when_no_value_is_entered()
    {
        var consoleHack = new ConsoleHack().RedirectToBuffer(true);
        CliOption<int> option = new CliOption<int>("--intValue");
        CliRootCommand rootCommand = [option];
        var configuration = new CliConfiguration(rootCommand);
        var pipeline = Pipeline.CreateEmpty();
        pipeline.Value = new ValueSubsystem();
        option.SetDefaultValue(43);
        const int expected = 43;
        var input = $"";

        pipeline.Execute(configuration, input, consoleHack);

        pipeline.Value.GetValue<int>(option).Should().Be(expected);
    }


    [Fact]
    public void ValueSubsystem_returns_calculated_default_value_when_no_value_is_entered()
    {
        var consoleHack = new ConsoleHack().RedirectToBuffer(true);
        CliOption<int> option = new CliOption<int>("--intValue");
        CliRootCommand rootCommand = [option];
        var configuration = new CliConfiguration(rootCommand);
        var pipeline = Pipeline.CreateEmpty();
        pipeline.Value = new ValueSubsystem();
        var x = 42;
        option.SetDefaultValueCalculation(() => x + 2);
        const int expected = 44;
        var input = "";

        var parseResult = pipeline.Parse(configuration, input); // assigned for debugging
        pipeline.Execute(configuration, input, consoleHack);

        pipeline.Value.GetValue<int>(option).Should().Be(expected);
    }
    */
}
