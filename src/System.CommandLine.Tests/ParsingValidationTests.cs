using System.CommandLine.Builder;
using System.IO;
using FluentAssertions;
using System.Linq;
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
        public void When_an_option_accepts_only_specific_arguments_but_a_wrong_one_is_supplied_then_an_informative_error_is_returned()
        {
            var builder = new ArgumentDefinitionBuilder();
            var parser = new Parser(
                new OptionDefinition(
                    "-x",
                    "",
                    argumentDefinition: builder.FromAmong("this", "that", "the-other-thing").ExactlyOne()));

            var result = parser.Parse("-x none-of-those");

            result.Errors
                  .Should()
                  .Contain(e => e.Message == "Required argument missing for option: -x");
        }

        [Fact]
        public void When_an_option_has_en_error_then_the_error_has_a_reference_to_the_option()
        {
            var builder = new ArgumentDefinitionBuilder();
            var option = new OptionDefinition(
                "-x",
                "",
                argumentDefinition: builder.FromAmong("this", "that").ExactlyOne());

            var parser = new Parser(option);

            var result = parser.Parse("-x something_else");

            result.Errors
                  .Where(e => e.Symbol != null)
                  .Should()
                  .Contain(e => e.Symbol.Name == option.Name);
        }

        [Fact]
        public void When_a_required_argument_is_not_supplied_then_an_error_is_returned()
        {
            var builder = new ArgumentDefinitionBuilder();
            var parser = new Parser(new OptionDefinition(
                                        "-x",
                                        "",
                                        argumentDefinition: builder.ExactlyOne()));

            var result = parser.Parse("-x");

            result.Errors
                  .Should()
                  .Contain(e => e.Message == "Required argument missing for option: -x");
        }

        [Fact]
        public void When_no_option_accepts_arguments_but_one_is_supplied_then_an_error_is_returned()
        {
            var parser = new Parser(new CommandDefinition("the-command", "", new[] {
                new OptionDefinition(
                    "-x",
                    "",
                    argumentDefinition: ArgumentDefinition.None)
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
            var builder = new ArgumentDefinitionBuilder();
            builder.AddValidator(symbol => {
                if (symbol.Children.Contains("one") &&
                    symbol.Children.Contains("two"))
                {
                    return "Options '--one' and '--two' cannot be used together.";
                }

                return null;
            });

            var command = new CommandDefinition("the-command", "", new[] {
                new OptionDefinition(
                    "--one",
                    "",
                    argumentDefinition: null),
                (SymbolDefinition)new OptionDefinition(
                    "--two",
                    "",
                    argumentDefinition: null)
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
            var builder = new ArgumentDefinitionBuilder();
            var command = new CommandDefinition("the-command", "", symbolDefinitions: null, argumentDefinition: builder.LegalFilePathsOnly().ZeroOrMore());

            var invalidCharacters = $"|{Path.GetInvalidPathChars().First()}|";

            // Convert to ushort so the xUnit XML writer doesn't complain about invalid characters
            _output.WriteLine(string.Join("\n", Path.GetInvalidPathChars().Select((c) => (ushort)(c))));

            var result = command.Parse($"the-command {invalidCharacters}");

            result.UnmatchedTokens
                  .Should()
                  .BeEquivalentTo(invalidCharacters);
        }

        [Fact]
        public void LegalFilePathsOnly_accepts_arguments_containing_valid_path_characters()
        {
            var builder = new ArgumentDefinitionBuilder();
            var command = new CommandDefinition("the-command", "", symbolDefinitions: null, argumentDefinition: builder.LegalFilePathsOnly().ZeroOrMore());

            var validPathName = Directory.GetCurrentDirectory();
            var validNonExistingFileName = Path.Combine(validPathName, Guid.NewGuid().ToString());

            var result = command.Parse($"the-command {validPathName} {validNonExistingFileName}");

            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void An_argument_can_be_invalid_based_on_file_existence()
        {
            var commandDefinitionBuilder = new CommandDefinitionBuilder("move")
                .AddOption("--to", "", toArgs => toArgs.ExactlyOne());
            commandDefinitionBuilder.Arguments.ExistingFilesOnly().ExactlyOne();
            var command = commandDefinitionBuilder.BuildCommandDefinition();

            var result = command.Parse($@"move ""{Guid.NewGuid()}.txt"" ""{Path.Combine(Directory.GetCurrentDirectory(), ".trash")}""");

            _output.WriteLine(result.Diagram());

            result.Command()
                  .Arguments
                  .Should()
                  .BeEmpty();
        }

        [Fact]
        public void An_argument_can_be_invalid_based_on_directory_existence()
        {
            var parser = new ParserBuilder()
                         .AddCommand("move", "",
                                     toArgs => toArgs.AddOption("--to", "", args => args.ExactlyOne()),
                                     moveArgs => moveArgs.ExistingFilesOnly()
                                                         .ExactlyOne())
                         .Build();

            var result = parser.Parse($@"move ""{Directory.GetCurrentDirectory()}"" --to ""{Path.Combine(Directory.GetCurrentDirectory(), ".trash")}""");

            _output.WriteLine(result.Diagram());

            result.Command()
                  .Arguments
                  .Should()
                  .BeEquivalentTo(Directory.GetCurrentDirectory());
        }

        [Fact]
        public void When_there_are_subcommands_and_options_then_a_subcommand_must_be_provided()
        {
            var command = new CommandDefinitionBuilder("outer")
                          .AddCommand("inner", "",
                                      inner => inner.AddCommand("inner-er", ""))
                          .BuildCommandDefinition();

            var result = command.Parse("outer inner arg");

            result.Errors
                  .Should()
                  .ContainSingle(
                      e => e.Message.Equals(ValidationMessages.Instance.RequiredCommandWasNotProvided()) &&
                           e.Symbol.Name.Equals("inner"));
        }

        [Fact]
        public void When_an_option_is_specified_more_than_once_but_only_allowed_once_then_an_informative_error_is_returned()
        {
            var parser = new Parser(
                new OptionDefinition(
                    "-x",
                    "",
                    argumentDefinition: new ArgumentDefinitionBuilder().ExactlyOne()));

            var result = parser.Parse("-x 1 -x 2");

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .Contain("Option '-x' cannot be specified more than once.");
        }

        [Theory]
        [InlineData(":")]
        [InlineData("=")]
        public void When_an_option_contains_a_delimiter_then_an_informative_error_is_returned(string delimiter)
        {
            Action create = () => new Parser(
               new OptionDefinition(
                   $"-x{delimiter}",
                   "",
               new ArgumentDefinitionBuilder().ExactlyOne()));

            create.Should().Throw<ArgumentException>().Which.Message.Should()
                    .Be($"Symbol cannot contain delimiter: {delimiter}");
        }
    }
}
