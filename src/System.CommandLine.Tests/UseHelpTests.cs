﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Help;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.CommandLine.Tests.Utility;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using static System.Environment;

namespace System.CommandLine.Tests
{
    public class UseHelpTests
    {
        private readonly TestConsole _console = new();

        [Fact]
        public async Task UseHelp_writes_help_for_the_specified_command()
        {
            var command = new Command("command");
            var subcommand = new Command("subcommand");
            command.Subcommands.Add(subcommand);

            var config =
                new CommandLineBuilder(new RootCommand
                    {
                        command
                    })
                    .UseHelp()
                    .Build();

            var result = command.Parse("command subcommand --help", config);

            await result.InvokeAsync(_console);

            _console.Out.ToString().Should().Contain($"{RootCommand.ExecutableName} command subcommand [options]");
        }
         
        [Fact]
        public async Task UseHelp_interrupts_execution_of_the_specified_command()
        {
            var wasCalled = false;
            var command = new Command("command");
            var subcommand = new Command("subcommand");
            subcommand.SetHandler(() => wasCalled = true);
            command.Subcommands.Add(subcommand);

            var config =
                new CommandLineBuilder(new RootCommand
                    {
                        command
                    })
                    .UseHelp()
                    .Build();

            await config.InvokeAsync("command subcommand --help", _console);

            wasCalled.Should().BeFalse();
        }

        [Theory]
        [InlineData("-h")]
        [InlineData("--help")]
        [InlineData("-?")]
        [InlineData("/?")]
        public async Task UseHelp_accepts_default_values(string value)
        {
            var config =
                new CommandLineBuilder(new RootCommand
                    {
                        new Command("command")
                    })
                    .UseHelp()
                    .Build();

            await config.InvokeAsync($"command {value}", _console);

            _console.Should().ShowHelp();
        }

        [Fact]
        public async Task UseHelp_does_not_display_when_option_defined_with_same_alias()
        {
            var command = new Command("command");
            command.Options.Add(new Option<bool>("-h"));
            
            var config =
                new CommandLineBuilder(new RootCommand
                    {
                        command
                    })
                    .UseHelp()
                    .Build();

            var result = command.Parse("command -h", config);

            await result.InvokeAsync(_console);

            _console.Out.ToString().Should().BeEmpty();
        }

        [Fact]
        public void There_are_no_parse_errors_when_help_is_invoked_on_root_command()
        {
            RootCommand rootCommand = new ();
            var config = new CommandLineBuilder(rootCommand)
                .UseHelp()
                .Build();

            var result = rootCommand.Parse("-h", config);

            result.Errors
                  .Should()
                  .BeEmpty();
        }
        
        [Fact]
        public void There_are_no_parse_errors_when_help_is_invoked_on_subcommand()
        {
            var root = new RootCommand
            {
                new Command("subcommand")
            };

            var config = new CommandLineBuilder(root)
                         .UseHelp()
                         .Build();

            var result = root.Parse("subcommand -h", config);

            result.Errors
                  .Should()
                  .BeEmpty();
        }

        [Fact]
        public void There_are_no_parse_errors_when_help_is_invoked_on_a_command_with_subcommands()
        {
            var root = new RootCommand
            {
                new Command("subcommand")
            };

            var config = new CommandLineBuilder(root)
                         .UseHelp()
                         .Build();

            var result = root.Parse("-h", config);

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
            };

            var configuration = new CommandLineBuilder(command)
                         .UseHelp()
                         .Build();
            var result = command.Parse("-h", configuration);

            result.Errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData("-h")]
        [InlineData("inner -h")]
        public void UseHelp_can_be_called_more_than_once_on_the_same_CommandLineBuilder(string commandline)
        {
            var root = new RootCommand
            {
                new Command("inner")
            };

            var config = new CommandLineBuilder(root)
                         .UseHelp()
                         .UseHelp()
                         .Build();

            config.Invoke(commandline, _console);

            _console.Should().ShowHelp();
        }

        [Theory]
        [InlineData("-h")]
        [InlineData("inner -h")]
        public void UseHelp_can_be_called_more_than_once_on_the_same_command_with_different_CommandLineBuilders(string commandline)
        {
            var root = new RootCommand
            {
                new Command("inner")
            };

            var config = new CommandLineBuilder(root)
                         .UseHelp()
                         .Build();

            var console1 = new TestConsole();

            config.Invoke(commandline, console1);

            console1.Should().ShowHelp();

            var parser2 = new CommandLineBuilder(root)
                          .UseHelp()
                          .Build();
            var console2 = new TestConsole();

            parser2.Invoke(commandline, console2);

            console2.Should().ShowHelp();
        }

        [Theory]
        [InlineData("/lost")]
        [InlineData("--confused")]
        public async Task UseHelp_with_custom_aliases_uses_aliases(string helpAlias)
        {
            var config =
                new CommandLineBuilder(new RootCommand())
                    .UseHelp("/lost", "--confused")
                    .Build();

            await config.InvokeAsync(helpAlias, _console);

            _console.Should().ShowHelp();
        }

        [Theory]
        [InlineData("-h")]
        [InlineData("/h")]
        [InlineData("--help")]
        [InlineData("-?")]
        [InlineData("/?")]
        public async Task UseHelp_with_custom_aliases_default_aliases_replaced(string helpAlias)
        {
            var config =
                new CommandLineBuilder(new RootCommand())
                    .UseHelp("--confused")
                    .Build();

            await config.InvokeAsync(helpAlias, _console);

            _console.Out.ToString().Should().Be("");
        }

        [Fact]
        public void Individual_symbols_can_be_customized()
        {
            var subcommand = new Command("subcommand", "The default command description");
            var option = new Option<int>("-x", "The default option description");
            var argument = new Argument<int>("int-value", "The default argument description");

            var rootCommand = new RootCommand
            {
                subcommand,
                option,
                argument
            };

            var config = new CommandLineBuilder(rootCommand)
                         .UseHelp(ctx =>
                         {
                             ctx.HelpBuilder.CustomizeSymbol(subcommand, secondColumnText: "The custom command description");
                             ctx.HelpBuilder.CustomizeSymbol(option, secondColumnText: "The custom option description");
                             ctx.HelpBuilder.CustomizeSymbol(argument, secondColumnText: "The custom argument description");
                         })
                         .Build();

            var console = new TestConsole();
            config.Invoke("-h", console);

            console.Out
                   .ToString()
                   .Should()
                   .ContainAll("The custom command description",
                               "The custom option description",
                               "The custom argument description");
        }

        [Fact]
        public void Help_sections_can_be_replaced()
        {
            var config = new CommandLineBuilder(new RootCommand())
                         .UseHelp(ctx => ctx.HelpBuilder.CustomizeLayout(CustomLayout))
                         .Build();

            var console = new TestConsole();
            config.Invoke("-h", console);

            console.Out.ToString().Should().Be($"one{NewLine}{NewLine}two{NewLine}{NewLine}three{NewLine}{NewLine}{NewLine}");

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
            var command = new RootCommand("hello");
            var config = new CommandLineBuilder(command)
                         .UseHelp(ctx => ctx.HelpBuilder.CustomizeLayout(CustomLayout))
                         .Build();

            var console = new TestConsole();
            config.Invoke("-h", console);

            var output = console.Out.ToString();
            var defaultHelp = GetDefaultHelp(command);

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
            var commandWithTypicalHelp = new Command("typical");
            var commandWithCustomHelp = new Command("custom");
            var command = new RootCommand
            {
                commandWithTypicalHelp,
                commandWithCustomHelp
            };

            var config = new CommandLineBuilder(command)
                         .UseHelp(
                             ctx =>
                                 ctx.HelpBuilder
                                    .CustomizeLayout(c =>
                                                         c.Command == commandWithTypicalHelp
                                                             ? HelpBuilder.Default.GetLayout()
                                                             : new Action<HelpContext>[]
                                                                 {
                                                                     c => c.Output.WriteLine("Custom layout!")
                                                                 }
                                                                 .Concat(HelpBuilder.Default.GetLayout())))
                         .Build();

            var typicalOutput = new TestConsole();
            config.Invoke("typical -h", typicalOutput);

            var customOutput = new TestConsole();
            config.Invoke("custom -h", customOutput);

            typicalOutput.Out.ToString().Should().Be(GetDefaultHelp(commandWithTypicalHelp, false));
            customOutput.Out.ToString().Should().Be($"Custom layout!{NewLine}{NewLine}{GetDefaultHelp(commandWithCustomHelp, false)}");
        }

        [Fact]
        public void Help_default_sections_can_be_wrapped()
        {
            Command command = new("test")
            {
                new Option<string>("--option", "option description")
            };

            var config = new CommandLineBuilder(command)
                         .UseHelp(maxWidth: 30)
                         .Build();

            var console = new TestConsole();
            config.Invoke("test -h", console);

            string result = console.Out.ToString();
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
            var config = new CommandLineBuilder(new RootCommand())
                         .UseHelp(ctx => ctx.HelpBuilder.CustomizeLayout(CustomLayout), maxWidth: 10)
                         .Build();

            var console = new TestConsole();
            config.Invoke("-h", console);

            string result = console.Out.ToString();
            result.Should().Be($"  123  123{NewLine}  456  456{NewLine}  78   789{NewLine}       0{NewLine}{NewLine}{NewLine}");

            IEnumerable<Action<HelpContext>> CustomLayout(HelpContext _)
            {
                yield return ctx => ctx.HelpBuilder.WriteColumns(new[] { new TwoColumnHelpRow("12345678", "1234567890") }, ctx);
            }
        }

        private string GetDefaultHelp(Command command, bool trimOneNewline = true)
        {
            var console = new TestConsole();

            var config = new CommandLineBuilder(command)
                         .UseHelp()
                         .Build();

            config.Invoke("-h", console);

            var output = console.Out.ToString();

            if (trimOneNewline)
            {
                output = output.Substring(0, output.Length - NewLine.Length);
            }
            return output;
        }
    }
}
