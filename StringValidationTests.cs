// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using FluentAssertions;
using System.CommandLine.Parsing;
using System.CommandLine.ValueSources;
using Xunit;

namespace System.CommandLine.Subsystems.Tests;

public class StringValidationTests
{
    private PipelineResult ExecutedPipelineResultForCommand(CliCommand command, string input)
    {
        var validationSubsystem = ValidationSubsystem.Create();
        var parseResult = CliParser.Parse(command, input, new CliConfiguration(command));
        var pipelineResult = new PipelineResult(parseResult, input, Pipeline.CreateEmpty());
        validationSubsystem.Execute(pipelineResult);
        return pipelineResult;
    }

    [Fact]
    public void StringValidationShouldPassWhenInputInLowerCase()
    {
        var option = new CliOption<string>("--opt");
        option.SetCasing("lower");
        var command = new CliCommand("cmd");
        command.Options.Add(option);

        var input = "--opt letuswin";

        var pipelineResult = ExecutedPipelineResultForCommand(command, input);

        pipelineResult.Should().NotBeNull();
        pipelineResult.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void StringValidationShouldFailWhenInputNotInLowerCase()
    {
        var option = new CliOption<string>("--opt");
        option.SetCasing("lower");
        var command = new CliCommand("cmd");
        command.Options.Add(option);

        var input = "--opt LetUsWin";

        var pipelineResult = ExecutedPipelineResultForCommand(command, input);

        pipelineResult.Should().NotBeNull();
        pipelineResult.GetErrors().Should().HaveCount(1);
    }

    [Fact]
    public void StringValidationShouldFailWhenInputNotInUpperCase()
    {
        var option = new CliOption<string>("--opt");
        option.SetCasing("lower");
        var command = new CliCommand("cmd");
        command.Options.Add(option);

        var input = "--opt LetUsWin";

        var pipelineResult = ExecutedPipelineResultForCommand(command, input);

        pipelineResult.Should().NotBeNull();
        pipelineResult.GetErrors().Should().HaveCount(1);
    }

    [Fact]
    public void StringValidationShouldPassWhenInputInUpperCase()
    {
        var option = new CliOption<string>("--opt");
        option.SetCasing("upper");
        var command = new CliCommand("cmd");
        command.Options.Add(option);

        var input = "--opt GOODWORK!";

        var pipelineResult = ExecutedPipelineResultForCommand(command, input);

        pipelineResult.Should().NotBeNull();
        pipelineResult.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void StringValidationShouldThrowExceptionWhenInputIsEmptyAndCasingIsSet()
    {
        var option = new CliOption<string>("--opt");
        option.SetCasing("lower");
        var command = new CliCommand("cmd");
        command.Options.Add(option);

        var input = "--opt ";

        Assert.Throws<InvalidOperationException>(() => ExecutedPipelineResultForCommand(command, input));
    }

    [Fact]
    public void StringValidationShouldPassWhenInputIsAllNumericWithLower()
    {
        var option = new CliOption<string>("--opt");
        option.SetCasing("lower");
        var command = new CliCommand("cmd");
        command.Options.Add(option);

        var input = "--opt fuzzy123";

        var pipelineResult = ExecutedPipelineResultForCommand(command, input);

        pipelineResult.Should().NotBeNull();
        pipelineResult.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void StringValidationShouldPassWhenInputIsAllNumericWithUpper()
    {
        var option = new CliOption<string>("--opt");
        option.SetCasing("upper");
        var command = new CliCommand("cmd");
        command.Options.Add(option);

        var input = "--opt FUZZY123";

        var pipelineResult = ExecutedPipelineResultForCommand(command, input);

        pipelineResult.Should().NotBeNull();
        pipelineResult.GetErrors().Should().BeEmpty();
    }
}
