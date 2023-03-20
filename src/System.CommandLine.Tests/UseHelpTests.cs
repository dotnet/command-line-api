// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Help;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using static System.Environment;

namespace System.CommandLine.Tests
{
    public class UseHelpTests
    {
        [Fact]
        public async Task UseHelp_writes_help_for_the_specified_command()
        {
            Command command = new RootCommand()
            {
                new HelpOption(),
                new Command("command")
                {
                    new Command("subcommand")
                }
            };

            CommandLineConfiguration config = new(command)
            {
                Out = new StringWriter()
            };

            var result = command.Parse("command subcommand --help", config);

            await result.InvokeAsync();

            config.Out.ToString().Should().Contain($"{RootCommand.ExecutableName} command subcommand [options]");
        }
         
        [Fact]
        public async Task UseHelp_interrupts_execution_of_the_specified_command()
        {
            var wasCalled = false;
            var command = new Command("command") { new HelpOption() };
            var subcommand = new Command("subcommand");
            subcommand.SetAction((_) => wasCalled = true);
            command.Subcommands.Add(subcommand);

            CommandLineConfiguration config = new(command)
            {
                Out = new StringWriter()
            };

            int result = await command.Parse("command subcommand --help", config).InvokeAsync();

            wasCalled.Should().BeFalse();
        }

        [Theory]
        [InlineData("-h")]
        [InlineData("--help")]
        [InlineData("-?")]
        [InlineData("/?")]
        public async Task UseHelp_accepts_default_values(string value)
        {
            CommandLineConfiguration config = new(new Command("command") { new HelpOption() })
            {
                Out = new StringWriter()
            };

            StringWriter console = new();
            config.Out = console;

            await config.InvokeAsync($"command {value}");

            console.ToString().Should().Contain("Usage:");
        }

        [Fact]
        public async Task UseHelp_does_not_display_when_option_defined_with_same_alias()
        {
            var command = new Command("command");
            command.Options.Add(new Option<bool>("-h"));

            CommandLineConfiguration config = new(command)
            {
                Out = new StringWriter()
            };

            int result = await command.Parse("command -h", config).InvokeAsync();

            config.Out.ToString().Should().BeEmpty();
        }

        [Fact]
        public void There_are_no_parse_errors_when_help_is_invoked_on_root_command()
        {
            RootCommand rootCommand = new() { new HelpOption() };

            var result = rootCommand.Parse("-h");

            result.Errors
                  .Should()
                  .BeEmpty();
        }
        
        [Fact]
        public void There_are_no_parse_errors_when_help_is_invoked_on_subcommand()
        {
            var root = new RootCommand
            {
                new Command("subcommand"),
                new HelpOption()
            };

            var result = root.Parse("subcommand -h");

            result.Errors
                  .Should()
                  .BeEmpty();
        }

        [Fact]
        public void There_are_no_parse_errors_when_help_is_invoked_on_a_command_with_subcommands()
        {
            var root = new RootCommand
            {
                new Command("subcommand"),
                new HelpOption()
            };

            var result = root.Parse("-h");

            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void There_are_no_parse_errors_when_help_is_invoked_on_a_command_with_required_options()
        {
            var command = new RootCommand
            {
                new Option<string>("-x")
                {
                    IsRequired = true
                },
                new HelpOption()
            };

            var result = command.Parse("-h");

            result.Errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData("/lost")]
        [InlineData("--confused")]
        public async Task HelpOption_with_custom_aliases_uses_aliases(string helpAlias)
        {
            RootCommand command = new()
            {
                new HelpOption("/lost", "--confused")
            };
            CommandLineConfiguration config = new(command)
            {
                Out = new StringWriter()
            };

            await config.InvokeAsync(helpAlias);

            config.Out.ToString().Should().Contain("Usage:");
        }

        [Theory]
        [InlineData("-h")]
        [InlineData("/h")]
        [InlineData("--help")]
        [InlineData("-?")]
        [InlineData("/?")]
        public async Task UseHelp_with_custom_aliases_default_aliases_replaced(string helpAlias)
        {
            RootCommand command = new()
            {
                new HelpOption("--confused")
            };
            CommandLineConfiguration config = new(command)
            {
                Out = new StringWriter(),
                EnableParseErrorReporting = false
            };

            await config.InvokeAsync(helpAlias);

            config.Out.ToString().Should().Be("");
        }

        [Fact]
        public void Individual_symbols_can_be_customized()
        {
            var subcommand = new Command("subcommand", "The default command description");
            var option = new Option<int>("-x") { Description = "The default option description" };
            var argument = new Argument<int>("int-value") { Description = "The default argument description" };

            var rootCommand = new RootCommand
            {
                subcommand,
                option,
                argument,
                new HelpOption(),
            };

            CommandLineConfiguration config = new (rootCommand)
            {
                Out = new StringWriter()
            };

            ParseResult parseResult = rootCommand.Parse("-h", config);

            if (parseResult.Action is HelpAction helpAction)
            { 
                helpAction.Builder.CustomizeSymbol(subcommand, secondColumnText: "The custom command description");
                helpAction.Builder.CustomizeSymbol(option, secondColumnText: "The custom option description");
                helpAction.Builder.CustomizeSymbol(argument, secondColumnText: "The custom argument description");
            }

            parseResult.Invoke();

            config.Out
                   .ToString()
                   .Should()
                   .ContainAll("The custom command description",
                               "The custom option description",
                               "The custom argument description");
        }

        [Fact]
        public void Help_sections_can_be_replaced()
        {
            CommandLineConfiguration config = new(new RootCommand() { new HelpOption() })
            {
                Out = new StringWriter()
            };

            ParseResult parseResult = config.RootCommand.Parse("-h", config);

            if (parseResult.Action is HelpAction helpAction)
            {
                helpAction.Builder.CustomizeLayout(CustomLayout);
            }

            parseResult.Invoke();

            config.Out.ToString().Should().Be($"one{NewLine}{NewLine}two{NewLine}{NewLine}three{NewLine}{NewLine}{NewLine}");

            IEnumerable<Action<HelpContext>> CustomLayout(HelpContext _)
            {
                yield return ctx => ctx.Output.WriteLine("one");
                yield return ctx => ctx.Output.WriteLine("two");
                yield return ctx => ctx.Output.WriteLine("three");
            }
        }

        [Fact]
        public void Help_sections_can_be_supplemented()
        {
            CommandLineConfiguration config = new(new RootCommand("hello") { new HelpOption() })
            {
                Out = new StringWriter(),
            };

            ParseResult parseResult = config.RootCommand.Parse("-h", config);

            if (parseResult.Action is HelpAction helpAction)
            {
                helpAction.Builder.CustomizeLayout(CustomLayout);
            }

            parseResult.Invoke();

            var output = config.Out.ToString();
            var defaultHelp = GetDefaultHelp(config.RootCommand);

            var expected = $"first{NewLine}{NewLine}{defaultHelp}last{NewLine}{NewLine}";

            output.Should().Be(expected);

            IEnumerable<Action<HelpContext>> CustomLayout(HelpContext _)
            {
                yield return ctx => ctx.Output.WriteLine("first");

                foreach (var section in HelpBuilder.Default.GetLayout())
                {
                    yield return section;
                }

                yield return ctx => ctx.Output.WriteLine("last");
            }
        }

        [Fact]
        public void Layout_can_be_composed_dynamically_based_on_context()
        {
            HelpBuilder helpBuilder = new();
            var commandWithTypicalHelp = new Command("typical");
            var commandWithCustomHelp = new Command("custom");
            var command = new RootCommand
            {
                commandWithTypicalHelp,
                commandWithCustomHelp,
                new HelpOption()
                {
                    Action = new HelpAction()
                    {
                        Builder = helpBuilder
                    }
                }
            };

            var config = new CommandLineConfiguration(command);
            helpBuilder.CustomizeLayout(c =>
                c.Command == commandWithTypicalHelp
                    ? HelpBuilder.Default.GetLayout()
                    : new Action<HelpContext>[]
                        {
                            c => c.Output.WriteLine("Custom layout!")
                        }
                        .Concat(HelpBuilder.Default.GetLayout()));

            var typicalOutput = new StringWriter();
            config.Out = typicalOutput;
            command.Parse("typical -h", config).Invoke();

            var customOutput = new StringWriter();
            config.Out = customOutput;
            command.Parse("custom -h", config).Invoke();

            typicalOutput.ToString().Should().Be(GetDefaultHelp(commandWithTypicalHelp, false));
            customOutput.ToString().Should().Be($"Custom layout!{NewLine}{NewLine}{GetDefaultHelp(commandWithCustomHelp, false)}");
        }

        [Fact]
        public void Help_default_sections_can_be_wrapped()
        {
            Command command = new("test")
            {
                new Option<string>("--option") 
                { 
                    Description = "option description",
                    HelpName = "option"
                },
                new HelpOption()
                {
                    Action = new HelpAction()
                    {
                        Builder = new HelpBuilder(30)
                    }
                }
            };

            CommandLineConfiguration config = new(command)
            {
                Out = new StringWriter()
            };

            config.Invoke("test -h");

            string result = config.Out.ToString();
            result.Should().Be(
        $"Description:{NewLine}{NewLine}" +
        $"Usage:{NewLine}  test [options]{NewLine}{NewLine}" +
        $"Options:{NewLine}" +
        $"  --option   option {NewLine}" +
        $"  <option>   description{NewLine}" +
        $"  -?, -h,    Show help and {NewLine}" +
        $"  --help     usage {NewLine}" +
        $"             information{NewLine}{NewLine}{NewLine}");
        }



        [Fact]
        public void Help_customized_sections_can_be_wrapped()
        {
            CommandLineConfiguration config = new(new RootCommand() {  new HelpOption() })
            {
                Out = new StringWriter()
            };

            ParseResult parseResult = config.RootCommand.Parse("-h", config);

            if (parseResult.Action is HelpAction helpAction)
            {
                helpAction.Builder = new HelpBuilder(10);
                helpAction.Builder.CustomizeLayout(CustomLayout);
            }

            parseResult.Invoke();

            string result = config.Out.ToString();
            result.Should().Be($"  123  123{NewLine}  456  456{NewLine}  78   789{NewLine}       0{NewLine}{NewLine}{NewLine}");

            IEnumerable<Action<HelpContext>> CustomLayout(HelpContext _)
            {
                yield return ctx => ctx.HelpBuilder.WriteColumns(new[] { new TwoColumnHelpRow("12345678", "1234567890") }, ctx);
            }
        }

        private string GetDefaultHelp(Command command, bool trimOneNewline = true)
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

            CommandLineConfiguration config = new(command)
            {
                Out = new StringWriter()
            };

            config.Invoke("-h");

            var output = config.Out.ToString();

            if (trimOneNewline)
            {
                output = output.Substring(0, output.Length - NewLine.Length);
            }
            return output;
        }
    }
}
