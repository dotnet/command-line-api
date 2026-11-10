// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace System.CommandLine.StaticCompletions.Tests;

using System.CommandLine.Help;
using System.CommandLine.StaticCompletions;

public class HelpExtensionsTests
{
    [Fact]
    public void HelpOptionOnlyShowsUsefulNames()
    {
        new HelpOption().Names().Should().BeEquivalentTo(["--help", "-h"]);
    }

    [Fact]
    public void OptionNamesListNameThenAliases()
    {
        new Option<string>("--name", "-n", "--nombre").Names().Should().Equal(["--name", "-n", "--nombre"]);
    }

    [Fact]
    public void OptionsWithNoAliasesHaveOnlyOneName()
    {
        new Option<string>("--name").Names().Should().Equal(["--name"]);
    }

    [Fact]
    public void HeirarchicalOptionsAreFlattened()
    {
        var parentCommand = new Command("parent");
        var childCommand = new Command("child");
        parentCommand.Subcommands.Add(childCommand);
        parentCommand.Options.Add(new Option<string>("--parent-global") { Recursive = true });
        parentCommand.Options.Add(new Option<string>("--parent-local") { Recursive = false });
        parentCommand.Options.Add(new Option<string>("--parent-global-but-hidden") { Recursive = true, Hidden = true });

        childCommand.Options.Add(new Option<string>("--child-local"));
        childCommand.Options.Add(new Option<string>("--child-hidden") { Hidden = true });

        // note: no parent-local or parent-global-but-hidden options, and no locally hidden options
        childCommand.HierarchicalOptions().Select(c => c.Name).Should().Equal(["--child-local", "--parent-global"]);
    }
}
