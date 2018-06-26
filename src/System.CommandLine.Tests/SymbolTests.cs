// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class SymbolTests
    {
        [Fact]
        public void Symbol_WithNullAliases_ThrowsArgumentNullException()
        {
            Action result = () => new SymbolTest(null, null);
            result.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Symbol_WithNoAliases_ThrowsArgumentException()
        {
            var mockAliases = new string[] {};
            Action result = () => new SymbolTest(mockAliases, null);
            result.Should().Throw<ArgumentException>()
                .WithMessage("An option must have at least one alias.");
        }

        [Fact]
        public void Symbol_WithWhitespaceAlias_ThrowsArgumentException()
        {
            var mockAliases = new [] {""};
            Action result = () => new SymbolTest(mockAliases, null);

            result.Should().Throw<ArgumentException>()
                .WithMessage("An option alias cannot be null, empty, or consist entirely of whitespace.");

            mockAliases = new [] {"test", ""};
            result.Should().Throw<ArgumentException>()
                .WithMessage("An option alias cannot be null, empty, or consist entirely of whitespace.");
        }

        [Fact]
        public void Symbol_WithAliasWithOnlyPrefix_ThrowsArgumentException()
        {
            var mockAliases = new [] {"--"};
            Action result = () => new SymbolTest(mockAliases, null);
            result.Should().Throw<ArgumentException>()
                .WithMessage("An option alias cannot be null, empty, or consist entirely of whitespace.");

            mockAliases = new [] {"--test", "--"};
            result.Should().Throw<ArgumentException>()
                .WithMessage("An option alias cannot be null, empty, or consist entirely of whitespace.");
        }
    }
}
