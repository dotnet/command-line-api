// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Benchmarks.Helpers;
using System.CommandLine.Parsing;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace System.CommandLine.Benchmarks.CommandLine
{
    internal enum Kind { Default, Parseable, SpanParseable }

    /// <summary>
    /// Measures the performance of <see cref="CliParser"/> when parsing options with arguments.
    /// </summary>
    [BenchmarkCategory(Categories.CommandLine)]
    public class Perf_Parser_Options_With_Parsed_Arguments_Variants
    {
        private string _testSymbolsAsString;
        private Command _defaultCommand;
        private RootCommand _parseableCommand;
        private RootCommand _spanParseableCommand;

        private IEnumerable<Option> GenerateTestOptions(int count, Kind kind)
            => Enumerable.Range(0, count)
                         .Select(i =>
                            kind switch
                            {
                                Kind.Default => new Option<DateOnly>($"-option{i}")
                                {
                                    Arity = ArgumentArity.ExactlyOne,
                                    Description = $"Description for -option {i} ...."
                                },
                                Kind.Parseable => new ParsableOption<DateOnly>($"-option{i}")
                                {
                                    Arity = ArgumentArity.ExactlyOne,
                                    Description = $"Description for -option {i} ...."
                                },
                                Kind.SpanParseable => new SpanParsableOption<DateOnly>($"-option{i}")
                            }
                         );

        /// <remarks>
        /// For optionsCount: 5, argumentsCount: 5 will return:
        /// -option0 arg0..arg4 -option1 arg0..arg4 ... -option4 arg0..arg4
        /// </remarks>
        private string GenerateTestOptionsWithArgumentsAsStringExpr(int optionsCount)
        {
            return Enumerable
                .Range(0, optionsCount)
                .Select(i => $"-option{i} 2022/02/02 ")
                .Aggregate("", (ac, next) => ac + " " + next);
        }

        [Params(1, 5, 20, 50, 100)]
        public int TestOptionsCount;

        [GlobalSetup]
        public void SetupParserFromOptionsWithArguments_Parse()
        {
            _defaultCommand = GenerateTestOptions(TestOptionsCount, Kind.Default).ToArray().CreateConfiguration();
            _parseableCommand = GenerateTestOptions(TestOptionsCount, Kind.Parseable).ToArray().CreateConfiguration();
            _spanParseableCommand = GenerateTestOptions(TestOptionsCount, Kind.SpanParseable).ToArray().CreateConfiguration();
            _testSymbolsAsString = GenerateTestOptionsWithArgumentsAsStringExpr(TestOptionsCount);
        }

        [Benchmark]
        public ParseResult Default() => _defaultCommand.Parse(_testSymbolsAsString);

        [Benchmark]
        public ParseResult Parseable() => _parseableCommand.Parse(_testSymbolsAsString);

        [Benchmark]
        public ParseResult SpanParseable() => _spanParseableCommand.Parse(_testSymbolsAsString);
    }
}
