// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Benchmarks.Helpers;
using System.Linq;
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
        private readonly CommandLineConfiguration _configuration;

        public Perf_Parser_TypoCorrection()
        {
            var option = new Option<bool>("--0123456789");

            _configuration = new CommandLineBuilder(new RootCommand { option })
                          .UseTypoCorrections()
                          .Build();
            _configuration.Output = System.IO.TextWriter.Null;
        }

        public IEnumerable<BdnParam<ParseResult>> GenerateTestParseResults()
            => new[]
                {
                    "--0123456789",
                    "--01234567x9",
                    "--0x234567y9",
                    "--0x234z67y9",
                    "--0x234z67yw",
                    "--01x23456789",
                    "--01x234y56789",
                    "--01x234y567z89",
                    "--01x234y567z89w",
                    "--013456789",
                    "--01346789",
                    "--0134679",
                    "--013467",
                    "--1023456789",
                    "--1023546789",
                    "--1023546798",
                    "--1032546798"
                }
                .Select(opt => new BdnParam<ParseResult>(_configuration.RootCommand.Parse(opt, _configuration), opt));

        [Benchmark]
        [ArgumentsSource(nameof(GenerateTestParseResults))]
        public Task TypoCorrection(BdnParam<ParseResult> parseResult)
            => parseResult.Value.InvokeAsync();
    }
}
