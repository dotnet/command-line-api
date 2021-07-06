// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// A delegate used to validate symbol results during parsing.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="SymbolResult"/>.</typeparam>
    /// <param name="symbolResult">The symbol result</param>
    /// <returns>An error message to indicate a validation error; otherwise, <c>null</c>.</returns>
    public delegate string? ValidateSymbolResult<in T>(T symbolResult) 
        where T : SymbolResult;
}
