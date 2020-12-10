// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public abstract class IdentifierSymbolTests : SymbolTests
    {
        [Fact]
        public void When_Name_is_changed_then_old_name_is_not_among_aliases()
        {
            var symbol = (IdentifierSymbol) CreateSymbol("original");

            symbol.Name = "changed";

            symbol.HasAlias("original").Should().BeFalse();
            symbol.Aliases.Should().NotContain("original");
            symbol.Aliases.Should().NotContain("original");
        }
    }
}