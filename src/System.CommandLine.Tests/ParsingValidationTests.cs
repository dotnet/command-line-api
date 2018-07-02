using System.Collections.Generic;
using System.CommandLine.Builder;
using System.IO;
using System.Linq;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests
{
    public class ParsingValidationTests
    {
        private readonly ITestOutputHelper _output;

        public ParsingValidationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void
            When_an_option_accepts_only_specific_arguments_but_a_wrong_one_is_supplied_then_an_informative_error_is_returned()
        {
            var builder = new ArgumentBuilder();
            var parser = new Parser(
                new Option(
                    "-x",
                    "",
                    builder.FromAmong("this", "that", "the-other-thing").ExactlyOne()));

            var result = parser.Parse("-x none-of-those");

            result.Errors
                .Should()
                .Contain(e => e.Message == "Required argument missing for option: -x");
        }

        [Fact]
        public void When_an_option_has_en_error_then_the_error_has_a_reference_to_the_option()
        {
            var builder = new ArgumentBuilder();
            var option = new Option(
                "-x",
                "",
                builder.FromAmong("this", "that").ExactlyOne());

            var parser = new Parser(option);

            var result = parser.Parse("-x something_else");

            result.Errors
                  .Where(e => e.SymbolResult != null)
                  .Should()
                  .Contain(e => e.SymbolResult.Name == option.Name);
        }

        [Fact]
        public void When_a_required_argument_is_not_supplied_then_an_error_is_returned()
        {
            var builder = new ArgumentBuilder();
            var parser = new Parser(new Option(
                                        "-x",
                                        "",
                                        builder.ExactlyOne()));

            var result = parser.Parse("-x");

            result.Errors
                .Should()
                .Contain(e => e.Message == "Required argument missing for option: -x");
        }

        [Fact]
        public void When_no_option_accepts_arguments_but_one_is_supplied_then_an_error_is_returned()
        {
            var parser = new Parser(new Command("the-command", "", new[] {
                new Option("-x", "")
            }));

            var result = parser.Parse("the-command -x some-arg");

            _output.WriteLine(result.ToString());

            result.Errors
                .Select(e => e.Message)
                .Should()
                .ContainSingle(e => e == "Unrecognized command or argument 'some-arg'");
        }

        [Fact]
        public void An_option_can_be_invalid_when_used_in_combination_with_another_option()
        {
            var builder = new ArgumentBuilder();
            builder.AddValidator(symbol => {
                if (symbol.Children.Contains("one") &&
                    symbol.Children.Contains("two"))
                {
                    return "Options '--one' and '--two' cannot be used together.";
                }

                return null;
            });

            var command = new Command("the-command", "", new[] {
                new Option("--one", ""),
                new Option("--two", "")
            }, builder.ExactlyOne());

            var result = command.Parse("the-command --one --two");

            result
                .Errors
                .Select(e => e.Message)
                .Should()
                .Contain("Options '--one' and '--two' cannot be used together.");
        }

        [Fact]
        public void LegalFilePathsOnly_rejects_arguments_containing_invalid_path_characters()
        {
            var builder = new ArgumentBuilder();
            var command = new Command("the-command", "", builder.LegalFilePathsOnly().ZeroOrMore());

            var invalidCharacters = $"|{Path.GetInvalidPathChars().First()}|";

            // Convert to ushort so the xUnit XML writer doesn't complain about invalid characters
            _output.WriteLine(string.Join("\n", Path.GetInvalidPathChars().Select(c => (ushort)c)));

            var result = command.Parse($"the-command {invalidCharacters}");

            result.UnmatchedTokens
                .Should()
                .BeEquivalentTo(invalidCharacters);
        }

        [Fact]
        public void LegalFilePathsOnly_accepts_arguments_containing_valid_path_characters()
        {
            var builder = new ArgumentBuilder();
            var command = new Command("the-command", "", builder.LegalFilePathsOnly().ZeroOrMore());

            var validPathName = Directory.GetCurrentDirectory();
            var validNonExistingFileName = Path.Combine(validPathName, Guid.NewGuid().ToString());

            var result = command.Parse($"the-command {validPathName} {validNonExistingFileName}");

            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void An_argument_can_be_invalid_based_on_file_existence()
        {
            var commandBuilder = new CommandBuilder("move")
                .AddOption("--to", "", toArgs => toArgs.ExactlyOne());
            commandBuilder.Arguments.ExistingFilesOnly().ExactlyOne();
            var command = commandBuilder.BuildCommand();

            var result =
                command.Parse(
                    $@"move ""{Guid.NewGuid()}.txt"" ""{Path.Combine(Directory.GetCurrentDirectory(), ".trash")}""");

            _output.WriteLine(result.Diagram());

            result.CommandResult
                  .Arguments
                  .Should()
                  .BeEmpty();
        }

        [Fact]
        public void An_argument_can_be_invalid_based_on_directory_existence()
        {
            var parser = new CommandLineBuilder()
                .AddCommand("move", "",
                    toArgs => toArgs.AddOption("--to", "", args => args.ExactlyOne()),
                    moveArgs => moveArgs.ExistingFilesOnly()
                        .ExactlyOne())
                .Build();

            var currentDirectory = Directory.GetCurrentDirectory();
            var trash = Path.Combine(currentDirectory, ".trash");

            var commandLine = $@"move ""{currentDirectory}"" --to ""{trash}""";

            _output.WriteLine(commandLine);

            var result = parser.Parse(commandLine);

            _output.WriteLine(result.Diagram());

            result.CommandResult
                  .Arguments
                  .Should()
                  .BeEquivalentTo(currentDirectory);
        }

        [Fact]
        public void When_there_are_subcommands_and_options_then_a_subcommand_must_be_provided()
        {
            var command = new CommandBuilder("outer")
                          .AddCommand("inner", "",
                                      inner => inner.AddCommand("inner-er", ""))
                          .BuildCommand();

            var result = command.Parse("outer inner arg");

            result.Errors
                  .Should()
                  .ContainSingle(
                      e => e.Message.Equals(ValidationMessages.Instance.RequiredCommandWasNotProvided()) &&
                           e.SymbolResult.Name.Equals("inner"));
        }

        [Fact]
        public void
            When_an_option_is_specified_more_than_once_but_only_allowed_once_then_an_informative_error_is_returned()
        {
            var parser = new Parser(
                new Option(
                    "-x",
                    "",
                    new ArgumentBuilder().ExactlyOne()));

            var result = parser.Parse("-x 1 -x 2");

            result.Errors
                .Select(e => e.Message)
                .Should()
                .Contain("Option '-x' cannot be specified more than once.");
        }

        [Fact]
        public void ParseArgumentsAs_with_arity_of_One_validates_against_extra_arguments()
        {
            var parser = new Parser(
                new Option(
                    "-x",
                    "",
                    new ArgumentBuilder().ParseArgumentsAs<int>()));

            var result = parser.Parse("-x 1 -x 2");

            result.Errors
                .Select(e => e.Message)
                .Should()
                .Contain("Option '-x' cannot be specified more than once.");
        }

        [Fact]
        public void When_an_option_has_a_default_value_it_is_not_valid_to_specify_the_option_without_an_argument()
        {
            var parser = new Parser(
                new Option(
                    "-x", "",
                    new ArgumentBuilder()
                        .WithDefaultValue(() => "123")
                        .ParseArgumentsAs<int>()));

            var result = parser.Parse("-x");

            result.Errors
                .Select(e => e.Message)
                .Should()
                .Contain("Required argument missing for option: -x");
        }

        [Fact]
        public void When_an_option_has_a_default_value_then_the_default_should_apply_if_not_specified()
        {
            var parser = new Parser(
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
            );

            var result = parser.Parse("");

            result.Errors.Should().BeEmpty();
            result.RootCommandResult.ValueForOption("-x").Should().Be(123);
            result.RootCommandResult.ValueForOption("-y").Should().Be(456);
        }

        [Fact]
        public void When_an_option_has_a_default_value_then_a_given_positional_value_should_override()
        {
            var parser = new Parser(
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
            );

            var result = parser.Parse("42");

            result.Errors.Should().BeEmpty();
            result.RootCommandResult.ValueForOption("-x").Should().Be(42);
            result.RootCommandResult.ValueForOption("-y").Should().Be(456);
        }

        [Fact]
        public void When_an_option_has_a_default_value_then_a_given_positional_value_should_override_with_other_specified()
        {
            var parser = new Parser(
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
            );

            var result = parser.Parse("-y 23 42");

            result.Errors.Should().BeEmpty();
            result.RootCommandResult.ValueForOption("-x").Should().Be(42);
            result.RootCommandResult.ValueForOption("-y").Should().Be(23);
        }

        [Fact]
        public void When_a_command_line_has_unmatched_tokens_they_are_not_applied_to_subsequent_options()
        {
            var parser = new CommandLineBuilder()
                        .AddOption("-x", "",
                            argument => argument.ExactlyOne())
                        .AddOption("-y", "",
                            argument => argument.ExactlyOne())
                        .TreatUnmatchedTokensAsErrors(false)
                        .Build();

            var result = parser.Parse("-x 23 unmatched-token -y 42");

            result.ValueForOption("-x").Should().Be("23");
            result.ValueForOption("-y").Should().Be("42");
            result.UnmatchedTokens.Should().BeEquivalentTo("unmatched-token");
        }

        [Fact]
        public void When_a_subcommand_has_options_they_can_be_positional()
        {
            var parser = new CommandLineBuilder()
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

        [Fact]
        public void When_a_sibling_commands_have_options_with_the_same_name_it_matches_based_on_command()
        {
            var parser = new CommandLineBuilder()
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
        public void When_nested_subcommands_have_options_they_can_be_positional(int subcommand1Options,
            int subcommand2Options)
        {
            var parser = new CommandLineBuilder()
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
            _output.WriteLine($"Parsing {commandLine}");

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
    }
}
