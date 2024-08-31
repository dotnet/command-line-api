// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine;

public abstract class CommandCondition(string name)
{
    public virtual bool MustHaveValidator { get; } = true;
    public string Name { get; } = name;
}
