// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using BenchmarkDotNet.Attributes;

namespace System.CommandLine.Benchmarks.CommandLine
{
    /// <summary>
    /// Measures the performance directives parsing.
    /// </summary>
    [BenchmarkCategory(Categories.CommandLine)]
    public class Perf_Parser_Directives
    {
        private Parser _testParser;

        [GlobalSetup]
        public void Setup()
        {
            var option = new Option("-opt");

            _testParser =
                new CommandLineBuilder()
                    .AddOption(option)
                    .UseParseDirective()
                    .Build();
        }

        [Params(
            "[directive1] -opt",
            "[directive1] [directive2] -opt",
            "[directive1:1] [directive2:2] -opt",
            "[directive1] [directive2] [directive2] -opt",
            "[directive1:1] [directive2:2] [directive2:3] -opt"
        )]
        public string TestCmdArgs;

        [Benchmark]
        public IDirectiveCollection ParseDirectives()
            => _testParser.Parse(TestCmdArgs).Directives;

        [Benchmark]
        public string ParseDirectives_Diagram()
            => _testParser.Parse(TestCmdArgs).Diagram();
    }
}
