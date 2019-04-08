// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Benchmarks.Helpers;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace System.CommandLine.Benchmarks.CommandLine
{
    /// <summary>
    /// Measures the performance directives parsing.
    /// </summary>
    [BenchmarkCategory(Categories.CommandLine)]
    public class Perf_Parser_ParseResult
    {
        private readonly Parser _testParser;

        public Perf_Parser_ParseResult()
        {
            var option = new Option("-opt");

            _testParser =
                new CommandLineBuilder()
                    .AddOption(option)
                    .UseParseDirective()
                    .Build();
        }

        public IEnumerable<object> GenerateTestParseResults()
            => new[]
            {
                "[directive1] -opt",
                "[directive1] [directive2] -opt",
                "[directive1:1] [directive2:2] -opt",
                "[directive1] [directive2] [directive2] -opt",
                "[directive1:1] [directive2:2] [directive2:3] -opt",
            }
            .Select(cmd => new BdnParam<ParseResult>(_testParser.Parse(cmd), cmd));

        [Benchmark]
        [ArgumentsSource(nameof(GenerateTestParseResults))]
        public IDirectiveCollection ParseResult_Directives(BdnParam<ParseResult> parseResult)
            => parseResult.Value.Directives;

        [Benchmark]
        [ArgumentsSource(nameof(GenerateTestParseResults))]
        public string ParseResult_Diagram(BdnParam<ParseResult> parseResult)
            => parseResult.Value.Diagram();
    }
}
