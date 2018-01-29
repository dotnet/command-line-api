// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using static System.Console;
using static Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet.DotNetParser;

namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet
{
    public class AddReferenceTests
    {
        private readonly ITestOutputHelper output;

        public AddReferenceTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void dotnet_add_reference_correctly_assigns_arguments_to_subcommands()
        {
            var result = Instance.Parse("dotnet add foo.csproj reference bar1.csproj bar2.csproj");

            WriteLine(result.Diagram());

            result["dotnet"]["add"]
                .Arguments
                .Should()
                .BeEquivalentTo("foo.csproj");

            result["dotnet"]["add"]["reference"]
                .Arguments
                .Should()
                .BeEquivalentTo("bar1.csproj", "bar2.csproj");
        }
    }
}