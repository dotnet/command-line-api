﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Benchmarks.Helpers;
using System.CommandLine.Parsing;
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
        private readonly CommandLineConfiguration _configuration;

        public Perf_Parser_ParseResult()
        {
            var option = new Option<bool>("-opt");

            _configuration = new CommandLineConfiguration(new RootCommand { option })
            {
                Directives = { new ParseDirective() }
            };
        }

        public IEnumerable<string> GenerateTestInputs() 
            => new[]
            {
                "[directive1] -opt",
                "[directive1] [directive2] -opt",
                "[directive1:1] [directive2:2] -opt",
                "[directive1] [directive2] [directive2] -opt",
                "[directive1:1] [directive2:2] [directive2:3] -opt",
            };

        public IEnumerable<object> GenerateTestParseResults()
            => GenerateTestInputs()
               .Select(input => new BdnParam<ParseResult>(_configuration.Parse(input), input));

        [Benchmark]
        [ArgumentsSource(nameof(GenerateTestInputs))]
        public ParseResult ParseResult_Directives(string input)
            => _configuration.Parse(input);

        [Benchmark]
        [ArgumentsSource(nameof(GenerateTestParseResults))]
        public string ParseResult_Diagram(BdnParam<ParseResult> parseResult)
            => parseResult.Value.Diagram();
    }
}
