// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.CommandLine.Collections
{
    public class SymbolSet : AliasedSet<ISymbol>, ISymbolSet
    {
        internal override void Add(ISymbol item)
        {
            ThrowIfAnyAliasIsInUse(item);

            base.Add(item);
        }

        internal void AddWithoutAliasCollisionCheck(ISymbol item) => base.Add(item);

        internal bool IsAnyAliasInUse(
            ISymbol item,
            [MaybeNullWhen(false)] out string aliasAlreadyInUse)
        {
            var itemRawAliases = GetRawAliases(item);

            for (var i = 0; i < Items.Count; i++)
            {
                var existingItem = Items[i];

                for (var j = 0; j < itemRawAliases.Count; j++)
                {
                    var rawAliasToCheckFor = itemRawAliases[j];

                    if (Contains(GetRawAliases(existingItem), rawAliasToCheckFor))
                    {
                        aliasAlreadyInUse = rawAliasToCheckFor;
                        return true;
                    }
                }
            }

            aliasAlreadyInUse = null!;
            return false;

            static IReadOnlyList<string> GetRawAliases(ISymbol symbol)
            {
                return symbol switch
                {
                    IArgument arg => new[] { arg.Name },
                    _ => symbol.RawAliases
                };
            }
        }

        internal void ThrowIfAnyAliasIsInUse(ISymbol item)
        {
            string? rawAliasAlreadyInUse;

            switch (item)
            {
                case IOption _:
                case ICommand _:
                    if (IsAnyAliasInUse(item, out rawAliasAlreadyInUse))
                    {
                        throw new ArgumentException($"Alias '{rawAliasAlreadyInUse}' is already in use.");
                    }

                    break;

                case IArgument argument:
                    if (IsAnyAliasInUse(argument, out rawAliasAlreadyInUse))
                    {
                        throw new ArgumentException($"Alias '{rawAliasAlreadyInUse}' is already in use.");
                    }

                    break;
            }
        }

        protected override IReadOnlyList<string> GetAliases(ISymbol item) =>
            item.Aliases;

        protected override IReadOnlyList<string> GetRawAliases(ISymbol item) => item.RawAliases;

        protected override bool IsCaseInsensitive(ISymbol item) => item.IsCaseInsensitive;
    }
}