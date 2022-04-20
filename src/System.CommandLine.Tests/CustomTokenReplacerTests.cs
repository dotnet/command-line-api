// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests;

public class CustomTokenReplacerTests
{
    [Fact]
    public void Custom_token_replacer_can_expand_argument_values()
    {
        var argument = new Argument<int>();

        var command = new RootCommand { argument };

        var parser = new CommandLineBuilder(command)
                     .UseTokenReplacer((string tokenToReplace, out IReadOnlyList<string> tokens, out string message) =>
                     {
                         tokens = new[] { "123" };
                         message = null;
                         return true;
                     })
                     .Build();

        var result = parser.Parse("@interpolate-me");

        result.Errors.Should().BeEmpty();

        result.GetValueForArgument(argument).Should().Be(123);
    }
}