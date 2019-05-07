﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;

namespace System.CommandLine.Benchmarks.CommandLine
{
    /// <summary>
    /// Measures the performance of <see cref="System.CommandLine.Parser"/> for custom scenarios.
    /// </summary>
    [BenchmarkCategory(Categories.CommandLine)]
    public class Perf_Parser_CustomScenarios
    {
        private string _testSymbolsAsString;
        private Parser _testParser;

        [GlobalSetup(Target = nameof(OneOptWithNestedCommand_Parse))]
        public void SetupOneOptWithNestedCommand()
        {
            var rootCommand = new Command("root_command");
            var nestedCommand = new Command("nested_command");
            nestedCommand.AddOption(new Option("-opt1",
                                               "description...",
                                               new Argument<int>(defaultValue: 123)
                                               {
                                                   Arity = ArgumentArity.ExactlyOne
                                               }));
            rootCommand.AddCommand(nestedCommand);

            _testParser = new Parser(rootCommand);
            _testSymbolsAsString = "root_command nested_command -opt1 321";
        }

        [Benchmark]
        public ParseResult OneOptWithNestedCommand_Parse() 
            => _testParser.Parse(_testSymbolsAsString);
    }
}
