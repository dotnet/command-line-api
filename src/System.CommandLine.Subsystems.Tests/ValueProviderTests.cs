// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.CommandLine.Directives;
using System.CommandLine.Parsing;
using System.Runtime.Serialization;
using Xunit;
using static System.CommandLine.Subsystems.Tests.TestData;

namespace System.CommandLine.Subsystems.Tests;

public class ValueProviderTests
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

    [Fact]
    public void Default_value_is_used_when_user_did_not_enter_a_value()
    {
        var intOption = new CliOption<int>("--intOption");
        intOption.SetDefault<int>(42);
        var rootCommand = new CliRootCommand { intOption };
        var configuration = new CliConfiguration(rootCommand);
        var pipeline = Pipeline.Create();
        var input = "";

        var parseResult = CliParser.Parse(rootCommand, input, configuration);
        var pipelineResult = new PipelineResult(parseResult, input, pipeline);

        pipelineResult.Should().NotBeNull();
        var intOptionValue = pipelineResult.GetValue<int>(intOption);
        intOptionValue.Should().Be(42);
    }

    [Fact]
    public void Can_retrieve_calculated_value_()
    {
        // TODO: This is problematic because we probably can't and don't want to add new types
        // of ValueSymbols. We need a different way to add these, which means we need to work
        // out where they live (probably as annotations on the root command or a special symbol
        // type) and how we add them (probably an extension method).
        var calcSymbol = new CalculatedValue<int>("calcValue", 42);
        var rootCommand = new CliRootCommand { calcSymbol };
        var configuration = new CliConfiguration(rootCommand);
        var pipeline = Pipeline.Create();
        var input = "";

        var parseResult = CliParser.Parse(rootCommand, input, configuration);
        var pipelineResult = new PipelineResult(parseResult, input, pipeline);

        pipelineResult.Should().NotBeNull();
        var intOptionValue = pipelineResult.GetValue<int>(calcSymbol);
        intOptionValue.Should().Be(42);
    }

    [Fact]
    public void Circular_default_value_dependency_throw()
    {
        var opt1 = new CliOption<int>("opt1");
        var opt2 = new CliOption<int>("opt2");
        var opt3 = new CliOption<int>("opt3");
        opt1.SetDefault<int>(opt2);
        opt2.SetDefault<int>(opt3);
        opt3.SetDefault<int>(opt1);
        var rootCommand = new CliRootCommand { opt1, opt2, opt3 };
        var configuration = new CliConfiguration(rootCommand);
        var pipeline = Pipeline.Create();
        var input = "";

        var parseResult = CliParser.Parse(rootCommand, input, configuration);
        var pipelineResult = new PipelineResult(parseResult, input, pipeline);

        pipelineResult.Should().NotBeNull();
        Assert.Throws<InvalidOperationException>(() => _ = pipelineResult.GetValue<int>(opt1));
    }
}
