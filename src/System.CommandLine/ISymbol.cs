// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;

namespace System.CommandLine
{
    public interface ISymbol : IValueDescriptor, ISuggestionSource
    {
        string Description { get; }

        IReadOnlyCollection<string> Aliases { get; }

        IReadOnlyCollection<string> RawAliases { get; }

        ICommand Parent { get; }

        bool HasAlias(string alias);

        bool HasRawAlias(string alias);

        bool IsHidden { get; }

        IArgument Argument { get; }

        ISymbolSet Children { get; }
    }
}
