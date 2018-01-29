// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using FluentAssertions;
using Xunit;

namespace Microsoft.DotNet.Cli.CommandLine.Tests
{
    public class CommandExecutionTests
    {
        [Fact]
        public void When_there_are_parse_errors_then_result_code_is_1()
        {
            var result = new Command("do-something", "").Parse("oops");

            result.Execute().Code.Should().Be(1);
        }

        [Fact]
        public void When_there_are_no_parse_errors_then_result_code_is_0()
        {
            var result = new Command("do-something", "").Parse("do-something");

            result.Execute().Code.Should().Be(0);
        }
    }
}