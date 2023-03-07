using FluentAssertions;
using System.CommandLine.Parsing;
using Xunit;

namespace System.CommandLine.Tests
{
    public class TokenTests
    {
        [Fact]
        public void Tokens_are_equal_when_they_use_same_value_type_and_symbol()
        {
            Option<int> count = new ("--count");
            Token token = new (count.Name, TokenType.Option, count);
            Token same = new (count.Name, TokenType.Option, count);

            token.Equals(same).Should().BeTrue();
            token.Equals((object)same).Should().BeTrue();
        }

        [Fact]
        public void Tokens_are_not_equal_when_they_do_not_use_same_value_type_and_symbol()
        {
            Option<int> symbol = new("--count");
            Option<int> symbolWithSameName = new("--count");

            Token token = new(symbol.Name, TokenType.Option, symbol);
            Token differentValue = new("different", TokenType.Option, symbol);
            Token differentType = new(symbol.Name, TokenType.Argument, symbol);
            Token differentSymbol = new(symbol.Name, TokenType.Option, symbolWithSameName);

            Assert(token, differentValue);
            Assert(token, differentType);
            Assert(token, differentSymbol);
            Assert(token, null);

            static void Assert(Token token, Token different)
            {
                token.Equals(different).Should().BeFalse();
                token.Equals((object)different).Should().BeFalse();
            }
        }

        [Fact]
        public void Symbol_property_returns_value_provided_in_constructor()
        {
            Option<int> symbol = new("--count");

            Token token = new(symbol.Name, TokenType.Option, symbol);

            token.Symbol.Should().Be(symbol);
        }
    }
}
