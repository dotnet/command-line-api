// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Benchmarks.Helpers;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace System.CommandLine.Benchmarks.CommandLine
{
    /// <summary>
    /// Measures the performance directives parsing.
    /// </summary>
    [BenchmarkCategory(Categories.CommandLine)]
    public class Perf_Parser_ParseResult
    {
        private readonly CliConfiguration _configuration;
        private readonly StringWriter _output;

        public Perf_Parser_ParseResult()
        {
            _output = new StringWriter();
            var option = new CliOption<bool>("-opt");

            _configuration = new CliConfiguration(new CliRootCommand { option })
            {
                Directives = { new DiagramDirective() },
                Output = _output
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
        {
            StringBuilder stringBuilder = _output.GetStringBuilder();

            // clear the contents, so each benchmark has the same starting state
            stringBuilder.Clear();

            ((SynchronousCliAction)parseResult.Value.Action)!.Invoke(parseResult.Value);

            return stringBuilder.ToString();
        }

        [Benchmark]
        [ArgumentsSource(nameof(GenerateTestParseResults))]
        public async Task<string> ParseResult_DiagramAsync(BdnParam<ParseResult> parseResult)
        {
            StringBuilder stringBuilder = _output.GetStringBuilder();

            // clear the contents, so each benchmark has the same starting state
            stringBuilder.Clear();

            await ((AsynchronousCliAction)parseResult.Value.Action!).InvokeAsync(parseResult.Value);

            return stringBuilder.ToString();
        }
    }
}
