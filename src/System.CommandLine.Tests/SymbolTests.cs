﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
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
            var symbol = CreateSymbol("--zzzzzz");

            symbol.Name = "bbb";

            symbol.Name.Should().Be("bbb");
        }

        protected abstract Symbol CreateSymbol(string name);
    }
}
