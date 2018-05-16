using System;
using System.IO;
using FluentAssertions;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using static System.CommandLine.Create;

namespace System.CommandLine.Tests
{
    public class ParsingValidationTests
    {
        private readonly ITestOutputHelper output;

        public ParsingValidationTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void When_an_option_accepts_only_specific_arguments_but_a_wrong_one_is_supplied_then_an_informative_error_is_returned()
        {
            var builder = new ArgumentDefinitionBuilder();
            var parser = new OptionParser(
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

            var parser = new OptionParser(option);

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
            var parser = new OptionParser(new OptionDefinition(
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
            var parser = new CommandParser(Command("the-command", "", new OptionDefinition(
                                                       "-x",
                                                       "",
                                                       argumentDefinition: ArgumentDefinition.None)));

            var result = parser.Parse("the-command -x some-arg");

            output.WriteLine(result.ToString());

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .ContainSingle(e => e == "Unrecognized command or argument 'some-arg'");
        }

        [Fact]
        public void An_option_can_be_invalid_when_used_in_combination_with_another_option()
        {
            var builder = new ArgumentDefinitionBuilder();
            builder.AddValidator(symbol =>
            {
                if (symbol.Children.Contains("one") &&
                    symbol.Children.Contains("two"))
                {
                    return "Options '--one' and '--two' cannot be used together.";
                }

                return null;
            });

            var command = Command("the-command", "",
                                  builder.ExactlyOne(),
                                  new OptionDefinition(
                                      "--one",
                                      "",
                                      argumentDefinition: null),
                                  new OptionDefinition(
                                      "--two",
                                      "",
                                      argumentDefinition: null));

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
            var command = Command("the-command", "",
                                  builder.LegalFilePathsOnly().ZeroOrMore());

            var invalidCharacters = $"|{Path.GetInvalidPathChars().First()}|";

            // Convert to ushort so the xUnit XML writer doesn't complain about invalid characters
            output.WriteLine(string.Join("\n", Path.GetInvalidPathChars().Select((c) => (ushort)(c))));

            var result = command.Parse($"the-command {invalidCharacters}");

            result.UnmatchedTokens
                  .Should()
                  .BeEquivalentTo(invalidCharacters);
        }

        [Fact]
        public void LegalFilePathsOnly_accepts_arguments_containing_valid_path_characters()
        {
            var builder = new ArgumentDefinitionBuilder();
            var command = Command("the-command", "",
                builder.LegalFilePathsOnly().ZeroOrMore());

            var validPathName = Directory.GetCurrentDirectory();
            var validNonExistingFileName = Path.Combine(validPathName, Guid.NewGuid().ToString());

            var result = command.Parse($"the-command {validPathName} {validNonExistingFileName}");

            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void An_argument_can_be_invalid_based_on_file_existence()
        {
            var command = Command("move", "",
                new ArgumentDefinitionBuilder().ExistingFilesOnly().ExactlyOne(),
                                  new OptionDefinition(
                                      "--to",
                                      "",
                                      argumentDefinition: new ArgumentDefinitionBuilder().ExactlyOne()));

            var result = command.Parse($@"move ""{Guid.NewGuid()}.txt"" ""{Path.Combine(Directory.GetCurrentDirectory(), ".trash")}""");

            output.WriteLine(result.Diagram());

            ParseResultExtensions.Command(result)
                .Arguments
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void An_argument_can_be_invalid_based_on_directory_existence()
        {
            var command = Command("move", "",
                new ArgumentDefinitionBuilder().ExistingFilesOnly().ExactlyOne(),
                                  new OptionDefinition(
                                      "--to",
                                      "",
                                      argumentDefinition: new ArgumentDefinitionBuilder().ExactlyOne()));

            var result = command.Parse($@"move ""{Directory.GetCurrentDirectory()}"" --to ""{Path.Combine(Directory.GetCurrentDirectory(), ".trash")}""");

            output.WriteLine(result.Diagram());

            ParseResultExtensions.Command(result)
                .Arguments
                .Should()
                .BeEquivalentTo(Directory.GetCurrentDirectory());
        }

        [Fact]
        public void When_there_are_subcommands_and_options_then_a_subcommand_must_be_provided()
        {
            var command = Command("outer", "",
                                  Command("inner", "",
                                      new ArgumentDefinitionBuilder().OneOrMore(),
                                          Command("three", "")));

            var result = command.Parse("outer inner arg");

            output.WriteLine(result.Diagram());
            output.WriteLine(string.Join('\n', result.Errors));
            result.Errors
                  .Should()
                  .ContainSingle(
                      e => e.Message == "Required command was not provided." &&
                           e.Symbol.Name == "inner");
        }

        [Fact]
        public void When_an_option_is_specified_more_than_once_but_only_allowed_once_then_an_informative_error_is_returned()
        {
            var parser = new OptionParser(
                new OptionDefinition(
                    "-x",
                    "",
                    argumentDefinition: Define.Arguments().ExactlyOne()));

            var result = parser.Parse("-x 1 -x 2");

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .Contain("Option '-x' cannot be specified more than once.");
        }
    }
}
