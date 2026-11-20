// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;

namespace System.CommandLine.Benchmarks.CommandLine
{
    /// <summary>
    /// Measures the cost of creating (and parsing) a large symbol tree with eagerly and lazily registered subcommands.
    /// </summary>
    [BenchmarkCategory(Categories.CommandLine)]
    public class Perf_LazySubcommands
    {
        private const int TopLevelCommandCount = 30;
        private const int NestedCommandCount = 5;
        private const int OptionCount = 10;

        private readonly string[] _args = ["command15", "subcommand3", "--option7", "value", "argument"];

        [Benchmark]
        public RootCommand Create_Eager() => CreateRootCommand(lazy: false);

        [Benchmark]
        public RootCommand Create_Lazy() => CreateRootCommand(lazy: true);

        [Benchmark]
        public ParseResult CreateAndParse_Eager() => CreateRootCommand(lazy: false).Parse(_args);

        [Benchmark]
        public ParseResult CreateAndParse_Lazy() => CreateRootCommand(lazy: true).Parse(_args);

        private static RootCommand CreateRootCommand(bool lazy)
        {
            RootCommand root = new("root command description");

            for (int i = 0; i < TopLevelCommandCount; i++)
            {
                string name = $"command{i}";
                AddSubcommand(root, name, () => CreateTopLevelCommand(name, lazy), lazy);
            }

            return root;
        }

        private static Command CreateTopLevelCommand(string name, bool lazy)
        {
            Command command = CreateCommand(name);

            for (int i = 0; i < NestedCommandCount; i++)
            {
                string nestedName = $"subcommand{i}";
                AddSubcommand(command, nestedName, () => CreateCommand(nestedName), lazy);
            }

            return command;
        }

        private static Command CreateCommand(string name)
        {
            Command command = new(name, $"{name} description")
            {
                new Argument<string>("argument") { Description = "argument description" }
            };

            for (int i = 0; i < OptionCount; i++)
            {
                command.Options.Add(new Option<string>($"--option{i}", $"-o{i}") { Description = $"option {i} description" });
            }

            command.SetAction(_ => 0);

            return command;
        }

        private static void AddSubcommand(Command parent, string name, Func<Command> factory, bool lazy)
        {
            if (lazy)
            {
                parent.Subcommands.AddLazy(name, factory);
            }
            else
            {
                parent.Subcommands.Add(factory());
            }
        }
    }
}
