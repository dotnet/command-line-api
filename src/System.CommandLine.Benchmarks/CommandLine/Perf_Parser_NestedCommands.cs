// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using BenchmarkDotNet.Attributes;

namespace System.CommandLine.Benchmarks.CommandLine
{
    /// <summary>
    /// Measures the performance of <see cref="CliParser"/> when parsing commands.
    /// </summary>
    [BenchmarkCategory(Categories.CommandLine)]
    public class Perf_Parser_NestedCommands
    {
        private string _testSymbolsAsString;
        private CliCommand _rootCommand;
        private CliConfiguration _configuration;

        /// <remarks>
        /// 1 - cmd-root
        ///
        /// 2 - cmd-root/
        ///     |-cmd-nested0
        /// 
        /// 5 - cmd-root/
        ///     |-cmd-nested0/
        ///       |-cmd-nested00/
        ///         |-cmd-nested000/
        ///           |-cmd-nested0000
        /// </remarks>
        [Params(1, 2, 5)]
        public int TestCommandsDepth;

        private void GenerateTestNestedCommands(CliCommand parent, int depth, int countPerLevel)
        {
            if (depth == 0)
                return;

            for (int i = 0; i < countPerLevel; i++)
            {
                string cmdName = $"{parent.Name}_{depth}.{i}";
                CliCommand cmd = new(cmdName);
                parent.Subcommands.Add(cmd);
                GenerateTestNestedCommands(cmd, depth - 1, countPerLevel);
            }
        }

        [GlobalSetup]
        public void SetupRootCommand()
        {
            string rootCommandName = "root";
            var rootCommand = new CliCommand(rootCommandName);
            _testSymbolsAsString = rootCommandName;
            GenerateTestNestedCommands(rootCommand, TestCommandsDepth, TestCommandsDepth);

            // Choose only one path from the commands tree for the test arguments string
            CliCommand currentCmd = rootCommand;
            while (currentCmd is not null && currentCmd.Subcommands.Count > 0)
            {
                currentCmd = currentCmd.Subcommands[0];
                _testSymbolsAsString = string.Join(" ", _testSymbolsAsString, currentCmd.Name);
            }

            _rootCommand = rootCommand;
            _configuration = new CliConfiguration(rootCommand);
        }

        [Benchmark]
        public ParseResult Parser_Parse() => CliParser.Parse(_rootCommand, _testSymbolsAsString, _configuration);
    }
}
