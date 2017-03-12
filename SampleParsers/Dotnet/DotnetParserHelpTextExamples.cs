// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Xunit;
using Xunit.Abstractions;
using static Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet.Create;

namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet
{
    public class DotnetParserHelpTextExamples
    {
        private readonly ITestOutputHelper output;

        private readonly Command dotnet = DotnetCommand();

        public DotnetParserHelpTextExamples(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory(Skip="Work in progress")]
        [InlineData("dotnet -h")]
        [InlineData("dotnet add -h")]
        [InlineData("dotnet add package -h")]
        [InlineData("dotnet add reference -h")]
        [InlineData("dotnet new -h")]
        [InlineData("dotnet restore -h")]
        [InlineData("dotnet build -h")]
        [InlineData("dotnet publish -h")]
        [InlineData("dotnet run -h")]
        [InlineData("dotnet test -h")]
        [InlineData("dotnet pack -h")]
        [InlineData("dotnet migrate -h")]
        [InlineData("dotnet clean -h")]
        [InlineData("dotnet sln -h")]
        [InlineData("dotnet sln add -h")]
        [InlineData("dotnet sln list -h")]
        [InlineData("dotnet sln remove -h")]
        [InlineData("dotnet remove -h")]
        [InlineData("dotnet list -h")]
        [InlineData("dotnet nuget -h")]
        [InlineData("dotnet msbuild -h")]
        [InlineData("dotnet vstest -h")]
        public void HelpText(string commandLine)
        {
            var result = dotnet.Parse(commandLine);

            output.WriteLine("DIAGRAM");
            output.WriteLine(result.Diagram() + Environment.NewLine);

            output.WriteLine("HELP");
            output.WriteLine(result.Command().HelpView());
        }
    }
}