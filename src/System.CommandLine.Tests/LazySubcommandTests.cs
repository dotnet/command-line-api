// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class LazySubcommandTests
    {
        [Fact]
        public void Unmatched_lazy_subcommands_are_not_materialized_during_parse_or_invoke()
        {
            int buildCalls = 0;
            int publishCalls = 0;
            int invokedCount = 0;

            RootCommand root = new("dotnet");
            Option<bool> diagOption = new("--diagnostics", "-d") { Recursive = true };
            root.Add(diagOption);

            root.AddSubcommand("build", () =>
            {
                Interlocked.Increment(ref buildCalls);
                Command cmd = new("build", "Build a project");
                Option<string> config = new("--configuration", "-c");
                cmd.Add(config);
                cmd.SetAction(pr =>
                {
                    if (pr.GetValue(config) == "Release" && pr.GetValue(diagOption))
                    {
                        Interlocked.Increment(ref invokedCount);
                    }
                });
                return cmd;
            });

            root.AddSubcommand("publish", () =>
            {
                Interlocked.Increment(ref publishCalls);
                return new Command("publish", "Publish a project");
            });

            root.Subcommands.Count.Should().Be(2);
            buildCalls.Should().Be(0);
            publishCalls.Should().Be(0);

            ParseResult parseResult = root.Parse(["-d", "build", "-c", "Release"]);
            buildCalls.Should().Be(1);
            publishCalls.Should().Be(0);
            parseResult.Errors.Should().BeEmpty();
            parseResult.RootCommandResult.Command.Should().BeSameAs(root);
            parseResult.GetValue<string>("--configuration").Should().Be("Release");
            publishCalls.Should().Be(0);

            parseResult.Invoke().Should().Be(0);
            invokedCount.Should().Be(1);
            publishCalls.Should().Be(0);
        }

        [Fact]
        public void Aliases_route_and_populate_Command_Aliases_without_materializing_siblings()
        {
            int slnCalls = 0;
            int listCalls = 0;
            int addCalls = 0;
            int toolCalls = 0;

            RootCommand root = new("dotnet");
            root.AddSubcommand("solution", ["sln"], () =>
            {
                slnCalls++;
                Command sln = new("solution", "Manage solutions");
                sln.AddSubcommand("list", () =>
                {
                    listCalls++;
                    Command list = new("list", "List projects");
                    list.SetAction(_ => 0);
                    return list;
                });
                sln.AddSubcommand("add", () =>
                {
                    addCalls++;
                    return new Command("add", "Add project");
                });
                return sln;
            });
            root.AddSubcommand("tool", () =>
            {
                toolCalls++;
                return new Command("tool");
            });

            ParseResult result = root.Parse(["sln", "list", "--help"]);
            slnCalls.Should().Be(1);
            listCalls.Should().Be(1);
            addCalls.Should().Be(0);
            toolCalls.Should().Be(0);

            StringWriter output = new();
            result.Invoke(new InvocationConfiguration { Output = output }).Should().Be(0);
            output.ToString().Should().Contain("List projects");
            addCalls.Should().Be(0);
            toolCalls.Should().Be(0);

            root.TryGetSubcommand("sln", out Command? slnCommand).Should().BeTrue();
            slnCommand!.Aliases.Should().Contain("sln");
        }

        [Fact]
        public void ContainsSubcommand_and_TryGetSubcommand_avoid_enumerating_unmatched_entries()
        {
            int firstCalls = 0;
            int secondCalls = 0;

            RootCommand root = new();
            root.AddSubcommand("first", () =>
            {
                firstCalls++;
                return new Command("first");
            });
            root.AddSubcommand("second", ["sec"], () =>
            {
                secondCalls++;
                return new Command("second");
            });

            root.ContainsSubcommand("sec").Should().BeTrue();
            root.ContainsSubcommand("SEC").Should().BeFalse();
            root.ContainsSubcommand("SEC", StringComparison.OrdinalIgnoreCase).Should().BeTrue();
            root.ContainsSubcommand("missing").Should().BeFalse();
            firstCalls.Should().Be(0);
            secondCalls.Should().Be(0);

            root.TryGetSubcommand("missing", out Command? missing).Should().BeFalse();
            missing.Should().BeNull();
            firstCalls.Should().Be(0);
            secondCalls.Should().Be(0);

            root.TryGetSubcommand("SEC", StringComparison.OrdinalIgnoreCase, out Command? second).Should().BeTrue();
            second!.Name.Should().Be("second");
            firstCalls.Should().Be(0);
            secondCalls.Should().Be(1);
        }

        [Fact]
        public void Mismatched_name_or_undeclared_alias_throws_InvalidOperationException()
        {
            RootCommand wrongNameRoot = new();
            wrongNameRoot.AddSubcommand("expected", () => new Command("actual"));

            Action parseWrongName = () => wrongNameRoot.Parse(["expected"]);
            parseWrongName.Should().Throw<InvalidOperationException>();

            RootCommand undeclaredAliasRoot = new();
            undeclaredAliasRoot.AddSubcommand("solution", ["sln"], () =>
            {
                Command cmd = new("solution");
                cmd.Aliases.Add("slnx");
                return cmd;
            });

            Action parseUndeclaredAlias = () => undeclaredAliasRoot.Parse(["sln"]);
            parseUndeclaredAlias.Should().Throw<InvalidOperationException>();

            RootCommand nullReturnRoot = new();
            nullReturnRoot.AddSubcommand("nullcmd", () => null!);
            Action parseNullReturn = () => nullReturnRoot.Parse(["nullcmd"]);
            parseNullReturn.Should().Throw<InvalidOperationException>();

            RootCommand reentrantRoot = new();
            reentrantRoot.AddSubcommand("loop", () =>
            {
                _ = reentrantRoot.Subcommands[0];
                return new Command("loop");
            });
            Action parseReentrant = () => reentrantRoot.Parse(["loop"]);
            parseReentrant.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Registration_and_lookup_validate_arguments()
        {
            RootCommand root = new();

            Action nullName = () => root.AddSubcommand(null!, () => new Command("x"));
            nullName.Should().Throw<ArgumentNullException>();

            Action whitespaceName = () => root.AddSubcommand("bad name", () => new Command("bad name"));
            whitespaceName.Should().Throw<ArgumentException>();

            Action whitespaceAlias = () => root.AddSubcommand("good", ["bad alias"], () => new Command("good"));
            whitespaceAlias.Should().Throw<ArgumentException>();

            Action nullFactory = () => root.AddSubcommand("good", null!);
            nullFactory.Should().Throw<ArgumentNullException>();

            Action nullContains = () => root.ContainsSubcommand(null!);
            nullContains.Should().Throw<ArgumentNullException>();

            Action invalidComparison = () => root.ContainsSubcommand("good", (StringComparison)999);
            invalidComparison.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Subcommands_list_operations_do_not_materialize_untouched_entries_and_manage_parents()
        {
            int lazyCalls = 0;
            RootCommand root = new();
            Command eager = new("eager");
            root.Add(eager);
            root.AddSubcommand("lazy", () =>
            {
                lazyCalls++;
                return new Command("lazy");
            });

            eager.Parents.Should().ContainSingle().Which.Should().BeSameAs(root);
            root.Subcommands.Contains(eager).Should().BeTrue();
            root.Subcommands.IndexOf(eager).Should().Be(0);
            lazyCalls.Should().Be(0);

            Command replacement = new("replacement");
            root.Subcommands[0] = replacement;
            eager.Parents.Should().BeEmpty();
            replacement.Parents.Should().ContainSingle().Which.Should().BeSameAs(root);
            lazyCalls.Should().Be(0);

            root.Subcommands.RemoveAt(1);
            root.Subcommands.Count.Should().Be(1);
            lazyCalls.Should().Be(0);

            root.Subcommands.Clear();
            replacement.Parents.Should().BeEmpty();
        }

        [Fact]
        public async Task Concurrent_parses_materialize_lazy_subcommand_once()
        {
            int factoryCalls = 0;
            using ManualResetEventSlim gate = new(false);

            RootCommand root = new();
            root.AddSubcommand("build", () =>
            {
                Interlocked.Increment(ref factoryCalls);
                gate.Wait(TimeSpan.FromSeconds(5));
                Command cmd = new("build");
                cmd.SetAction(_ => 0);
                return cmd;
            });

            Task<ParseResult>[] tasks = Enumerable.Range(0, 8)
                .Select(_ => Task.Run(() => root.Parse(["build"])))
                .ToArray();

            gate.Set();
            ParseResult[] results = await Task.WhenAll(tasks);

            factoryCalls.Should().Be(1);
            Command expected = results[0].CommandResult.Command;
            results.Select(r => r.CommandResult.Command).Should().AllBeEquivalentTo(expected);
        }
    }
}
