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
    /// Measures the performance of <see cref="CliParser"/> when parsing options with arguments.
    /// </summary>
    [BenchmarkCategory(Categories.CommandLine)]
    public class Perf_Parser_Options_With_Parsed_Arguments
    {
        private string _testSymbolsAsString;
        private Command _command;

        private IEnumerable<Option> GenerateTestOptions(int count)
            => Enumerable.Range(0, count)
                         .Select(i => new Option<DateOnly>($"-option{i}")
                             {
                                 Arity = ArgumentArity.ExactlyOne,
                                 Description = $"Description for -option {i} ...."
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

        [GlobalSetup(Target = nameof(ParserFromOptionsWithArguments_Parse))]
        public void SetupParserFromOptionsWithArguments_Parse()
        {
            var testSymbolsArr = GenerateTestOptions(TestOptionsCount).ToArray();
            _command = testSymbolsArr.CreateConfiguration();
            _testSymbolsAsString = GenerateTestOptionsWithArgumentsAsStringExpr(testSymbolsArr.Length);
        }

        [Benchmark]
        public ParseResult ParserFromOptionsWithArguments_Parse() => _command.Parse(_testSymbolsAsString);
    }
}
