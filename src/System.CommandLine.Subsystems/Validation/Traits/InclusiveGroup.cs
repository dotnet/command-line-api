// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Validation.Traits;

public class InclusiveGroup : Trait
{
    private IEnumerable<CliDataSymbol> group = [];

    public InclusiveGroup(IEnumerable<CliDataSymbol> group)
        : base(isCommandTrait: true)
    {
        this.group = group;
    }

    public IEnumerable<CliDataSymbol> Members => group.ToList();
}
