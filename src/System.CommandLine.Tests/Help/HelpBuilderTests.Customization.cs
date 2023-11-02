// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Help;
using System.IO;
using System.Linq;
using FluentAssertions;
using Xunit;
using static System.Environment;

namespace System.CommandLine.Tests.Help;

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

        private HelpBuilder GetHelpBuilder(int maxWidth) => new (maxWidth);

        [Fact]
        public void Option_can_customize_displayed_default_value()
        {
            var option = new CliOption<string>("--the-option") { DefaultValueFactory = _ => "not 42" };
            var command = new CliCommand("the-command", "command help")
            {
                option
            };

            _helpBuilder.CustomizeSymbol(option, defaultValue: "42");

            _helpBuilder.Write(command, _console);
            var expected =
                $"Options:{NewLine}" +
                $"{_indentation}--the-option{_columnPadding}[default: 42]{NewLine}{NewLine}";

            _console.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Option_can_customize_first_column_text()
        {
            var option = new CliOption<string>("--the-option") { Description = "option description" };
            var command = new CliCommand("the-command", "command help")
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
            var option = new CliOption<bool>("option");
            var commandA = new CliCommand("a", "a command help")
            {
                option
            };
            var commandB = new CliCommand("b", "b command help")
            {
                option
            };
            var command = new CliCommand("root", "root command help")
            {
                commandA, commandB
            };
            var optionAFirstColumnText = "option a help";
            var optionBFirstColumnText = "option b help";

            var helpBuilder = new HelpBuilder(LargeMaxWidth);
            helpBuilder.CustomizeSymbol(option, firstColumnText: ctx =>
                                            ctx.Command.Equals(commandA) 
                                                ? optionAFirstColumnText
                                                : optionBFirstColumnText);
            command.Options.Add(new HelpOption()
            {
                Action = new HelpAction()
                {
                    Builder = helpBuilder
                }
            });

            var console = new StringWriter();
            var config = new CliConfiguration(command)
            {
                Output = console
            };
            command.Parse("root a -h", config).Invoke();
            console.ToString().Should().Contain(optionAFirstColumnText);

            console = new StringWriter();
            config.Output = console;
            command.Parse("root b -h", config).Invoke();
            console.ToString().Should().Contain(optionBFirstColumnText);
        }

        [Fact]
        public void Option_can_customize_second_column_text_based_on_parse_result()
        {
            var option = new CliOption<bool>("option");
            var commandA = new CliCommand("a", "a command help")
            {
                option
            };
            var commandB = new CliCommand("b", "b command help")
            {
                option
            };
            var command = new CliCommand("root", "root command help")
            {
                commandA, commandB
            };
            var optionADescription = "option a help";
            var optionBDescription = "option b help";

            var helpBuilder = new HelpBuilder(LargeMaxWidth);
            helpBuilder.CustomizeSymbol(option, secondColumnText: ctx =>
                                            ctx.Command.Equals(commandA)
                                                ? optionADescription
                                                : optionBDescription);
            command.Options.Add(new HelpOption
            {
                Action = new HelpAction
                { 
                    Builder = helpBuilder
                }
            });

            var config = new CliConfiguration(command)
            {
                Output = new StringWriter()
            };

            config.Invoke("root a -h");
            config.Output.ToString().Should().Contain($"option          {optionADescription}");

            config.Output = new StringWriter();
            config.Invoke("root b -h");
            config.Output.ToString().Should().Contain($"option          {optionBDescription}");
        }

        [Fact]
        public void Subcommand_can_customize_first_column_text()
        {
            var subcommand = new CliCommand("subcommand", "subcommand description");
            var command = new CliCommand("the-command", "command help")
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
            var argument = new CliArgument<string>("arg-name") { Description = "arg description" };
            var command = new CliCommand("the-command", "command help")
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
            var argument = new CliArgument<string>("some-arg")
            {
                Description = "Default description",
                DefaultValueFactory = _ => "not 42"
            };
            var command = new CliCommand("the-command", "command help")
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
            var argument = new CliArgument<string>("some-arg")
            {
                DefaultValueFactory = (_) => "not 42"
            };
            var command = new CliCommand("the-command", "command help")
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
            Action action = () => new HelpBuilder().CustomizeSymbol(null!, "");
            action.Should().Throw<ArgumentNullException>();
        }


        [Theory]
        [InlineData(false, false, "--option \\s*description")]
        [InlineData(true, false, "custom 1st\\s*description")]
        [InlineData(false, true, "--option \\s*custom 2nd")]
        [InlineData(true, true, "custom 1st\\s*custom 2nd")]
        public void Option_can_fallback_to_default_when_customizing(bool conditionA, bool conditionB, string expected)
        {
            var command = new CliCommand("test");
            var option = new CliOption<string>("--option") { Description = "description" };

            command.Options.Add(option);

            var helpBuilder = new HelpBuilder(LargeMaxWidth);
            helpBuilder.CustomizeSymbol(option,
                                        firstColumnText: ctx => conditionA ? "custom 1st" : HelpBuilder.Default.GetOptionUsageLabel(option),
                                        secondColumnText: ctx => conditionB ? "custom 2nd" : option.Description ?? string.Empty);

            command.Options.Add(new HelpOption
            {
                Action = new HelpAction
                {
                    Builder = helpBuilder
                }
            });

            CliConfiguration config = new (command);
            var console = new StringWriter();
            config.Output = console;
            command.Parse("test -h", config).Invoke();
            console.ToString().Should().MatchRegex(expected);
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
            var command = new CliCommand("test");
            var argument = new CliArgument<string>("arg")
            {
                Description = "description",
                DefaultValueFactory = _ => "default"
            };

            command.Arguments.Add(argument);

            var helpBuilder = new HelpBuilder(LargeMaxWidth);
            helpBuilder.CustomizeSymbol(argument,
                                        firstColumnText: ctx => conditionA ? "custom 1st" : HelpBuilder.Default.GetArgumentUsageLabel(argument),
                                        secondColumnText: ctx => conditionB ? "custom 2nd" : HelpBuilder.Default.GetArgumentDescription(argument),
                                        defaultValue: ctx => conditionC ? "custom def" : HelpBuilder.Default.GetArgumentDefaultValue(argument));


            CliConfiguration config = new (command);

            command.Options.Add(new HelpOption
            {
                Action = new HelpAction
                {
                    Builder = helpBuilder
                }
            });

            config.Output = new StringWriter();
            command.Parse("test -h", config).Invoke();
            config.Output.ToString().Should().MatchRegex(expected);
        }


        [Fact]
        public void Individual_symbols_can_be_customized()
        {
            var subcommand = new CliCommand("subcommand", "The default command description");
            var option = new CliOption<int>("-x") { Description = "The default option description" };
            var argument = new CliArgument<int>("int-value") { Description = "The default argument description" };

            var rootCommand = new CliRootCommand
            {
                subcommand,
                option,
                argument,
            };

            CliConfiguration config = new(rootCommand)
            {
                Output = new StringWriter()
            };

            ParseResult parseResult = rootCommand.Parse("-h", config);

            if (parseResult.Action is HelpAction helpAction)
            {
                helpAction.Builder.CustomizeSymbol(subcommand, secondColumnText: "The custom command description");
                helpAction.Builder.CustomizeSymbol(option, secondColumnText: "The custom option description");
                helpAction.Builder.CustomizeSymbol(argument, secondColumnText: "The custom argument description");
            }

            parseResult.Invoke();

            config.Output
                  .ToString()
                  .Should()
                  .ContainAll("The custom command description",
                              "The custom option description",
                              "The custom argument description");
        }

        [Fact]
        public void Help_sections_can_be_replaced()
        {
            CliConfiguration config = new(new CliRootCommand())
            {
                Output = new StringWriter()
            };

            ParseResult parseResult = config.Parse("-h");

            if (parseResult.Action is HelpAction helpAction)
            {
                helpAction.Builder.CustomizeLayout(CustomLayout);
            }

            parseResult.Invoke();

            config.Output.ToString().Should().Be($"one{NewLine}{NewLine}two{NewLine}{NewLine}three{NewLine}{NewLine}");

            IEnumerable<Func<HelpContext, bool>> CustomLayout(HelpContext _)
            {
                yield return ctx => { ctx.Output.WriteLine("one"); return true; };
                yield return ctx => { ctx.Output.WriteLine("two"); return true; };
                yield return ctx => { ctx.Output.WriteLine("three"); return true; };
            }
        }

        [Fact]
        public void Help_sections_can_be_supplemented()
        {
            CliConfiguration config = new(new CliRootCommand("hello"))
            {
                Output = new StringWriter(),
            };

            var defaultHelp = GetDefaultHelp(config.RootCommand);

            ParseResult parseResult = config.Parse("-h");

            if (parseResult.Action is HelpAction helpAction)
            {
                helpAction.Builder.CustomizeLayout(CustomLayout);
            }

            parseResult.Invoke();

            var output = config.Output.ToString();

            var expected = $"first{NewLine}{NewLine}{defaultHelp}{NewLine}last{NewLine}{NewLine}";

            output.Should().Be(expected);

            IEnumerable<Func<HelpContext, bool>> CustomLayout(HelpContext _)
            {
                yield return ctx => { ctx.Output.WriteLine("first"); return true; };

                foreach (var section in HelpBuilder.Default.GetLayout())
                {
                    yield return section;
                }

                yield return ctx => { ctx.Output.WriteLine("last"); return true; };
            }
        }

        [Fact]
        public void Layout_can_be_composed_dynamically_based_on_context()
        {
            HelpBuilder helpBuilder = new();
            var commandWithTypicalHelp = new CliCommand("typical");
            var commandWithCustomHelp = new CliCommand("custom");
            var command = new CliRootCommand
            {
                commandWithTypicalHelp,
                commandWithCustomHelp
            };

            command.Options.OfType<HelpOption>().Single().Action = new HelpAction
            {
                Builder = helpBuilder
            };

            var config = new CliConfiguration(command);
            helpBuilder.CustomizeLayout(c =>
                                            c.Command == commandWithTypicalHelp
                                                ? HelpBuilder.Default.GetLayout()
                                                : new Func<HelpContext, bool>[] { c => { c.Output.WriteLine("Custom layout!"); return true; } }
                                                    .Concat(HelpBuilder.Default.GetLayout()));

            var typicalOutput = new StringWriter();
            config.Output = typicalOutput;
            command.Parse("typical -h", config).Invoke();

            var customOutput = new StringWriter();
            config.Output = customOutput;
            command.Parse("custom -h", config).Invoke();

            typicalOutput.ToString().Should().Be(GetDefaultHelp(commandWithTypicalHelp, false));
            customOutput.ToString().Should().Be($"Custom layout!{NewLine}{NewLine}{GetDefaultHelp(commandWithCustomHelp, false)}");
        }

        [Fact]
        public void Help_default_sections_can_be_wrapped()
        {
            CliCommand command = new("test")
            {
                new CliOption<string>("--option")
                {
                    Description = "option description",
                    HelpName = "option"
                },
                new HelpOption
                {
                    Action = new HelpAction
                    {
                        Builder = new HelpBuilder(30)
                    }
                }
            };

            CliConfiguration config = new(command)
            {
                Output = new StringWriter()
            };

            config.Invoke("test -h");

            string result = config.Output.ToString();
            result.Should().Be(
                $"Description:{NewLine}{NewLine}" +
                $"Usage:{NewLine}  test [options]{NewLine}{NewLine}" +
                $"Options:{NewLine}" +
                $"  --option   option {NewLine}" +
                $"  <option>   description{NewLine}" +
                $"  -?, -h,    Show help and {NewLine}" +
                $"  --help     usage {NewLine}" +
                $"             information{NewLine}{NewLine}");
        }

        [Fact]
        public void Help_customized_sections_can_be_wrapped()
        {
            CliConfiguration config = new(new CliRootCommand())
            {
                Output = new StringWriter()
            };

            ParseResult parseResult = config.Parse("-h");

            if (parseResult.Action is HelpAction helpAction)
            {
                helpAction.Builder = new HelpBuilder(10);
                helpAction.Builder.CustomizeLayout(CustomLayout);
            }

            parseResult.Invoke();

            string result = config.Output.ToString();
            result.Should().Be($"  123  123{NewLine}  456  456{NewLine}  78   789{NewLine}       0{NewLine}{NewLine}");

            IEnumerable<Func<HelpContext, bool>> CustomLayout(HelpContext _)
            {
                yield return ctx => { ctx.HelpBuilder.WriteColumns(new[] { new TwoColumnHelpRow("12345678", "1234567890") }, ctx); return true; };
            }
        }

        private string GetDefaultHelp(CliCommand command, bool trimOneNewline = true)
        {
            // The command might have already defined a HelpOption with custom settings,
            // we need to overwrite it to get the actual defaults.
            HelpOption defaultHelp = new();
            // HelpOption overrides Equals and treats every other instance of same type as equal
            int index = command.Options.IndexOf(defaultHelp);
            if (index >= 0)
            {
                command.Options[index] = defaultHelp;
            }
            else
            {
                command.Options.Add(defaultHelp);
            }

            CliConfiguration config = new(command)
            {
                Output = new StringWriter()
            };

            config.Invoke("-h");

            var output = config.Output.ToString();

            if (trimOneNewline)
            {
                output = output.Substring(0, output.Length - NewLine.Length);
            }

            return output;
        }
    }
}