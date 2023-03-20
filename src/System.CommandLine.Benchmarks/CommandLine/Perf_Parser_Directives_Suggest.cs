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
        private CommandLineConfiguration _configuration;

        [GlobalSetup]
        public void Setup()
        {
            Option<string> fruitOption = new("--fruit");
            fruitOption.CompletionSources.Add("apple", "banana", "cherry");

            Option<string> vegetableOption = new("--vegetable");
            vegetableOption.CompletionSources.Add("asparagus", "broccoli", "carrot");

            var eatCommand = new Command("eat")
            {
                fruitOption,
                vegetableOption
            };

            _configuration = new CommandLineConfiguration(eatCommand)
            {
                Directives = { new SuggestDirective() },
                Out = System.IO.TextWriter.Null
            };
        }

        [Params(
          "[suggest:4] \"eat\"",
          "[suggest:13] \"eat --fruit\""
        )]
        public string TestCmdArgs;

        [Benchmark]
        public Task InvokeSuggest()
            => _configuration.InvokeAsync(TestCmdArgs);

    }
}
