// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing;

/// <summary>
/// Base class for CliValueResult and CliCommandResult.
/// </summary>
/// <remarks>
/// Common values such as `TextForDisplay` are expected
/// </remarks>
public abstract class CliSymbolResult(IEnumerable<Location> locations)
{
    /// <summary>
    /// Gets the locations at which the tokens that made up the value appeared.
    /// </summary>
    /// <remarks>
    /// This needs to be a collection for CliValueType because collection types have 
    /// multiple tokens and they will not be simple offsets when response files are used.
    /// </remarks>
    public IEnumerable<Location> Locations { get; } = locations;

}
