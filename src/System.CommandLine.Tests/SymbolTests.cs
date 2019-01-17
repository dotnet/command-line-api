// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class SymbolTests
    {
        [Fact]
        public void Symbol_WithNullAliases_ThrowsArgumentNullException()
        {
            Action result = () => new TestSymbol(null, null);
            result.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Symbol_WithNoAliases_ThrowsArgumentException()
        {
            var mockAliases = new string[] { };
            Action result = () => new TestSymbol(mockAliases, null);
            result.Should().Throw<ArgumentException>()
                  .WithMessage("An option must have at least one alias.");
        }

        [Fact]
        public void Symbol_WithWhitespaceAlias_ThrowsArgumentException()
        {
            var mockAliases = new[] { "" };
            Action result = () => new TestSymbol(mockAliases, null);

            result.Should().Throw<ArgumentException>()
                  .WithMessage("An alias cannot be null, empty, or consist entirely of whitespace.");

            mockAliases = new[] { "test", "" };
            result.Should().Throw<ArgumentException>()
                  .WithMessage("An alias cannot be null, empty, or consist entirely of whitespace.");
        }

        [Fact]
        public void Symbol_WithAliasWithOnlyPrefix_ThrowsArgumentException()
        {
            var mockAliases = new[] { "--" };
            Action result = () => new TestSymbol(mockAliases, null);
            result.Should().Throw<ArgumentException>()
                  .WithMessage("An alias cannot be null, empty, or consist entirely of whitespace.");

            mockAliases = new[] { "--test", "--" };
            result.Should().Throw<ArgumentException>()
                  .WithMessage("An alias cannot be null, empty, or consist entirely of whitespace.");
        }

        [Fact]
        public void When_Name_is_explicitly_set_then_adding_aliases_does_not_change_it()
        {
            var symbol = new TestSymbol(new[] { "-a", "--zzzzzz" }, "");

            symbol.Name = "bbb";

            symbol.Name.Should().Be("bbb");
        }

        [Theory]
        [InlineData("--bbb")]
        [InlineData("/bbb")]
        public void Name_cannot_be_prefixed(string name)
        {
            var symbol = new TestSymbol(new[] { "-a" }, "");

            symbol.Invoking(s => s.Name = name)
                  .Should()
                  .Throw<ArgumentException>()
                  .WithMessage($"Property {typeof(TestSymbol).Name}.Name cannot have a prefix.");
        }

        [Fact]
        public void Symbol_defaults_argument_to_alias_name_when_it_is_not_provided()
        {
            var symbol = new TestSymbol(new[] { "-alias" }, "", new Argument() { Arity = ArgumentArity.ZeroOrOne });

            symbol.Argument.Name.Should().Be("ALIAS");
        }

        [Fact]
        public void Symbol_retains_argument_name_when_it_is_provided()
        {
            var symbol = new TestSymbol(new[] { "-alias" }, "", new Argument() { Name = "arg", Arity = ArgumentArity.ZeroOrOne });

            symbol.Argument.Name.Should().Be("arg");
        }

        [Fact]
        public void Symbol_does_not_default_argument_name_when_arity_is_zero()
        {
            var symbol = new TestSymbol(new[] { "-alias" }, "", new Argument());

            symbol.Argument.Name.Should().BeNull();
        }

        private class TestSymbol : Symbol
        {
            internal TestSymbol(IReadOnlyCollection<string> aliases, string description, Argument argDef = null)
                : base(aliases, description, argDef)
            {
            }
        }
    }
}
