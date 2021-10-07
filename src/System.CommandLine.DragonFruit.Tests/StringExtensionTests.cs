// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;

namespace System.CommandLine.DragonFruit.Tests
{
    public class StringExtensionTests
    {
        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("Option123", "option123")]
        [InlineData("dWORD", "d-word")]
        [InlineData("MSBuild", "msbuild")]
        [InlineData("NoEdit", "no-edit")]
        [InlineData("SetUpstreamBranch", "set-upstream-branch")]
        [InlineData("lowerCaseFirst", "lower-case-first")]
        [InlineData("_field", "field")]
        [InlineData("__field", "field")]
        [InlineData("___field", "field")]
        [InlineData("m_field", "m-field")]
        [InlineData("m_Field", "m-field")]
        public void ToKebabCase(string input, string expected) => input.ToKebabCase().Should().Be(expected);
    }
}
