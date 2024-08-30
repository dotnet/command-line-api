// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.ValueConditions;

/// <summary>
/// Specifies how bounds should be treated.
/// </summary>
/// <remarks>
/// If the lower bound is 2 and the upper bound is 4,
/// and the value is 2, an inclusive lower bound would 
/// pass, and an exclusive bound would fail. Similarly, a
/// value of 4 would fail if the upper bound was inclusive, 
/// and fail if the bound was exclusive. Exclusive bounds are
/// often used with fractional values, such as double, single and decimal.
/// </remarks>
[Flags]
public enum RangeBounds
{
    /// <summary>
    /// If the value equals the upper or lower bound, it passes.
    /// </summary>
    Inclusive = 0,

    /// <summary>
    /// If the value is greater than the lower bound, and less than or equal to the upper bound, it passes.
    /// </summary>
    ExclusiveLowerBound = 1,

    /// <summary>
    /// If the value is greater than or equal to the lower bound, and less than the upper bound, it passes.
    /// </summary>
    ExclusiveUpperBound = 2,

    /// <summary>
    /// The value passes only if it is greater than the lower bound and less than the upper bound.
    /// </summary>
    ExclusiveUpperAndLowerBounds = 3,
}
