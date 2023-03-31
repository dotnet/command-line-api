// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using BenchmarkDotNet.Attributes;

namespace System.CommandLine.Benchmarks.CommandLine
{
    /// <summary>
    /// Measures the performance of <see cref="CliParser"/> for custom scenarios.
    /// </summary>
    [BenchmarkCategory(Categories.CommandLine)]
    public class Perf_Parser_CustomScenarios
    {
        private string _testSymbolsAsString;
        private CliCommand _rootCommand;
        private CliConfiguration _configuration;

        [GlobalSetup(Target = nameof(OneOptWithNestedCommand_Parse))]
        public void SetupOneOptWithNestedCommand()
        {
            _rootCommand = new CliCommand("root_command");
            var nestedCommand = new CliCommand("nested_command");
            var option = new CliOption<int>("-opt1") { DefaultValueFactory = (_) => 123 };
            nestedCommand.Options.Add(option);
            _rootCommand.Subcommands.Add(nestedCommand);

            _testSymbolsAsString = "root_command nested_command -opt1 321";
            _configuration = new CliConfiguration(_rootCommand);
        }

        [Benchmark]
        public ParseResult OneOptWithNestedCommand_Parse() 
            => _rootCommand.Parse(_testSymbolsAsString, _configuration);
    }
}
