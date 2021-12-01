// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.IO;
using FluentAssertions;
using Xunit;
using static System.Environment;

namespace System.CommandLine.Tests.Help
{
    public partial class HelpBuilderTests
    {
        public class Customization
        {
            private readonly HelpBuilder _helpBuilder;
            private readonly StringWriter _console;
            private readonly string _columnPadding;
            private readonly string _indentation;

            public Customization()
            {
                _console = new();
                _helpBuilder = GetHelpBuilder(LargeMaxWidth);
                _columnPadding = new string(' ', ColumnGutterWidth);
                _indentation = new string(' ', IndentationWidth);
            }

            private HelpBuilder GetHelpBuilder(int maxWidth) =>
                new(LocalizationResources.Instance,
                    maxWidth);

            [Fact]
            public void Option_can_customize_default_value()
            {
                var option = new Option<string>("--the-option", getDefaultValue: () => "not 42");
                var command = new Command("the-command", "command help")
                {
                    option
                };

                _helpBuilder.Customize(option, defaultValue: "42");

                _helpBuilder.Write(command, _console);
                var expected =
                    $"Options:{NewLine}" +
                    $"{_indentation}--the-option <the-option>{_columnPadding}[default: 42]{NewLine}{NewLine}";

                _console.ToString().Should().Contain(expected);
            }

            [Fact]
            public void Option_can_customize_first_column_text()
            {
                var option = new Option<string>("--the-option", "option description");
                var command = new Command("the-command", "command help")
                {
                    option
                };

                _helpBuilder.Customize(option, firstColumnText: "other-name");

                _helpBuilder.Write(command, _console);
                var expected =
                    $"Options:{NewLine}" +
                    $"{_indentation}other-name{_columnPadding}option description{NewLine}{NewLine}";

                _console.ToString().Should().Contain(expected);
            }

            [Fact]
            public void Option_can_customize_left_column_text_based_on_parse_result()
            {
                var option = new Option<bool>("option");
                var commandA = new Command("a", "a command help")
                {
                    option
                };
                var commandB = new Command("b", "b command help")
                {
                    option
                };
                var command = new Command("root", "root command help")
                {
                    commandA, commandB
                };
                var optionADescription = "option a help";
                var optionBDescription = "option b help";

                var helpBuilder = new HelpBuilder(LocalizationResources.Instance, LargeMaxWidth);
                helpBuilder.Customize(option, secondColumnText: parseResult =>
                                          parseResult.CommandResult.Command.Equals(commandA)
                                              ? optionADescription
                                              : optionBDescription);

                var parser = new CommandLineBuilder(command)
                             .UseDefaults()
                             .UseHelpBuilder(_ => helpBuilder)
                             .Build();

                var console = new TestConsole();
                parser.Invoke("root a -h", console);
                console.Out.ToString().Should().Contain($"option          {optionADescription}");

                console = new TestConsole();
                parser.Invoke("root b -h", console);
                console.Out.ToString().Should().Contain($"option          {optionBDescription}");
            }

            [Fact]
            public void Option_can_customize_first_column_text_based_on_parse_result()
            {
                var option = new Option<bool>("option");
                var commandA = new Command("a", "a command help")
                {
                    option
                };
                var commandB = new Command("b", "b command help")
                {
                    option
                };
                var command = new Command("root", "root command help")
                {
                    commandA, commandB
                };
                var optionAFirstColumnText = "option a help";
                var optionBFirstColumnText = "option b help";

                var helpBuilder = new HelpBuilder(LocalizationResources.Instance, LargeMaxWidth);
                helpBuilder.Customize(option, firstColumnText: parseResult =>
                                          parseResult.CommandResult.Command.Equals(commandA) 
                                              ? optionAFirstColumnText
                                              : optionBFirstColumnText);
                var parser = new CommandLineBuilder(command)
                             .UseDefaults()
                             .UseHelpBuilder(_ => helpBuilder)
                             .Build();

                var console = new TestConsole();
                parser.Invoke("root a -h", console);
                console.Out.ToString().Should().Contain(optionAFirstColumnText);

                console = new TestConsole();
                parser.Invoke("root b -h", console);
                console.Out.ToString().Should().Contain(optionBFirstColumnText);
            }

            [Fact]
            public void Subcommand_can_customize_first_column_text()
            {
                var subcommand = new Command("subcommand", "subcommand description");
                var command = new Command("the-command", "command help")
                {
                    subcommand
                };

                _helpBuilder.Customize(subcommand, firstColumnText: "other-name");

                _helpBuilder.Write(command, _console);
                var expected =
                    $"Commands:{NewLine}" +
                    $"{_indentation}other-name{_columnPadding}subcommand description{NewLine}{NewLine}";

                _console.ToString().Should().Contain(expected);
            }

            [Fact]
            public void Command_arguments_can_customize_default_value()
            {
                var argument = new Argument<string>("some-arg", getDefaultValue: () => "not 42");
                var command = new Command("the-command", "command help")
                {
                    argument
                };

                _helpBuilder.Customize(argument, defaultValue: "42");

                _helpBuilder.Write(command, _console);
                var expected =
                    $"Arguments:{NewLine}" +
                    $"{_indentation}<some-arg>{_columnPadding}[default: 42]{NewLine}{NewLine}";

                _console.ToString().Should().Contain(expected);
            }

            [Fact]
            public void Command_arguments_can_customize_second_column_text()
            {
                var argument = new Argument<string>("some-arg", getDefaultValue: () => "not 42");
                var command = new Command("the-command", "command help")
                {
                    argument
                };

                _helpBuilder.Customize(argument, firstColumnText: "some-other-arg");

                _helpBuilder.Write(command, _console);
                var expected =
                    $"Arguments:{NewLine}" +
                    $"{_indentation}some-other-arg{_columnPadding}[default: not 42]{NewLine}{NewLine}";

                _console.ToString().Should().Contain(expected);
            }

            [Fact]
            public void Customize_throws_when_symbol_is_null()
            {
                Action action = () => new HelpBuilder(LocalizationResources.Instance).Customize(null!, "");
                action.Should().Throw<ArgumentNullException>();
            }
        }
        
        private class CustomLocalizationResources : LocalizationResources
        {
            public string OverrideHelpDescriptionTitle { get; set; }

            public override string HelpDescriptionTitle()
            {
                return OverrideHelpDescriptionTitle ?? base.HelpDescriptionTitle();
            }
        }
    }
}