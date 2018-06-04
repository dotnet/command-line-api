// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests
{
    public class ParseDirectiveTests
    {
        private readonly ITestOutputHelper output;

        public ParseDirectiveTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task Parse_directive_writes_parse_diagram()
        {
            var parser = new CommandLineBuilder()
                         .AddCommand("the-command", "",
                                     cmd => cmd.AddOption(new[] { "-c", "--count" }, "",
                                                          args => args.ParseArgumentsAs<int>()))
                         .UseParseDirective()
                         .Build();

            var result = parser.Parse("!parse the-command -c 34 --nonexistent wat");

            output.WriteLine(result.Diagram());

            var console = new TestConsole();

            await parser.InvokeAsync(result, console);

            console.Out
                   .ToString()
                   .Should()
                   .Be($"[ {CommandLineBuilder.ExeName} [ the-command [ -c <34> ] ] ]   ???--> --nonexistent wat" + Environment.NewLine);
        }
    }
}
