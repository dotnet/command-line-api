// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class ArgumentTests : SymbolTests
    {
        [Fact]
        public void By_default_there_is_no_default_value()
        {
            var argument = new Argument();

            argument.HasDefaultValue.Should().BeFalse();
        }

        [Fact]
        public void When_default_value_is_set_to_null_then_HasDefaultValue_is_true()
        {
            var argument = new Argument();

            argument.SetDefaultValue(null);

            argument.HasDefaultValue.Should().BeTrue();
        }

        [Fact]
        public void When_default_value_factory_is_set_then_HasDefaultValue_is_true()
        {
            var argument = new Argument();

            argument.SetDefaultValueFactory(() => null);

            argument.HasDefaultValue.Should().BeTrue();
        }

        [Fact]
        public void When_there_is_no_default_value_then_GetDefaultValue_throws()
        {
            var argument = new Argument<string>("the-arg");

            argument.Invoking(a => a.GetDefaultValue())
                    .Should()
                    .Throw<InvalidOperationException>()
                    .Which
                    .Message
                    .Should()
                    .Be("Argument \"the-arg\" does not have a default value");
        }

        protected override Symbol CreateSymbol(string name)
        {
            return new Argument(name);
        }
    }
}