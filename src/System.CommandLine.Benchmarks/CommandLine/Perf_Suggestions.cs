// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Completions;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace System.CommandLine.Benchmarks.CommandLine
{
    /// <summary>
    /// Measures the performance of suggestions.
    /// </summary>
    [BenchmarkCategory(Categories.CommandLine)]
    public class Perf_Suggestions
    {
        private CliOption _testSymbol;
        private ParseResult _testParseResult;

        /// <remarks>
        /// count=1  : { "suggestion0" }
        /// count=5  : { "suggestion0", .., "suggestion5" }
        /// </remarks>
        private string[] GenerateSuggestionsArray(int count)
            => Enumerable.Range(0, count)
                         .Select(i => $"suggestion{i}")
                         .ToArray();

        private IEnumerable<CliOption> GenerateOptionsArray(int count)
            => Enumerable.Range(0, count)
                         .Select(i => new CliOption<string>($"suggestion{i}"));

        [Params(1, 5, 20, 100)]
        public int TestSuggestionsCount;

        [GlobalSetup(Target = nameof(SuggestionsFromSymbol))]
        public void Setup_FromSymbol()
        {
            _testSymbol = new CliOption<string>("--hello");
            _testSymbol.CompletionSources.Add(GenerateSuggestionsArray(TestSuggestionsCount));
        }

        [Benchmark]
        public void SuggestionsFromSymbol()
        {
            _testSymbol.GetCompletions(CompletionContext.Empty).Consume(new Consumer());
        }

        [GlobalSetup(Target = nameof(SuggestionsFromParseResult))]
        public void Setup_FromParseResult()
        {
            var testCommand = new CliCommand("command");

            foreach (var option in GenerateOptionsArray(TestSuggestionsCount))
            {
                testCommand.Options.Add(option);
            }

            _testParseResult = testCommand.Parse("--wrong");
        }

        [Benchmark]
        public void SuggestionsFromParseResult()
        {
            _testParseResult.GetCompletions("--wrong".Length + 1).Consume(new Consumer());
        }
    }
}
