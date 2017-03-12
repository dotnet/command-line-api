// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;
using static Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet.Create;

namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet
{
    public class SuggestionTests
    {
        [Fact]
        public void dotnet_add()
        {
            var result = DotnetCommand().Parse("dotnet add ");
            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("reference", "package", "-h", "--help");
        }

        [Fact]
        public void dotnet_sln_add()
        {
            var command = DotnetCommand();

            var result = command.Parse("dotnet sln add ");

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("-h", "--help");
        }
    }
}