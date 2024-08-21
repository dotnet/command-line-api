// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.ValueConditions;

public class InclusiveGroup : CommandCondition
{
    private IEnumerable<CliValueSymbol> group = [];

    public InclusiveGroup(IEnumerable<CliValueSymbol> group)
        : base(nameof(InclusiveGroup))
    {
        this.group = group;
    }

    public IEnumerable<CliValueSymbol> Members => group.ToList();
}
