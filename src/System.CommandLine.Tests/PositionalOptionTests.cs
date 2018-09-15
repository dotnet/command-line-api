using System.Collections.Generic;
using System.CommandLine.Builder;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace System.CommandLine.Tests
{
    public class PositionalOptionTests
    {
        [Fact]
        public void When_an_option_has_a_default_value_then_a_given_positional_value_should_overriden()
        {
            var configuration = new CommandLineConfiguration(
                new[] {
                    new Option(
                        "-x",
                        "",
                        new ArgumentBuilder()
                            .WithDefaultValue(() => "123")
                            .ParseArgumentsAs<int>()),
                    new Option(
                        "-y",
                        "",
                        new ArgumentBuilder()
                            .WithDefaultValue(() => "456")
                            .ParseArgumentsAs<int>())
                },
                enablePositionalOptions: true);

            var parser = new Parser(configuration);

            var result = parser.Parse("42");

            result.Errors.Should().BeEmpty();
            result.RootCommandResult.ValueForOption("-x").Should().Be(42);
            result.RootCommandResult.ValueForOption("-y").Should().Be(456);
        }

        [Fact]
        public void When_an_option_has_a_default_value_then_a_given_positional_value_should_override_with_other_specified()
        {
            var configuration = new CommandLineConfiguration(
                new[] {
                    new Option(
                        "-x",
                        "",
                        new ArgumentBuilder()
                            .WithDefaultValue(() => "123")
                            .ParseArgumentsAs<int>()),
                    new Option(
                        "-y",
                        "",
                        new ArgumentBuilder()
                            .WithDefaultValue(() => "456")
                            .ParseArgumentsAs<int>())
                },
                enablePositionalOptions: true);

            var parser = new Parser(configuration);

            var result = parser.Parse("-y 23 42");

            result.Errors.Should().BeEmpty();
            result.RootCommandResult.ValueForOption("-x").Should().Be(42);
            result.RootCommandResult.ValueForOption("-y").Should().Be(23);
        }

        [Fact]
        public void When_a_sibling_commands_have_options_with_the_same_name_it_matches_based_on_command()
        {
            var parser = new CommandLineBuilder()
                         .EnablePositionalOptions()
                         .AddCommand("command1", symbols: b =>
                                         b.AddOption("-anon", arguments: argumentsBuilder => argumentsBuilder.ExactlyOne())
                         )
                         .AddCommand("command2", symbols: b =>
                                         b.AddOption("-anon", arguments: argumentsBuilder => argumentsBuilder.ExactlyOne())
                         )
                         .Build();

            ParseResult result = parser.Parse("command2 anon-value");

            result.Errors.Should().BeEmpty();
            result.CommandResult.Name.Should().Be("command2");
            result.CommandResult["-anon"].GetValueOrDefault<string>().Should().Be("anon-value");
        }

        [Theory]
        [InlineData(2, 0)]
        [InlineData(1, 1)]
        [InlineData(0, 2)]
        public void When_nested_subcommands_have_options_they_can_be_positional(
            int subcommand1Options,
            int subcommand2Options)
        {
            var parser = new CommandLineBuilder()
                         .EnablePositionalOptions()
                         .AddCommand("subcommand1", symbols: b => {
                             foreach (int optionIndex in Enumerable.Range(1, subcommand1Options))
                             {
                                 b.AddOption($"-anon{optionIndex}", arguments: argumentsBuilder => argumentsBuilder.ExactlyOne());
                             }

                             b.AddCommand("subcommand2", symbols: subCommandBuilder => {
                                 foreach (int optionIndex in Enumerable.Range(1, subcommand2Options))
                                 {
                                     subCommandBuilder.AddOption($"-anon{optionIndex}",
                                                                 arguments: argumentsBuilder => argumentsBuilder.ExactlyOne());
                                 }
                             });
                         })
                         .Build();

            string commandLine = string.Join(' ', GetCommandLineParts());

            ParseResult result = parser.Parse(commandLine);

            result.Errors.Should().BeEmpty();
            for (var commandResult = result.CommandResult; commandResult != null; commandResult = commandResult.Parent)
            {
                int index = 1;
                foreach (var optionResult in commandResult.Children.OfType<OptionResult>())
                {
                    optionResult.GetValueOrDefault<string>().Should().Be($"anon{index++}-value");
                }
            }

            IEnumerable<string> GetCommandLineParts()
            {
                yield return "subcommand1";
                foreach (int optionIndex in Enumerable.Range(1, subcommand1Options))
                {
                    yield return $"anon{optionIndex}-value";
                }

                yield return "subcommand2";
                foreach (int optionIndex in Enumerable.Range(1, subcommand2Options))
                {
                    yield return $"anon{optionIndex}-value";
                }
            }
        }

        [Fact]
        public void When_a_subcommand_has_options_they_can_be_positional()
        {
            var parser = new CommandLineBuilder()
                         .EnablePositionalOptions()
                         .AddCommand("subcommand", symbols: b =>
                                         b.AddOption("-anon1", arguments: argumentsBuilder => argumentsBuilder.ExactlyOne())
                                          .AddOption("-anon2", arguments: argumentsBuilder => argumentsBuilder.ExactlyOne())
                         )
                         .Build();

            ParseResult result = parser.Parse("subcommand anon1-value anon2-value");

            result.Errors.Should().BeEmpty();
            result.CommandResult["-anon1"].GetValueOrDefault<string>().Should().Be("anon1-value");
            result.CommandResult["-anon2"].GetValueOrDefault<string>().Should().Be("anon2-value");
        }
    }
}
