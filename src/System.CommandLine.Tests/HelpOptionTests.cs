// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.Tests.Utility;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.CommandLine.Tests;

public class HelpOptionTests
{
    [Fact]
    public async Task Help_option_writes_help_for_the_specified_command()
    {
        Command command = new RootCommand
        {
            new Command("command")
            {
                new Command("subcommand")
            }
        };

        var result = command.Parse("command subcommand --help");

        var output = new StringWriter();
        await result.InvokeAsync(new() { Output = output }, CancellationToken.None);

        output.ToString().Should().Contain($"{RootCommand.ExecutableName} command subcommand [options]");
    }

    [Fact]
    public async Task Help_option_interrupts_execution_of_the_specified_command()
    {
        var wasCalled = false;
        var command = new Command("command") { new HelpOption() };
        var subcommand = new Command("subcommand");
        subcommand.SetAction(_ => wasCalled = true);
        command.Subcommands.Add(subcommand);

        var output = new StringWriter();

        await command.Parse("command subcommand --help").InvokeAsync(new() { Output = output }, CancellationToken.None);

        wasCalled.Should().BeFalse();
    }

    [Theory]
    [InlineData("-h")]
    [InlineData("--help")]
    [InlineData("-?")]
    [InlineData("/?")]
    public async Task Help_option_accepts_default_values(string value)
    {
        var command = new Command("command")
        {
            new HelpOption()
        };

        StringWriter output = new();

        await command.Parse($"command {value}").InvokeAsync(new() { Output = output }, CancellationToken.None);

        output.ToString().Should().ShowHelp();
    }

    [Fact]
    public async Task Help_option_does_not_display_when_option_defined_with_same_alias()
    {
        var command = new Command("command");
        command.Options.Add(new Option<bool>("-h"));
        
        var output = new StringWriter();

        await command.Parse("command -h").InvokeAsync(new() { Output = output }, CancellationToken.None);

        output.ToString().Should().NotShowHelp();
    }

    [Fact]
    public void There_are_no_parse_errors_when_help_is_invoked_on_root_command()
    {
        RootCommand rootCommand = new();

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
                Required = true
            },
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
        var output = new StringWriter();

        await command.Parse(helpAlias).InvokeAsync(new() { Output = output }, CancellationToken.None);

        output.ToString().Should().ShowHelp();
    }

    [Theory]
    [InlineData("-h")]
    [InlineData("/h")]
    [InlineData("--help")]
    [InlineData("-?")]
    [InlineData("/?")]
    public async Task Help_option_with_custom_aliases_does_not_recognize_default_aliases(string helpAlias)
    {
        RootCommand command = new();
        command.Options.Clear();
        command.Options.Add(new HelpOption("--confused"));

        var output = new StringWriter();

        await command.Parse(helpAlias).InvokeAsync(new() { Output = output }, CancellationToken.None);

        output.ToString().Should().NotContain(helpAlias);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void The_users_can_provide_usage_examples(bool subcommand)
    {
        HelpOption helpOption = new();
        helpOption.Action = new CustomizedHelpAction(helpOption);

        RootCommand rootCommand = new();
        rootCommand.Options.Clear();
        rootCommand.Options.Add(helpOption);
        rootCommand.Subcommands.Add(new Command("subcommand")
        {
            new Option<string>("-x")
            {
                Description = "An example option."
            }
        });

        TextWriter output = new StringWriter();
       
        var result = subcommand ? rootCommand.Parse("subcommand -h") : rootCommand.Parse("-h");
        result.InvocationConfiguration.Output = output;
        result.Invoke();

        if (subcommand)
        {
            output.ToString().Should().Contain(CustomizedHelpAction.CustomUsageText);
        }
        else
        {
            output.ToString().Should().NotContain(CustomizedHelpAction.CustomUsageText);
        }
    }

    [Fact]
    public void The_users_can_print_help_output_of_a_subcommand()
    {
        const string RootDescription = "This is a custom root description.";
        const string SubcommandDescription = "This is a custom subcommand description.";
        RootCommand rootCommand = new(RootDescription);
        Command subcommand = new("subcommand", SubcommandDescription)
        {
            new Option<string>("-x")
            {
                Description = "An example option."
            }
        };
        rootCommand.Subcommands.Add(subcommand);

        var output = new StringWriter();

        subcommand.Parse("--help").Invoke(new() { Output = output });

        output.ToString().Should().Contain(SubcommandDescription);
        output.ToString().Should().NotContain(RootDescription);
    }

    [Fact]
    public void The_users_can_set_max_width()
    {
        string firstPart = new('a', count: 50);
        string secondPart = new('b', count: 50);
        string description = firstPart + secondPart;

        RootCommand rootCommand = new(description);
        rootCommand.Options.Clear();
        rootCommand.Options.Add(new HelpOption
        {
            Action = new HelpAction
            {
                MaxWidth = 2 /* each line starts with two spaces */ + description.Length / 2
            }
        });
        StringWriter output = new ();

        rootCommand.Parse("--help").Invoke(new() { Output = output });

        output.ToString().Should().NotContain(description);
        output.ToString().Should().Contain($"  {firstPart}{Environment.NewLine}");
        output.ToString().Should().Contain($"  {secondPart}{Environment.NewLine}");
    }

    [Fact] // https://github.com/dotnet/command-line-api/issues/2640
    public void DefaultValueFactory_does_not_throw_when_help_is_invoked()
    {
        var invocationConfiguration = new InvocationConfiguration
        {
            Output = new StringWriter(),
            Error = new StringWriter()
        };

        Command subcommand = new("do")
        {
            new Option<DirectoryInfo>("-x")
            {
                DefaultValueFactory = result =>
                {
                    result.AddError("Oops!");
                    return null;
                }
            }
        };
        subcommand.SetAction(_ => { });
        RootCommand rootCommand = new()
        {
            subcommand
        };

        rootCommand.Parse("do --help").Invoke(invocationConfiguration);

        invocationConfiguration.Error.ToString().Should().Be("");
    }

    [Fact] // https://github.com/dotnet/command-line-api/issues/2589
    public void Help_and_version_options_are_displayed_after_other_options_on_root_command()
    {
        var command = new RootCommand
        {
            Subcommands =
            {
                new Command("subcommand")
                {
                    Description = "The subcommand"
                }
            },
            Options =
            {
                new Option<int>("-i")
                {
                    Description = "The option"
                }
            }
        };

        var output = new StringWriter();


        command.Parse("-h").Invoke(new() { Output = output });

        output.ToString()
              .Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries)
              .Select(line => line.Trim())
              .Should()
              .ContainInOrder([
                  "Options:",
                  "-i <i>          The option",
                  "-?, -h, --help  Show help and usage information",
                  "--version       Show version information",
                  "Commands:"
              ]);
    }

    [Fact] // https://github.com/dotnet/command-line-api/issues/2589
    public void Help_and_version_options_are_displayed_after_other_options_on_subcommand()
    {
        var command = new RootCommand
        {
            Subcommands =
            {
                new Command("subcommand")
                {
                    Description = "The subcommand", Options =
                    {
                        new Option<int>("-i")
                        {
                            Description = "The option"
                        }
                    }
                }
            }
        };

        var output = new StringWriter();

        command.Parse("subcommand -h").Invoke(new() { Output = output });

        output.ToString()
              .Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries)
              .Select(line => line.Trim())
              .Should()
              .ContainInOrder([
                  "Options:",
                  "-i <i>          The option",
                  "-?, -h, --help  Show help and usage information",
              ]);
    }

    private sealed class CustomizedHelpAction : SynchronousCommandLineAction
    {
        internal const string CustomUsageText = "This is custom command usage example.";

        private readonly HelpAction _helpAction;

        public CustomizedHelpAction(HelpOption helpOption)
        {
            _helpAction = (HelpAction)helpOption.Action;
        }

        public override int Invoke(ParseResult parseResult)
        {
            _helpAction.Invoke(parseResult);

            if (parseResult.CommandResult.Command.Name == "subcommand")
            {
                var output = parseResult.InvocationConfiguration.Output;
                output.WriteLine(CustomUsageText);
            }
            
            return 0;
        }
    }
}