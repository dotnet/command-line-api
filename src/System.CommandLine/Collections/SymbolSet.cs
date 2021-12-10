// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.CommandLine.Collections
{
    /// <summary>
    /// A set of symbols, unique and indexed by their aliases.
    /// </summary>
    public class SymbolSet : AliasedSet<ISymbol>, ISymbolSet
    {
        private List<Argument>? _arguments;
        private List<Option>? _options;

        internal override void Add(ISymbol item)
        {
            ThrowIfAnyAliasIsInUse(item);
            AddWithoutAliasCollisionCheck(item);
        }

        private void ResetIndex(ISymbol item)
        {
            switch (item)
            {
                case Argument _:
                    _arguments = null;
                    break;
                case Option _:
                    _options = null;
                    break;
            }
        }

        internal void AddWithoutAliasCollisionCheck(ISymbol item)
        {
            base.Add(item);
            ResetIndex(item);
        }

        internal bool IsAnyAliasInUse(
            ISymbol item,
            [MaybeNullWhen(false)] out string aliasAlreadyInUse)
        {
            if (Items.Count > 0)
            {
                if (item is IIdentifierSymbol identifier)
                {
                    foreach (string alias in identifier.Aliases)
                    {
                        foreach (ISymbol symbol in Items)
                        {
                            if (symbol.Matches(alias))
                            {
                                aliasAlreadyInUse = alias;
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    string name = item.Name;
                    foreach (ISymbol symbol in Items)
                    {
                        if (symbol.Matches(name))
                        {
                            aliasAlreadyInUse = name;
                            return true;
                        }
                    }
                }
            }

            aliasAlreadyInUse = null!;

            return false;
        }

        internal void ThrowIfAnyAliasIsInUse(ISymbol item)
        {
            if (IsAnyAliasInUse(item, out var rawAliasAlreadyInUse))
            {
                throw new ArgumentException($"Alias '{rawAliasAlreadyInUse}' is already in use.");
            }
        }

        /// <inheritdoc/>
        protected override IReadOnlyCollection<string> GetAliases(ISymbol item) =>
            item switch
            {
                IIdentifierSymbol named => named.Aliases,
                _ => new[] { item.Name }
            };

        internal IReadOnlyList<Argument> Arguments
        {
            get
            {
                return _arguments ??= BuildArgumentsList();

                List<Argument> BuildArgumentsList()
                {
                    var arguments = new List<Argument>(Count);

                    for (var i = 0; i < Count; i++)
                    {
                        if (this[i] is Argument argument)
                        {
                            arguments.Add(argument);
                        }
                    }

                    return arguments;
                }
            }
        }
        
        internal IReadOnlyList<Option> Options
        {
            get
            {
                return _options ??= BuildOptionsList();

                List<Option> BuildOptionsList()
                {
                    var options = new List<Option>(Count);

                    for (var i = 0; i < Count; i++)
                    {
                        if (this[i] is Option Option)
                        {
                            options.Add(Option);
                        }
                    }

                    return options;
                }
            }
        }
    }
}
