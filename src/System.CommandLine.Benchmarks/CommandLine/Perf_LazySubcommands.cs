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
        [Params(3, 10)]
        public int TopLevelCommandCount { get; set; }

        [Params(2, 5)]
        public int NestedCommandCount { get; set; }

        [Params(3, 10)]
        public int OptionCount { get; set; }

        // Precomputed, so they behave like string literals used by real-world code.
        private string[] TopLevelNames;
        private string[] NestedNames;
        private string[] OptionNames;
        private string[] OptionAliases;
        private string[] OptionDescriptions;

        // Real-world code uses static lambdas, which the compiler allocates once and caches.
        private Func<Command>[] TopLevelFactories;
        private Func<Command>[] NestedFactories;

        private string[] _args;

        [GlobalSetup]
        public void Setup()
        {
            TopLevelNames = CreateStrings(TopLevelCommandCount, "command{0}");
            NestedNames = CreateStrings(NestedCommandCount, "subcommand{0}");
            OptionNames = CreateStrings(OptionCount, "--option{0}");
            OptionAliases = CreateStrings(OptionCount, "-o{0}");
            OptionDescriptions = CreateStrings(OptionCount, "option {0} description");

            TopLevelFactories = CreateFactories(TopLevelNames, name => () => CreateTopLevelCommand_Lazy(name));
            NestedFactories = CreateFactories(NestedNames, name => () => CreateCommand(name));

            _args = [TopLevelNames[TopLevelCommandCount - 1], NestedNames[NestedCommandCount - 1], OptionNames[OptionCount - 1], "value", "argument"];
        }

        [Benchmark]
        public RootCommand Create_Eager() => CreateRootCommand_Eager();

        [Benchmark]
        public RootCommand Create_Lazy() => CreateRootCommand_Lazy();

        [Benchmark]
        public ParseResult CreateAndParse_Eager() => CreateRootCommand_Eager().Parse(_args);

        [Benchmark]
        public ParseResult CreateAndParse_Lazy() => CreateRootCommand_Lazy().Parse(_args);

        private RootCommand CreateRootCommand_Eager()
        {
            RootCommand root = new("root command description");

            for (int i = 0; i < TopLevelNames.Length; i++)
            {
                root.Subcommands.Add(CreateTopLevelCommand_Eager(TopLevelNames[i]));
            }

            return root;
        }

        private RootCommand CreateRootCommand_Lazy()
        {
            RootCommand root = new("root command description");

            for (int i = 0; i < TopLevelNames.Length; i++)
            {
                root.Add(TopLevelNames[i], TopLevelFactories[i]);
            }

            return root;
        }

        private Command CreateTopLevelCommand_Eager(string name)
        {
            Command command = CreateCommand(name);

            for (int i = 0; i < NestedNames.Length; i++)
            {
                command.Subcommands.Add(CreateCommand(NestedNames[i]));
            }

            return command;
        }

        private Command CreateTopLevelCommand_Lazy(string name)
        {
            Command command = CreateCommand(name);

            for (int i = 0; i < NestedNames.Length; i++)
            {
                command.Add(NestedNames[i], NestedFactories[i]);
            }

            return command;
        }

        private Command CreateCommand(string name)
        {
            Command command = new(name, "command description")
            {
                new Argument<string>("argument") { Description = "argument description" }
            };

            for (int i = 0; i < OptionNames.Length; i++)
            {
                command.Options.Add(new Option<string>(OptionNames[i], OptionAliases[i]) { Description = OptionDescriptions[i] });
            }

            command.SetAction(static _ => 0);

            return command;
        }

        private static string[] CreateStrings(int count, string format)
        {
            string[] strings = new string[count];
            for (int i = 0; i < count; i++)
            {
                strings[i] = string.Format(format, i);
            }
            return strings;
        }

        private static Func<Command>[] CreateFactories(string[] names, Func<string, Func<Command>> createFactory)
        {
            Func<Command>[] factories = new Func<Command>[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                factories[i] = createFactory(names[i]);
            }
            return factories;
        }
    }
}
