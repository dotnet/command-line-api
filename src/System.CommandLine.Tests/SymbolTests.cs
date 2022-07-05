// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public abstract class SymbolTests
    {
        [Fact]
        public void When_Name_is_explicitly_set_then_adding_aliases_does_not_change_it()
        {
            var symbol = CreateSymbol("original");

            symbol.Name = "changed";

            symbol.Name.Should().Be("changed");
        }

        [Fact]
        public void Parse_extension_method_reuses_implicit_parser_instance()
        {
            var symbol = CreateSymbol("x");

            Func<ParseResult> parse = () => symbol switch
            {
                Argument argument => argument.Parse(""),
                Command command => command.Parse(""),
                Option option => option.Parse(""),
                _ => throw new ArgumentOutOfRangeException(nameof(symbol))
            };

            var parser1 = parse().Parser;
            var parser2 = parse().Parser;

            parser1.Should().BeSameAs(parser2);
        }

        protected abstract Symbol CreateSymbol(string name);
    }
}