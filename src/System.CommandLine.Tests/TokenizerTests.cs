using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.IO;
using FluentAssertions;
using FluentAssertions.Equivalency;
using System.Linq;
using FluentAssertions.Common;
using Xunit;
using Xunit.Abstractions;


namespace System.CommandLine.Tests
{
    public partial class TokenizerTests
    {

        [Fact]
        public void The_tokenizer_can_handle_single_option()
        {
            var option = new CliOption<string>("--hello");
            var command = new CliRootCommand { option };
            IReadOnlyList<string> args = ["--hello", "world"];
            List<CliToken> tokens = null;
            List<string> errors = null;
            Tokenizer.Tokenize(args, command, new CliConfiguration(command), true, out tokens, out errors);
            Tokenizer.Tokenize(args, command, new CliConfiguration(command), true, out tokens, out errors);

            tokens
                .Skip(1)
                .Select(t => t.Value)
                .Should()
                .BeEquivalentTo("--hello", "world");

            errors.Should().BeNull();
        }

        [Fact]
        public void Location_stack_ToString_is_correct()
        {
            var option = new CliOption<string>("--hello");
            var command = new CliRootCommand { option };
            IReadOnlyList<string> args = ["--hello", "world"];
            List<CliToken> tokens = null;
            List<string> errors = null;

            Tokenizer.Tokenize(args,
                               command,
                               new CliConfiguration(command),
                               true,
                               out tokens,
                               out errors);

            var locations = tokens
                            .Skip(1)
                            .Select(t => t.Location.ToString())
                            .ToList();
            errors.Should().BeNull();
            tokens.Count.Should().Be(3); 
            locations.Count.Should().Be(2);
            locations[0].Should().Be("testhost from User[-1, 8, 0]; --hello from User[0, 7, 0]");
            locations[1].Should().Be("testhost from User[-1, 8, 0]; world from User[1, 5, 0]");
        }

        [Fact]
        public void Directives_are_skipped()
        {
            var option = new CliOption<string>("--hello");
            var command = new CliRootCommand { option };
            var configuration = new CliConfiguration(command);
            configuration.AddPreprocessedLocation(new Location("[diagram]", Location.User, 0, null));
            IReadOnlyList<string> args = ["[diagram] --hello", "world"];

            List<CliToken> tokens = null;
            List<string> errors = null;

            Tokenizer.Tokenize(args,
                               command,
                               new CliConfiguration(command),
                               true,
                               out tokens,
                               out errors);

            var hasDiagram = tokens
                            .Any(t => t.Value == "[diagram]");
            errors.Should().BeNull();
            tokens.Count.Should().Be(3); // root is a token
            hasDiagram .Should().BeFalse();
        }
    }
}
