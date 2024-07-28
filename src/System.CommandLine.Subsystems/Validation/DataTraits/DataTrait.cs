// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Validation.DataTraits;

public abstract class DataTrait
{
    public Type FallbackType { get; set; }

    protected DataTrait(Type? fallbackType = null)
        => FallbackType = fallbackType ?? GetType();
}
