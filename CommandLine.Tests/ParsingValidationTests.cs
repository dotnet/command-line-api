using System;
using System.IO;
using FluentAssertions;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using static Microsoft.DotNet.Cli.CommandLine.Accept;
using static Microsoft.DotNet.Cli.CommandLine.Create;

namespace Microsoft.DotNet.Cli.CommandLine.Tests
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
            var parser = new Parser(
                Option("-x", "",
                       AnyOneOf("this", "that", "the-other-thing")));

            var result = parser.Parse("-x none-of-those");

            result.Errors
                  .Should()
                  .Contain(e => e.Message == "Required argument missing for option: -x");
        }

        [Fact]
        public void When_an_option_has_en_error_then_the_error_has_a_reference_to_the_option()
        {
            var option = Option("-x", "", AnyOneOf("this", "that"));

            var parser = new Parser(option);

            var result = parser.Parse("-x something_else");

            result.Errors
                  .Where(e => e.Option != null)
                  .Should()
                  .Contain(e => e.Option.Name == option.Name);
        }

        [Fact]
        public void When_a_required_argument_is_not_supplied_then_an_error_is_returned()
        {
            var parser = new Parser(Option("-x", "", ExactlyOneArgument()));

            var result = parser.Parse("-x");

            result.Errors
                  .Should()
                  .Contain(e => e.Message == "Required argument missing for option: -x");
        }

        [Fact]
        public void When_no_option_accepts_arguments_but_one_is_supplied_then_an_error_is_returned()
        {
            var parser = new Parser(Command("the-command", "", Option("-x", "", NoArguments())));

            var result = parser.Parse("the-command -x some-arg");

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .ContainSingle(e => e == "Unrecognized command or argument 'some-arg'");
        }

        [Fact]
        public void An_option_can_be_invalid_when_used_in_combination_with_another_option()
        {
            var validator = new ArgumentsRule(p =>
            {
                if (p.AppliedOptions.Contains("one") &&
                    p.AppliedOptions.Contains("two"))
                {
                    return "Options '--one' and '--two' cannot be used together.";
                }
                return null;
            });

            var command = Command("the-command", "",
                                  validator,
                                  Option("--one", ""),
                                  Option("--two", ""));

            var result = command.Parse("the-command --one --two");

            result
                .Errors
                .Select(e => e.Message)
                .Should()
                .Contain("Options '--one' and '--two' cannot be used together.");
        }

        [Fact]
        public void An_argument_can_be_invalid_based_on_file_existence()
        {
            var command = Command("move", "",
                                  ExactlyOneArgument()
                                      .ExistingFilesOnly(),
                                  Option("--to", "",
                                         ExactlyOneArgument()));

            var result = command.Parse($@"move ""{Guid.NewGuid()}.txt"" ""{Path.Combine(Directory.GetCurrentDirectory(), ".trash")}""");

            output.WriteLine(result.Diagram());

            result["move"]
                .Arguments
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void An_argument_can_be_invalid_based_on_directory_existence()
        {
            var command = Command("move", "",
                                  ExactlyOneArgument()
                                      .ExistingFilesOnly(),
                                  Option("--to", "",
                                         ExactlyOneArgument()));

            var result = command.Parse($@"move ""{Directory.GetCurrentDirectory()}"" ""{Path.Combine(Directory.GetCurrentDirectory(), ".trash")}""");

            output.WriteLine(result.Diagram());

            result["move"]
                .Arguments
                .Should()
                .BeEquivalentTo(Directory.GetCurrentDirectory());
        }

        [Fact]
        public void When_there_are_subcommands_and_options_then_a_subcommand_must_be_provided()
        {
            var command = Command("outer", "",
                                  Command("inner", "",
                                          OneOrMoreArguments(),
                                          Command("three", "")));

            var result = command.Parse("outer inner arg");

            result.Errors
                  .Should()
                  .ContainSingle(
                      e => e.Message == "Required command was not provided." &&
                           e.Option.Name == "inner");
        }
    }
}