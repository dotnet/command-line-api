// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    /// <summary>
    /// A symbol, such as an option or command, having one or more fixed names in a command line interface.
    /// </summary>
    public abstract class IdentifierSymbol : Symbol
    {
        internal AliasSet? _aliases;

        private protected IdentifierSymbol(string name) : base(name) 
        {
        }

        private protected IdentifierSymbol(string name, string[] aliases) : base(name)
            => _aliases = new(aliases ?? throw new ArgumentNullException(nameof(aliases)));

        /// <summary>
        /// Gets the unique set of strings that can be used on the command line to specify the symbol.
        /// </summary>
        /// <remarks>The collection does not contain the Name of the Symbol.</remarks>
        public ICollection<string> Aliases => _aliases ??= new();

        internal bool EqualsNameOrAlias(string name)
            => Name.Equals(name, StringComparison.Ordinal) || (_aliases is not null && _aliases.Contains(name));
    }
}