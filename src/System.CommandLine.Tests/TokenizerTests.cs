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
        public void The_tokenizer_is_accessible()
        {
            var option = new CliOption<string>("--hello");
            var command = new CliRootCommand { option };
            IReadOnlyList<string> args = ["--hello", "world"];
            List<CliToken> tokens = null;
            List<string> errors = null;
            CliTokenizer.Tokenize(args,command,false, true, out tokens, out errors);

            tokens
                .Skip(1)
                .Select(t => t.Value)
                .Should()
                .BeEquivalentTo("--hello", "world");

            errors.Should().BeNull();
        }
    }
}
