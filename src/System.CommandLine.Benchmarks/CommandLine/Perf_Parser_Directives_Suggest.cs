// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Benchmarks.Helpers;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace System.CommandLine.Benchmarks.CommandLine
{
    /// <summary>
    /// Measures the performance of [suggest] directive.
    /// </summary>
    [BenchmarkCategory(Categories.CommandLine)]
    public class Perf_Parser_Directives_Suggest
    {
        private NullConsole _nullConsole;
        private Parser _testParser;

        [GlobalSetup]
        public void Setup()
        {
            _nullConsole = new NullConsole();

            var eatCommand = new Command("eat")
            {
                new Option<string>("--fruit").AddCompletions("apple", "banana", "cherry"),
                new Option<string>("--vegetable").AddCompletions("asparagus", "broccoli", "carrot")
            };

            _testParser = new CommandLineBuilder(eatCommand)
                .UseSuggestDirective()
                .Build();
        }

        [Params(
          "[suggest:4] \"eat\"",
          "[suggest:13] \"eat --fruit\""
        )]
        public string TestCmdArgs;

        [Benchmark]
        public async Task InvokeSuggest()
            => await _testParser.InvokeAsync(TestCmdArgs, _nullConsole);

    }
}
