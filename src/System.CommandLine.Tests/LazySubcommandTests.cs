// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
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
        public void Subcommands_property_return_type_remains_IList_of_Command_for_binary_compatibility()
        {
            var prop = typeof(Command).GetProperty(nameof(Command.Subcommands));
            prop.Should().NotBeNull();
            prop!.PropertyType.Should().Be(typeof(IList<Command>));
        }

        [Fact]
        public void Unmatched_lazy_subcommands_are_not_materialized_during_parse_or_invoke()
        {
            int buildCalls = 0;
            int publishCalls = 0;
            int invokedCount = 0;

            Option<bool> diagOption = new("--diagnostics", "-d") { Recursive = true };
            RootCommand root = new("dotnet")
            {
                diagOption,
                {
                    "build",
                    () =>
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
                    }
                },
                {
                    "publish",
                    () =>
                    {
                        Interlocked.Increment(ref publishCalls);
                        return new Command("publish", "Publish a project");
                    }
                }
            };

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
            root.Add("solution", ["sln"], () =>
            {
                slnCalls++;
                Command sln = new("solution", "Manage solutions");
                sln.Add("list", () =>
                {
                    listCalls++;
                    Command list = new("list", "List projects");
                    list.SetAction(_ => 0);
                    return list;
                });
                sln.Add("add", () =>
                {
                    addCalls++;
                    return new Command("add", "Add project");
                });
                return sln;
            });
            root.Add("tool", () =>
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
        public void GetResult_for_unselected_lazy_sibling_returns_null_without_invoking_its_factory()
        {
            int selectedCalls = 0;
            int otherCalls = 0;

            RootCommand root = new();
            root.Add("selected", () =>
            {
                selectedCalls++;
                return new Command("selected");
            });
            root.Add("other", () =>
            {
                otherCalls++;
                throw new InvalidOperationException("other should not be created");
            });

            ParseResult result = root.Parse("selected");
            selectedCalls.Should().Be(1);
            otherCalls.Should().Be(0);

            result.GetResult("other").Should().BeNull();
            otherCalls.Should().Be(0);
        }

        [Fact]
        public void Parsed_command_chain_lookup_remains_authoritative_after_fallback_index_is_built()
        {
            Option<int> sharedOption = new("--value");
            Option<int> activeOption = new("--value");

            RootCommand root = new() { sharedOption };
            root.Add("active", () => new Command("active") { activeOption });
            root.Add("inactive", () => new Command("inactive") { sharedOption });

            ParseResult result = root.Parse("--value 1 active --value 2");
            result.GetValue<int>("--value").Should().Be(2);

            Action lookupMissing = () => result.GetResult("missing");
            lookupMissing.Should().Throw<ArgumentException>();

            // After _symbolsByName fallback has been built, the active subcommand's option must still win.
            result.GetValue<int>("--value").Should().Be(2);
        }

        [Fact]
        public void Mutating_caller_aliases_array_after_registration_does_not_affect_lazy_subcommand()
        {
            RootCommand singleAliasRoot = new();
            string[] singleAliases = ["alias"];
            singleAliasRoot.Add("build", singleAliases, () => new Command("build"));
            singleAliases[0] = "changed";

            ParseResult singleResult = singleAliasRoot.Parse("alias");
            singleResult.Errors.Should().BeEmpty();
            singleResult.CommandResult.Command.Name.Should().Be("build");
            singleResult.CommandResult.Command.Aliases.Should().Equal("alias");

            RootCommand multiAliasRoot = new();
            string[] multiAliases = ["a1", "a2", "a1", "build"];
            multiAliasRoot.Add("build", multiAliases, () => new Command("build"));
            multiAliases[0] = "mutated1";
            multiAliases[1] = "mutated2";

            ParseResult multiResult = multiAliasRoot.Parse("a2");
            multiResult.Errors.Should().BeEmpty();
            multiResult.CommandResult.Command.Name.Should().Be("build");
            multiResult.CommandResult.Command.Aliases.Should().BeEquivalentTo(["a1", "a2"]);
        }

        [Fact]
        public void Reentrant_factory_throws_InvalidOperationException_and_invokes_factory_only_once()
        {
            RootCommand root = new();
            int calls = 0;

            root.Add("repeat", () =>
            {
                calls++;
                _ = root.Subcommands[0];
                return new Command("repeat");
            });

            Action act = () => _ = root.Subcommands[0];
            act.Should().Throw<InvalidOperationException>();
            calls.Should().Be(1);
        }

        [Fact]
        public void Bundled_short_option_and_lazy_command_alias_parses_identically_to_eager()
        {
            Option<bool> optA = new("-a");
            Option<bool> subOptC = new("-c");

            RootCommand root = new() { optA };
            root.Add("build", ["-b"], () =>
            {
                Command cmd = new("build") { subOptC };
                cmd.SetAction(_ => 0);
                return cmd;
            });

            ParseResult resultAb = root.Parse("-ab");
            resultAb.Errors.Should().BeEmpty();
            resultAb.GetValue(optA).Should().BeTrue();
            resultAb.CommandResult.Command.Name.Should().Be("build");

            ParseResult resultAbc = root.Parse("-abc");
            resultAbc.Errors.Should().BeEmpty();
            resultAbc.GetValue(optA).Should().BeTrue();
            resultAbc.GetValue(subOptC).Should().BeTrue();
            resultAbc.CommandResult.Command.Name.Should().Be("build");
        }

        [Fact]
        public void CaptureRemainingTokens_does_not_materialize_lazy_subcommands_when_tokens_are_captured()
        {
            int subCalls = 0;
            Argument<string> toolName = new("toolName");
            Argument<string[]> toolArgs = new("toolArgs") { CaptureRemainingTokens = true };

            RootCommand root = new()
            {
                toolName,
                toolArgs
            };
            root.SetAction(_ => 0);
            root.Add("sub", () =>
            {
                subCalls++;
                throw new InvalidOperationException("Captured token should not materialize lazy subcommand");
            });

            ParseResult result = root.Parse("myTool sub --flag");
            subCalls.Should().Be(0);
            result.Errors.Should().BeEmpty();
            result.CommandResult.Command.Should().BeSameAs(root);
            result.GetValue(toolName).Should().Be("myTool");
            result.GetValue(toolArgs).Should().Equal("sub", "--flag");
        }

        [Fact]
        public void Scalar_CaptureRemainingTokens_overflow_resumes_lazy_subcommand_matching()
        {
            int subCalls = 0;
            Argument<string> first = new("first");
            Argument<string> escaped = new("escaped") { CaptureRemainingTokens = true };

            RootCommand root = new() { first, escaped };
            root.Add("sub", () =>
            {
                subCalls++;
                return new Command("sub");
            });

            // First "sub" is captured by scalar `escaped`; second "sub" overflows capture and matches lazy subcommand.
            ParseResult capturedOnly = root.Parse("foo sub");
            subCalls.Should().Be(0);
            capturedOnly.GetValue(escaped).Should().Be("sub");

            ParseResult overflowToSub = root.Parse("foo capturedVal sub");
            subCalls.Should().Be(1);
            overflowToSub.CommandResult.Command.Name.Should().Be("sub");
        }

        [Fact]
        public async Task Shared_command_instance_across_concurrent_lazy_parents_preserves_all_parents()
        {
            for (int iter = 0; iter < 25; iter++)
            {
                Command shared = new("reused") { new Option<string>("--opt") };
                shared.SetAction(_ => 0);

                Command inner1 = new("inner1");
                Command inner2 = new("inner2");
                using ManualResetEventSlim gate = new(false);

                inner1.Add("reused", () =>
                {
                    gate.Wait(TimeSpan.FromSeconds(5));
                    return shared;
                });
                inner2.Add("reused", () =>
                {
                    gate.Wait(TimeSpan.FromSeconds(5));
                    return shared;
                });

                RootCommand outer = new() { inner1, inner2 };

                Task<ParseResult> t1 = Task.Run(() => outer.Parse("inner1 reused --opt 1"));
                Task<ParseResult> t2 = Task.Run(() => outer.Parse("inner2 reused --opt 2"));
                gate.Set();

                ParseResult[] results = await Task.WhenAll(t1, t2);
                results[0].Errors.Should().BeEmpty();
                results[1].Errors.Should().BeEmpty();
                shared.Parents.Should().Contain(inner1).And.Contain(inner2);
            }
        }

        [Fact]
        public void TryGetSubcommand_avoids_enumerating_unmatched_entries_and_supports_StringComparison()
        {
            int firstCalls = 0;
            int secondCalls = 0;

            RootCommand root = new();
            root.Add("first", () =>
            {
                firstCalls++;
                return new Command("first");
            });
            root.Add("second", ["sec"], () =>
            {
                secondCalls++;
                return new Command("second");
            });

            root.TryGetSubcommand("missing", out Command? missing).Should().BeFalse();
            missing.Should().BeNull();
            root.TryGetSubcommand("SEC", out _).Should().BeFalse();
            firstCalls.Should().Be(0);
            secondCalls.Should().Be(0);

            root.TryGetSubcommand("SEC", StringComparison.OrdinalIgnoreCase, out Command? second).Should().BeTrue();
            second!.Name.Should().Be("second");
            firstCalls.Should().Be(0);
            secondCalls.Should().Be(1);
        }

        [Fact]
        public void Mismatched_name_or_undeclared_alias_or_null_throws_InvalidOperationException()
        {
            RootCommand wrongNameRoot = new();
            wrongNameRoot.Add("expected", () => new Command("actual"));
            Action parseWrongName = () => wrongNameRoot.Parse(["expected"]);
            parseWrongName.Should().Throw<InvalidOperationException>();

            RootCommand undeclaredAliasRoot = new();
            undeclaredAliasRoot.Add("solution", ["sln"], () =>
            {
                Command cmd = new("solution");
                cmd.Aliases.Add("slnx");
                return cmd;
            });
            Action parseUndeclaredAlias = () => undeclaredAliasRoot.Parse(["sln"]);
            parseUndeclaredAlias.Should().Throw<InvalidOperationException>();

            RootCommand nullReturnRoot = new();
            nullReturnRoot.Add("nullcmd", () => null!);
            Action parseNullReturn = () => nullReturnRoot.Parse(["nullcmd"]);
            parseNullReturn.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Registration_and_lookup_validate_arguments_and_detect_sibling_collisions()
        {
            RootCommand root = new();

            Action nullName = () => root.Add(null!, () => new Command("x"));
            nullName.Should().Throw<ArgumentNullException>();

            Action whitespaceName = () => root.Add("bad name", () => new Command("bad name"));
            whitespaceName.Should().Throw<ArgumentException>();

            Action whitespaceAlias = () => root.Add("good", ["bad alias"], () => new Command("good"));
            whitespaceAlias.Should().Throw<ArgumentException>();

            Action nullAlias = () => root.Add("good", [null!], () => new Command("good"));
            nullAlias.Should().Throw<ArgumentNullException>();

            Action nullFactory = () => root.Add("good", null!);
            nullFactory.Should().Throw<ArgumentNullException>();

            Action nullLookup = () => root.TryGetSubcommand(null!, out _);
            nullLookup.Should().Throw<ArgumentNullException>();

            Action invalidComparison = () => root.TryGetSubcommand("good", (StringComparison)999, out _);
            invalidComparison.Should().Throw<ArgumentException>();

            // Sibling alias collision detected during Parse without materializing either lazy subcommand:
            RootCommand colliding = new();
            colliding.Add("first", ["dup"], () => throw new InvalidOperationException("should not run"));
            colliding.Add("second", ["dup"], () => throw new InvalidOperationException("should not run"));
            Action parseCollision = () => colliding.Parse("other");
            parseCollision.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Subcommands_list_operations_do_not_materialize_untouched_entries_and_manage_parents()
        {
            int lazyCalls = 0;
            RootCommand root = new();
            Command eager = new("eager");
            root.Add(eager);
            root.Add("lazy", () =>
            {
                lazyCalls++;
                return new Command("lazy");
            });

            eager.Parents.Should().ContainSingle().Which.Should().BeSameAs(root);
            root.Subcommands.Contains(eager).Should().BeTrue();
            root.Subcommands.Contains(new Command("lazy")).Should().BeFalse();
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
        public void Help_and_completions_materialize_direct_children_without_materializing_grandchildren()
        {
            RootCommand root = new();
            root.Add("alpha", () =>
            {
                Command alpha = new("alpha", "Alpha command description");
                alpha.Add("grandchild", () => throw new InvalidOperationException("grandchild should not be created"));
                return alpha;
            });

            StringWriter output = new();
            root.Parse("--help").Invoke(new InvocationConfiguration { Output = output }).Should().Be(0);
            output.ToString().Should().Contain("Alpha command description");

            var completions = root.Parse("al").GetCompletions().Select(c => c.Label).ToArray();
            completions.Should().Contain("alpha");
        }

        [Fact]
        public async Task Concurrent_parses_materialize_lazy_subcommand_once()
        {
            int factoryCalls = 0;
            using ManualResetEventSlim gate = new(false);

            RootCommand root = new();
            root.Add("build", () =>
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
