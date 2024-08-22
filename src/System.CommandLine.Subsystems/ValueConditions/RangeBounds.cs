// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.ValueConditions;

[Flags]
public enum RangeBounds
{
    Inclusive = 0,
    ExclusiveLowerBound = 1,
    ExclusiveUpperBound = 2,
    ExclusiveUpperAndLowerBounds = 3,
}
