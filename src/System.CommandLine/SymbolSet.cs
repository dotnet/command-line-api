// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    public class SymbolSet : AliasedSet<ISymbol>, ISymbolSet
    {
        internal override void Add(ISymbol item)
        {
            switch (item)
            {
                case IOption _:
                case ICommand _:
                    CheckForCollision(item, GetRawAliases);
                    break;

                case IArgument argument:
                    CheckForCollision(argument, i => new[] { i.Name });
                    break;
            }

            base.Add(item);
        }

        private void CheckForCollision(
            ISymbol item, 
            Func<ISymbol, IReadOnlyList<string>> values)
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
                        throw new ArgumentException($"Alias '{rawAliasToCheckFor}' is already in use.");
                    }
                }
            }
        }

        protected override IReadOnlyList<string> GetAliases(ISymbol item) =>
            item.Aliases;

        protected override IReadOnlyList<string> GetRawAliases(ISymbol item) => item.RawAliases;
    }
}
