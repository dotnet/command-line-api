// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;
using static System.Console;
using static Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet.Create;

namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet
{
    public class AddReferenceTests
    {
        [Fact]
        public void dotnet_add_reference_correctly_assigns_arguments_to_subcommands()
        {
            var result = DotnetCommand().Parse("dotnet add foo.csproj reference bar1.csproj bar2.csproj");

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