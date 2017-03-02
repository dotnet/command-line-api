using FluentAssertions;
using Xunit;

namespace CommandLine.Tests
{
    public class TokenizeTests
    {
        [Fact]
        public void Tokenize_splits_strings_based_on_whitespace()
        {
            var commandLine = "one two\tthree   four ";

            commandLine.Tokenize()
                       .Should()
                       .BeEquivalentTo("one", "two", "three", "four");
        }

        [Fact]
        public void Tokenize_does_not_break_up_double_quote_delimited_values()
        {
            var commandLine = @"rm -r ""c:\temp files\""";

            commandLine.Tokenize()
                       .Should()
                       .BeEquivalentTo("rm", "-r", @"c:\temp files\");
        }
    }
}