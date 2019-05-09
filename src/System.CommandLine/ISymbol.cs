// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    public interface ISymbol : ISuggestionSource
    {
        string Name { get; }

        string Description { get; }

        IReadOnlyCollection<string> Aliases { get; }

        IReadOnlyCollection<string> RawAliases { get; }

        ISymbol Parent { get; }

        bool HasAlias(string alias);

        bool HasRawAlias(string alias);

        bool IsHidden { get; }

        ISymbolSet Children { get; }
    }
}
