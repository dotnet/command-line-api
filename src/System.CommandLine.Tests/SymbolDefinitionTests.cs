// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    internal class SymbolDefinitionTest : SymbolDefinition
    {
        internal SymbolDefinitionTest(IReadOnlyCollection<string> aliases, string description, ArgumentDefinition argDef = null)
            : base(aliases, description, argDef)
        {
        }
    }

    public class SymbolDefinitionTests
    {
        [Fact]
        public void SymbolDefinition_WithNullAliases_ThrowsArgumentNullException()
        {
            Action result = () => new SymbolDefinitionTest(null, null);
            result.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void SymbolDefinition_WithNoAliases_ThrowsArgumentException()
        {
            var mockAliases = new string[] {};
            Action result = () => new SymbolDefinitionTest(mockAliases, null);
            result.Should().Throw<ArgumentException>()
                .WithMessage("An option must have at least one alias.");
        }

        [Fact]
        public void SymbolDefinition_WithWhitespaceAlias_ThrowsArgumentException()
        {
            var mockAliases = new [] {""};
            Action result = () => new SymbolDefinitionTest(mockAliases, null);

            result.Should().Throw<ArgumentException>()
                .WithMessage("An option alias cannot be null, empty, or consist entirely of whitespace.");

            mockAliases = new [] {"test", ""};
            result.Should().Throw<ArgumentException>()
                .WithMessage("An option alias cannot be null, empty, or consist entirely of whitespace.");
        }

        [Fact]
        public void SymbolDefinition_WithAliasWithOnlyPrefix_ThrowsArgumentException()
        {
            var mockAliases = new [] {"--"};
            Action result = () => new SymbolDefinitionTest(mockAliases, null);
            result.Should().Throw<ArgumentException>()
                .WithMessage("An option alias cannot be null, empty, or consist entirely of whitespace.");

            mockAliases = new [] {"--test", "--"};
            result.Should().Throw<ArgumentException>()
                .WithMessage("An option alias cannot be null, empty, or consist entirely of whitespace.");
        }
    }
}
