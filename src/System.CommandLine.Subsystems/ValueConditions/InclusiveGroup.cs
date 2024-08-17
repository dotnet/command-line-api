// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine;

public class InclusiveGroup : ValueCondition
{
    private IEnumerable<CliValueSymbol> group = [];

    public InclusiveGroup(IEnumerable<CliValueSymbol> group)
    {
        this.group = group;
    }

    public IEnumerable<CliValueSymbol> Members => group.ToList();
}
