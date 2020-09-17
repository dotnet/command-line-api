// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System.CommandLine.Collections
{
    public class SymbolSet : AliasedSet<ISymbol>, ISymbolSet
    {
        internal override void Add(ISymbol item)
        {
            ThrowIfAnyAliasIsInUse(item);

            base.Add(item);

            if (item is Symbol symbol)
            {
                symbol.OnNameOrAliasChanged += Resync;
            }
        }

        internal override void Remove(ISymbol item)
        {
            base.Remove(item);

            if (item is Symbol symbol)
            {
                symbol.OnNameOrAliasChanged -= Resync;
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

            if (item is IArgument)
            {
                // arguments don't have aliases so match based on Name
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
            else
            {
                var itemRawAliases = item.Aliases.ToArray();

                for (var i = 0; i < itemRawAliases.Length; i++)
                {
                    var alias = itemRawAliases[i];

                    if (_itemsByAlias.ContainsKey(alias))
                    {
                        aliasAlreadyInUse = alias;
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

        protected override IReadOnlyCollection<string> GetAliases(ISymbol item) =>
            item.Aliases;

        protected override IReadOnlyCollection<string> GetRawAliases(ISymbol item) => item.Aliases;
    }
}