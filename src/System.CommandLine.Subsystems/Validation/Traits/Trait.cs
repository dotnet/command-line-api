// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Validation.Traits;

public abstract class Trait
{
    protected Trait(bool isCommandTrait = false, Type? fallbackType = null)
    {
        IsCommandTrait = isCommandTrait;
        FallbackType = fallbackType ?? GetType();
    }
    public Type FallbackType { get; set; }
    public bool IsCommandTrait { get; }
}
