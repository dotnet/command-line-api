﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace System.CommandLine.Benchmarks.CommandLine
{
    /// <summary>
    /// Measures the performance of <see cref="System.CommandLine.Parser"/> when parsing options with arguments.
    /// </summary>
    [BenchmarkCategory(Categories.CommandLine)]
    public class Perf_Parser_Options_With_Arguments
    {
        private string _testSymbolsAsString;
        private Parser _testParser;

        private IEnumerable<Option> GenerateTestOptions(int count, IArgumentArity arity)
            => Enumerable.Range(0, count)
                         .Select(i => new Option(
                             $"-option{i}",
                             $"Description for -option {i} ....",
                             new Argument { Arity = arity }
                         ));

        /// <remarks>
        /// For optionsCount: 5, argumentsCount: 5 will return:
        /// -option0 arg0..arg4 -option1 arg0..arg4 ... -option4 arg0..arg4
        /// </remarks>
        private string GenerateTestOptionsWithArgumentsAsStringExpr(int optionsCount, int argumentsCount)
        {
            var arguments = Enumerable
                .Range(0, argumentsCount)
                .Select(i => $"arg{i}")
                .Aggregate("", (ac, next) => ac + " " + next);

            return Enumerable
                .Range(0, optionsCount)
                .Select(i => $"-option{i} {arguments} ")
                .Aggregate("", (ac, next) => ac + " " + next);
        }

        [Params(1, 5, 20)]
        public int TestOptionsCount;

        [Params(1, 5, 20)]
        public int TestArgumentsCount;

        [GlobalSetup(Target = nameof(ParserFromOptionsWithArguments_Parse))]
        public void SetupParserFromOptionsWithArguments_Parse()
        {
            var testSymbolsArr = GenerateTestOptions(TestOptionsCount, ArgumentArity.OneOrMore).ToArray();
            _testParser = new Parser(testSymbolsArr);
            _testSymbolsAsString = GenerateTestOptionsWithArgumentsAsStringExpr(testSymbolsArr.Length, TestArgumentsCount);
        }

        [Benchmark]
        public ParseResult ParserFromOptionsWithArguments_Parse() => _testParser.Parse(_testSymbolsAsString);
    }
}
