// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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

                _helpBuilder.CustomizeSymbol(option, defaultValue: "42");

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

                _helpBuilder.CustomizeSymbol(option, firstColumnText: "other-name");

                _helpBuilder.Write(command, _console);
                var expected =
                    $"Options:{NewLine}" +
                    $"{_indentation}other-name{_columnPadding}option description{NewLine}{NewLine}";

                _console.ToString().Should().Contain(expected);
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
                helpBuilder.CustomizeSymbol(option, firstColumnText: ctx =>
                                          ctx.Command.Equals(commandA) 
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
            public void Option_can_customize_second_column_text_based_on_parse_result()
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
                helpBuilder.CustomizeSymbol(option, secondColumnText: ctx =>
                                          ctx.Command.Equals(commandA)
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
            public void Subcommand_can_customize_first_column_text()
            {
                var subcommand = new Command("subcommand", "subcommand description");
                var command = new Command("the-command", "command help")
                {
                    subcommand
                };

                _helpBuilder.CustomizeSymbol(subcommand, firstColumnText: "other-name");

                _helpBuilder.Write(command, _console);
                var expected =
                    $"Commands:{NewLine}" +
                    $"{_indentation}other-name{_columnPadding}subcommand description{NewLine}{NewLine}";

                _console.ToString().Should().Contain(expected);
            }

            [Fact]
            public void Command_arguments_can_customize_first_column_text()
            {
                var argument = new Argument<string>("arg-name", "arg description");
                var command = new Command("the-command", "command help")
                {
                    argument
                };

                _helpBuilder.CustomizeSymbol(argument, firstColumnText: "<CUSTOM-ARG-NAME>");

                _helpBuilder.Write(command, _console);
                var expected =
                    $"Arguments:{NewLine}" +
                    $"{_indentation}<CUSTOM-ARG-NAME>{_columnPadding}arg description{NewLine}{NewLine}";

                _console.ToString().Should().Contain(expected);
            }

            [Fact]
            public void Command_arguments_can_customize_second_column_text()
            {
                var argument = new Argument<string>("some-arg", description: "Default description", getDefaultValue: () => "not 42");
                var command = new Command("the-command", "command help")
                {
                    argument
                };

                _helpBuilder.CustomizeSymbol(argument, secondColumnText: "Custom description");

                _helpBuilder.Write(command, _console);
                var expected =
                    $"Arguments:{NewLine}" +
                    $"{_indentation}<some-arg>{_columnPadding}Custom description [default: not 42]{NewLine}{NewLine}";

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

                _helpBuilder.CustomizeSymbol(argument, defaultValue: "42");

                _helpBuilder.Write(command, _console);
                var expected =
                    $"Arguments:{NewLine}" +
                    $"{_indentation}<some-arg>{_columnPadding}[default: 42]{NewLine}{NewLine}";

                _console.ToString().Should().Contain(expected);
            }

            [Fact]
            public void Customize_throws_when_symbol_is_null()
            {
                Action action = () => new HelpBuilder(LocalizationResources.Instance).CustomizeSymbol(null!, "");
                action.Should().Throw<ArgumentNullException>();
            }


            [Theory]
            [InlineData(false, false, "--option <option>\\s*description")]
            [InlineData(true, false, "custom 1st\\s*description")]
            [InlineData(false, true, "--option <option>\\s*custom 2nd")]
            [InlineData(true, true, "custom 1st\\s*custom 2nd")]
            public void Option_can_fallback_to_default_when_customizing(bool conditionA, bool conditionB, string expected)
            {
                var command = new Command("test");
                var option = new Option<string>("--option", "description");

                command.AddOption(option);

                var helpBuilder = new HelpBuilder(LocalizationResources.Instance, LargeMaxWidth);
                helpBuilder.CustomizeSymbol(option,
                    firstColumnText: ctx => conditionA ? "custom 1st" : HelpBuilder.Default.GetIdentifierSymbolUsageLabel(option, ctx),
                    secondColumnText: ctx => conditionB ? "custom 2nd" : HelpBuilder.Default.GetIdentifierSymbolDescription(option));


                var parser = new CommandLineBuilder(command)
                             .UseDefaults()
                             .UseHelpBuilder(_ => helpBuilder)
                             .Build();

                var console = new TestConsole();
                parser.Invoke("test -h", console);
                console.Out.ToString().Should().MatchRegex(expected);
            }

            [Theory]
            [InlineData(false, false, false, "\\<arg\\>\\s*description\\s*\\[default\\: default\\]")]
            [InlineData(true, false, false, "custom 1st\\s*description\\s*\\[default\\: default\\]")]
            [InlineData(false, true, false, "\\<arg\\>\\s*custom 2nd\\s*\\[default\\: default\\]")]
            [InlineData(true, true, false, "custom 1st\\s*custom 2nd\\s*\\[default\\: default\\]")]
            [InlineData(false, false, true, "\\<arg\\>\\s*description\\s*\\[default\\: custom def\\]")]
            [InlineData(true, false, true, "custom 1st\\s*description\\s*\\[default\\: custom def\\]")]
            [InlineData(false, true, true, "\\<arg\\>\\s*custom 2nd\\s*\\[default\\: custom def\\]")]
            [InlineData(true, true, true, "custom 1st\\s*custom 2nd\\s*\\[default\\: custom def\\]")]
            public void Argument_can_fallback_to_default_when_customizing(
                bool conditionA, 
                bool conditionB, 
                bool conditionC, 
                string expected)
            {
                var command = new Command("test");
                var argument = new Argument<string>("arg", "description");
                argument.SetDefaultValue("default");

                command.AddArgument(argument);

                var helpBuilder = new HelpBuilder(LocalizationResources.Instance, LargeMaxWidth);
                helpBuilder.CustomizeSymbol(argument,
                    firstColumnText: ctx => conditionA ? "custom 1st" : HelpBuilder.Default.GetArgumentUsageLabel(argument),
                    secondColumnText: ctx => conditionB ? "custom 2nd" : HelpBuilder.Default.GetArgumentDescription(argument),
                    defaultValue: ctx => conditionC ? "custom def" : HelpBuilder.Default.GetArgumentDefaultValue(argument));


                var parser = new CommandLineBuilder(command)
                             .UseDefaults()
                             .UseHelpBuilder(_ => helpBuilder)
                             .Build();

                var console = new TestConsole();
                parser.Invoke("test -h", console);
                console.Out.ToString().Should().MatchRegex(expected);
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