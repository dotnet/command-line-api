// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Benchmarks.Helpers;
using System.CommandLine.Parsing;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace System.CommandLine.Benchmarks.CommandLine
{
    /// <summary>
    /// Measures the performance of <see cref="Parser"/> when parsing options without arguments.
    /// </summary>
    [BenchmarkCategory(Categories.CommandLine)]
    public class Perf_Parser_Options_Bare
    {
        private IEnumerable<Option> _testSymbols;
        private string _testSymbolsAsString;
        private Parser _testParser;

        private IEnumerable<Option> GenerateTestOptions(int count, ArgumentArity arity)
            => Enumerable.Range(0, count)
                         .Select(i =>
                                     new Option<string>($"-option{i}")
                                     {
                                         Arity = arity,
                                         Description = $"Description for -option {i} ...."
                                     }
                         );

        /// <remarks>
        /// count=1  : cmd-root -option0
        /// count=5  : cmd-root -option0 -option1 ... -option4
        /// count=20 : cmd-root -option0 -option1 ... -option19
        /// </remarks>
        private string GenerateTestOptionsAsStringExpr(int count)
            => Enumerable.Range(0, count)
                         .Select(i => $"-option{i}")
                         .Aggregate("", (ac, next) => ac + " " + next);

        [Params(1, 5, 20)]
        public int TestSymbolsCount;

        [GlobalSetup(Target = nameof(ParserFromOptions_Ctor))]
        public void SetupTestOptions()
        {
            _testSymbols = GenerateTestOptions(TestSymbolsCount, ArgumentArity.Zero);
        }

        [Benchmark]
        public Parser ParserFromOptions_Ctor()
        {
            return _testSymbols.CreateParser();
        }

        [GlobalSetup(Target = nameof(ParserFromOptions_Parse))]
        public void SetupParserFromOptions_Parse()
        {
            var testSymbolsArr = GenerateTestOptions(TestSymbolsCount, ArgumentArity.Zero).ToArray();
            _testParser = testSymbolsArr.CreateParser();
            _testSymbolsAsString = GenerateTestOptionsAsStringExpr(testSymbolsArr.Length);
        }

        [Benchmark]
        public ParseResult ParserFromOptions_Parse() => _testParser.Parse(_testSymbolsAsString);
    }
}
