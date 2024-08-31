// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine;

/// <summary>
/// The base class for all conditions. Conditions describe aspects of 
/// symbol results, including restrictions used for validation. 
/// </summary>
/// <param name="name"></param>
public abstract class ValueCondition(string name)
{
    /// <summary>
    /// Whether a diagnostic should be reported if there is no validator. 
    /// Conditions may be used for other purposes, such as completions and 
    /// not require validation.
    /// </summary>
    public virtual bool MustHaveValidator { get; } = true;

    /// <summary>
    /// The name of the ValueCondition.
    /// </summary>
    public string Name { get; } = name;
}
