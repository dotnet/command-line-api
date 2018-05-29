// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using static System.Environment;

namespace System.CommandLine.Tests
{
    public class SuggestDirectiveTests
    {
        [Fact]
        public async Task Suggest_directive_writes_suggestions()
        {
            var parser = new CommandLineBuilder()
                         .AddCommand("eat", "",
                                     cmd => cmd.AddOption(new[] { "--fruit" }, "",
                                                          args => args.AddSuggestions("apple", "banana", "cherry")))
                         .UseSuggestDirective()
                         .Build();

            var result = parser.Parse("!suggest eat --fruit ");
            
            var console = new TestConsole();

            await parser.InvokeAsync(result, console);

            console.Out
                   .ToString()
                   .Should()
                   .Be($"apple{NewLine}banana{NewLine}cherry{NewLine}");
        }
    }
}
