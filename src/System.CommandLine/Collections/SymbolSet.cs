// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System.CommandLine.Collections
{
    public class SymbolSet : AliasedSet<ISymbol>, ISymbolSet
    {
        private List<Argument>? _arguments;
        private List<Option>? _options;

        internal override void Add(ISymbol item)
        {
            ThrowIfAnyAliasIsInUse(item);

            base.Add(item);

            ResetIndex(item);

            if (item is Symbol symbol)
            {   
                symbol.OnNameOrAliasChanged += Resync;
            }
        }

        internal override void Remove(ISymbol item)
        {
            base.Remove(item);

            ResetIndex(item);

            if (item is Symbol symbol)
            {
                symbol.OnNameOrAliasChanged -= Resync;
            }
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

        private void Resync(ISymbol symbol)
        {
            DirtyItems.Add(symbol);
        }

        internal void AddWithoutAliasCollisionCheck(ISymbol item) => base.Add(item);

        internal bool IsAnyAliasInUse(
            ISymbol item,
            [MaybeNullWhen(false)] out string aliasAlreadyInUse)
        {
            EnsureAliasIndexIsCurrent();

            if (item is IIdentifierSymbol identifier)
            {
                var aliases = identifier.Aliases.ToArray();

                for (var i = 0; i < aliases.Length; i++)
                {
                    var alias = aliases[i];

                    if (ItemsByAlias.ContainsKey(alias))
                    {
                        aliasAlreadyInUse = alias;
                        return true;
                    }
                }
            }
            else
            {
                for (var i = 0; i < Items.Count; i++)
                {
                    var existing = Items[i];
                    if (string.Equals(item.Name, existing.Name, StringComparison.Ordinal))
                    {
                        aliasAlreadyInUse = existing.Name;
                        return true;
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