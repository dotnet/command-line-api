// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Collections;
using System.CommandLine.Suggestions;

namespace System.CommandLine
{
    public interface ISymbol : ISuggestionSource
    {
        string Name { get; }

        string? Description { get; }

        bool IsHidden { get; }

        ISymbolSet Children { get; }

        ISymbolSet Parents { get; }
    }
}