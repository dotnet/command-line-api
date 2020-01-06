// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Collections
{
    public class SymbolSet : AliasedSet<ISymbol>, ISymbolSet
    {
        internal override void Add(ISymbol item)
        {
            string rawAliasAlreadyInUse;

            switch (item)
            {
                case IOption _:
                case ICommand _:
                    if (IsAliasInUse(item, GetRawAliases, out rawAliasAlreadyInUse))
                    {
                        throw new ArgumentException($"Alias '{rawAliasAlreadyInUse}' is already in use.");
                    }

                    break;

                case IArgument argument:
                    if (IsAliasInUse(argument, i => new[] { i.Name }, out rawAliasAlreadyInUse))
                    {
                        throw new ArgumentException($"Alias '{rawAliasAlreadyInUse}' is already in use.");
                    }

                    break;
            }

            base.Add(item);
        }

        internal void AddWithoutAliasCollisionCheck(ISymbol item) => base.Add(item);

        private bool IsAliasInUse(
            ISymbol item,
            Func<ISymbol, IReadOnlyList<string>> values,
            out string rawAliasAlreadyInUse)
        {
            var itemRawAliases = values(item);

            for (var i = 0; i < Items.Count; i++)
            {
                var existingItem = Items[i];

                for (var j = 0; j < itemRawAliases.Count; j++)
                {
                    var rawAliasToCheckFor = itemRawAliases[j];

                    if (Contains(values(existingItem), rawAliasToCheckFor))
                    {
                        rawAliasAlreadyInUse = rawAliasToCheckFor;
                        return true;
                    }
                }
            }

            rawAliasAlreadyInUse = null;
            return false;
        }

        protected override IReadOnlyList<string> GetAliases(ISymbol item) =>
            item.Aliases;

        protected override IReadOnlyList<string> GetRawAliases(ISymbol item) => item.RawAliases;
    }
}