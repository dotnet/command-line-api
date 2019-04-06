// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Benchmarks.Helpers;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace System.CommandLine.Benchmarks.CommandLine
{
    /// <summary>
    /// Measures the performance of typo correction when using parser.
    /// </summary>
    [BenchmarkCategory(Categories.CommandLine)]
    public class Perf_Parser_TypoCorrection
    {
        private readonly NullConsole _nullConsole = new NullConsole();
        private Parser _testParser;

        [GlobalSetup]
        public void Setup()
        {
            var option = new Option("--0123456789");

            _testParser =
                new CommandLineBuilder()
                    .AddOption(option)
                    .UseTypoCorrections()
                    .Build();
        }

        [Benchmark]
        [Arguments("--0123456789", "equal")]
        [Arguments("--01234567x9", "1s")]
        [Arguments("--0x234567y9", "2s")]
        [Arguments("--0x234z67y9", "3s")]
        [Arguments("--0x234z67yw", "4s")]
        [Arguments("--01x23456789", "1i")]
        [Arguments("--01x234y56789", "2i")]
        [Arguments("--01x234y567z89", "3i")]
        [Arguments("--01x234y567z89w", "4i")]
        [Arguments("--013456789", "1d")]
        [Arguments("--01346789", "2d")]
        [Arguments("--0134679", "3d")]
        [Arguments("--013467", "4d")]
        [Arguments("--1023456789", "1t")]
        [Arguments("--1023546789", "2t")]
        [Arguments("--1023546798", "3t")]
        [Arguments("--1032546798", "4t")]
        public async Task TypoCorrection(string input, string _)
        {
            var result = _testParser.Parse(input);
            await _testParser.InvokeAsync(result, _nullConsole);
        }
    }
}
