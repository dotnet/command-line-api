// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.ValueConditions;

/// <summary>
/// Describes that a set of options and arguments must all be entered
/// if one or more are entered.
/// </summary>
public class InclusiveGroup : CommandCondition
{
    private IEnumerable<CliValueSymbol> group = [];

    /// <summary>
    /// The constructor for InclusiveGroup.
    /// </summary>
    /// <param name="group">The group of options and arguments that must all be present, or note be present.</param>
    public InclusiveGroup(IEnumerable<CliValueSymbol> group)
        : base(nameof(InclusiveGroup))
    {
        this.group = group;
    }

    /// <summary>
    /// The members of the inclusive group.
    /// </summary>
    public IEnumerable<CliValueSymbol> Members => group.ToList();
}
