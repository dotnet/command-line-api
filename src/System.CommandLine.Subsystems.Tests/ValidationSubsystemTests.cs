﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.CommandLine.Validation;
using Xunit;

namespace System.CommandLine.Subsystems.Tests;

public class ValidationSubsystemTests
{
    private CliRootCommand IntegerRangeOptionRootCommand
    {
        get
        {
            CliOption<int> cliOption = new CliOption<int>("--int-opt1");
            cliOption.SetRange(1, 4);
            CliRootCommand rootCommand = [
            new CliCommand("x")
            {
                cliOption
            }];
            return rootCommand;
        }
    }

    private CliRootCommand InclusiveGroupRootCommand
    {
        get
        {
            CliOption<int> opt1 = new("--opt1");
            CliOption<string> opt2 = new ("--opt2");
            CliArgument<int> arg1 = new("arg1");
            CliArgument<string> arg2 = new ("arg2");
            var cmd = new CliCommand("x") { opt1, opt2, arg1, arg2} ;
            CliRootCommand rootCommand = [cmd];
            cmd.SetInclusiveGroup([opt2, arg1, arg2]);

            return rootCommand;
        }
    }

    private CliRootCommand IntegerRangeArgumentRootCommand
    {
        get
        {
            CliArgument<int> cliArgument = new CliArgument<int>("arg1");
            cliArgument.SetRange(1, 4);
            CliRootCommand rootCommand = [
            new CliCommand("x")
            {
                cliArgument
            }];
            return rootCommand;
        }
    }

    [Fact]
    public void ValidationSubsystem_is_included_by_default()
    {
        var subsystem = new ValidationSubsystem();
        var isActive = Subsystem.GetIsActivated(subsystem, null);

        isActive.Should().BeTrue();
    }

    [Fact]
    public void Range_validation_succeeds_when_option_value_in_range()
    {
        var rootCommand = IntegerRangeOptionRootCommand;
        var configuration = new CliConfiguration(rootCommand);
        var input = "x --int-opt1 3";

        var pipeline = Pipeline.CreateEmpty();
        var result = pipeline.Execute(configuration, input);

        result.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void Range_validation_succeeds_when_argument_value_in_range()
    {
        var rootCommand = IntegerRangeArgumentRootCommand;
        var configuration = new CliConfiguration(rootCommand);
        var input = "x 3";

        var pipeline = Pipeline.CreateEmpty();
        var result = pipeline.Execute(configuration, input);

        result.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void Range_validation_fails_when_option_value_not_in_range()
    {
        var rootCommand = IntegerRangeOptionRootCommand;
        var configuration = new CliConfiguration(rootCommand);
        var input = "x --int-opt1 42";

        var pipeline = Pipeline.CreateEmpty();
        var result = pipeline.Execute(configuration, input);

        result.GetErrors().Count().Should().Be(1);
        var rangeError = result.GetErrors().Single();
        rangeError.Message.Should().Be("The value for '--int-opt1' is above the upper bound of 4");
    }

    [Fact]
    public void Range_validation_fails_when_argument_value_is_not_in_range()
    {
        var rootCommand = IntegerRangeArgumentRootCommand;
        var configuration = new CliConfiguration(rootCommand);
        var input = "x 42";

        var pipeline = Pipeline.CreateEmpty();
        var result = pipeline.Execute(configuration, input);

        result.GetErrors().Count().Should().Be(1);
        var rangeError = result.GetErrors().Single();
        rangeError.Message.Should().Be("The value for 'arg1' is above the upper bound of 4");
    }

    [Fact]
    public void InclusiveGroup_validation_succeeds_when_only_all_members_are_present()
    {
        var rootCommand = InclusiveGroupRootCommand;
        var configuration = new CliConfiguration(rootCommand);
        var input = "x kirk spock --opt2 ";

        var pipeline = Pipeline.CreateEmpty();
        var result = pipeline.Execute(configuration, input);

        result.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void InclusiveGroup_validation_succeeds_when_all_members_and_others_are_present()
    {
        var rootCommand = InclusiveGroupRootCommand;
        var configuration = new CliConfiguration(rootCommand);
        var input = "x 1 spock --opt2 3 --opt1";

        var pipeline = Pipeline.CreateEmpty();
        var result = pipeline.Execute(configuration, input);

        result.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void InclusiveGroup_validation_succeeds_when_all_members_are_missing()
    {
        var rootCommand = InclusiveGroupRootCommand;
        var configuration = new CliConfiguration(rootCommand);
        var input = "x --opt1";

        var pipeline = Pipeline.CreateEmpty();
        var result = pipeline.Execute(configuration, input);

        result.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void InclusiveGroup_validation_fails_when_arg_member_is_missing()
    {
        var rootCommand = InclusiveGroupRootCommand;
        var configuration = new CliConfiguration(rootCommand);
        var input = "x kirk --opt2 ";

        var pipeline = Pipeline.CreateEmpty();
        var result = pipeline.Execute(configuration, input);

        result.GetErrors().Count().Should().Be(1);
        var rangeError = result.GetErrors().Single();
        // TODO: Use diagnostic ID
        rangeError.Message.Should().Be("The members --opt2, arg1, arg2 must all be used if one is used. arg2 is missing.");
    }

    [Fact]
    public void InclusiveGroup_validation_fails_when_option_member_is_missing()
    {
        var rootCommand = InclusiveGroupRootCommand;
        var configuration = new CliConfiguration(rootCommand);
        var input = "x kirk spock --opt1 ";

        var pipeline = Pipeline.CreateEmpty();
        var result = pipeline.Execute(configuration, input);

        result.GetErrors().Count().Should().Be(1);
        var rangeError = result.GetErrors().Single();
        rangeError.Message.Should().Be("The members --opt2, arg1, arg2 must all be used if one is used. --opt2 is missing.");
    }

    [Fact(Skip = "Not yet written")]
    public void Range_bound_must_be_same_type_as_option()
    { }
}
