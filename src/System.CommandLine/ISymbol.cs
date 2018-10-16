// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    public interface ISymbol 
    {
        IReadOnlyCollection<string> Aliases { get; }

        IReadOnlyCollection<string> RawAliases { get; }

        string Name { get; }

        string Description { get; }

        HelpDetail Help { get; }

        ICommand Parent { get; }

        bool HasAlias(string alias);

        bool HasRawAlias(string alias);

        Argument Argument { get; }

        ISymbolSet Children { get; }
    }
}
