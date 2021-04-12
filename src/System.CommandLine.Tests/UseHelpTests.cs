// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.CommandLine.Tests.Utility;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class UseHelpTests
    {
        private readonly TestConsole _console = new TestConsole();

        [Fact]
        public async Task UseHelp_writes_help_for_the_specified_command()
        {
            var command = new Command("command");
            var subcommand = new Command("subcommand");
            command.AddCommand(subcommand);

            var parser =
                new CommandLineBuilder()
                    .AddCommand(command)
                    .UseHelp()
                    .Build();

            var result = parser.Parse("command subcommand --help");

            await result.InvokeAsync(_console);

            _console.Out.ToString().Should().Contain($"{RootCommand.ExecutableName} [options] command subcommand");
        }
         
        [Fact]
        public async Task UseHelp_interrupts_execution_of_the_specified_command()
        {
            var wasCalled = false;
            var command = new Command("command");
            var subcommand = new Command("subcommand");
            subcommand.Handler = CommandHandler.Create(() => wasCalled = true);
            command.AddCommand(subcommand);

            var parser =
                new CommandLineBuilder()
                    .AddCommand(command)
                    .UseHelp()
                    .Build();

            await parser.InvokeAsync("command subcommand --help", _console);

            wasCalled.Should().BeFalse();
        }

        [Theory]
        [InlineData("-h")]
        [InlineData("--help")]
        [InlineData("-?")]
        [InlineData("/?")]
        public async Task UseHelp_accepts_default_values(string value)
        {
            var parser =
                new CommandLineBuilder()
                    .AddCommand(new Command("command"))
                    .UseHelp()
                    .Build();

            await parser.InvokeAsync($"command {value}", _console);

            _console.Should().ShowHelp();
        }

        [Fact]
        public async Task UseHelp_does_not_display_when_option_defined_with_same_alias()
        {
            var command = new Command("command");
            command.AddOption(new Option("-h"));
            
            var parser =
                new CommandLineBuilder()
                    .AddCommand(command)
                    .UseHelp()
                    .Build();

            var result = parser.Parse("command -h");

            await result.InvokeAsync(_console);

            _console.Out.ToString().Should().BeEmpty();
        }

        [Fact]
        public void There_are_no_parse_errors_when_help_is_invoked_on_root_command()
        {
            var parser = new CommandLineBuilder()
                .UseHelp()
                .Build();

            var result = parser.Parse("-h");

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

            var parser = new CommandLineBuilder(root)
                         .UseHelp()
                         .Build();

            var result = parser.Parse("subcommand -h");

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

            var parser = new CommandLineBuilder(root)
                         .UseHelp()
                         .Build();

            var result = parser.Parse("-h");

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

            var result = new CommandLineBuilder(command)
                         .UseHelp()
                         .Build()
                         .Parse("-h");

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

            var parser = new CommandLineBuilder(root)
                         .UseHelp()
                         .UseHelp()
                         .Build();

            parser.Invoke(commandline, _console);

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

            var parser = new CommandLineBuilder(root)
                         .UseHelp()
                         .Build();

            var console1 = new TestConsole();

            parser.Invoke(commandline, console1);

            console1.Should().ShowHelp();

            var parser2 = new CommandLineBuilder(root)
                          .UseHelp()
                          .Build();
            var console2 = new TestConsole();

            parser2.Invoke(commandline, console2);

            console2.Should().ShowHelp();
        }

        [Fact]
        public void UseHelp_allows_help_builder_to_be_customized()
        {
            var option = new Option<string>("-x");
            var command = new RootCommand
            {
                option
            };

            var console = new TestConsole();

            new CommandLineBuilder(command)
                         .UseHelp<HelpBuilder>(builder =>
                         {
                             builder.Customize(option, descriptor: "-x (eXtreme)");
                         })
                         .Build()
                         .Invoke("-h", console);

            console.Should().ShowHelp();
            console.Out.ToString().Should().Contain("-x (eXtreme)");
        }

        [Fact]
        public void UseHelp_specifying_multiple_delegates_the_last_one_in_wins()
        {
            var option = new Option<string>("-x");
            var command = new RootCommand
            {
                option
            };

            var console = new TestConsole();

            new CommandLineBuilder(command)
                         .UseHelp<HelpBuilder>(builder =>
                         {
                             builder.Customize(option, descriptor: "-x (eXtreme)");
                         })
                         .UseHelp<HelpBuilder>(null)
                         .Build()
                         .Invoke("-h", console);

            console.Should().ShowHelp();
            console.Out.ToString().Should().NotContain("-x (eXTreme)");
        }

        [Fact]
        public void UseHelp_specifying_wrong_type_for_the_help_builder_throws_exception()
        {
            var option = new Option<string>("-x");
            var command = new RootCommand
            {
                option
            };


            var console = new TestConsole();
            var parser = new CommandLineBuilder(command)
                         .UseHelp<HelpBuilder>(builder =>
                         {
                             builder.Customize(option, descriptor: "-x (eXtreme)");
                         })
                         .UseHelpBuilder(context => new CustomHelpBuilder())
                         .Build();

            Action action = () => parser.Invoke("-h", console);
            action.Should().Throw<InvalidCastException>();
        }

        private class CustomHelpBuilder : IHelpBuilder
        {
            public void Write(ICommand command)
            {
                throw new NotImplementedException();
            }
        }
    }
}
