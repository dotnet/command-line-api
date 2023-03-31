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
    /// Measures the performance of <see cref="CliParser"/> when parsing options without arguments.
    /// </summary>
    [BenchmarkCategory(Categories.CommandLine)]
    public class Perf_Parser_Options_Bare
    {
        private IEnumerable<CliOption> _testSymbols;
        private string _testSymbolsAsString;
        private CliConfiguration _testConfiguration;

        private IEnumerable<CliOption> GenerateTestOptions(int count, ArgumentArity arity)
            => Enumerable.Range(0, count)
                         .Select(i =>
                                     new CliOption<string>($"-option{i}")
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
        public CliConfiguration ParserFromOptions_Ctor()
        {
            return _testSymbols.CreateConfiguration();
        }

        [GlobalSetup(Target = nameof(ParserFromOptions_Parse))]
        public void SetupParserFromOptions_Parse()
        {
            var testSymbolsArr = GenerateTestOptions(TestSymbolsCount, ArgumentArity.Zero).ToArray();
            _testConfiguration = testSymbolsArr.CreateConfiguration();
            _testSymbolsAsString = GenerateTestOptionsAsStringExpr(testSymbolsArr.Length);
        }

        [Benchmark]
        public ParseResult ParserFromOptions_Parse() => _testConfiguration.Parse(_testSymbolsAsString);
    }
}
