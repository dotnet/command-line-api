// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    internal class ArgumentConversionResultSet : AliasedSet<ArgumentConversionResult>
    {
        protected override IReadOnlyList<string> GetAliases(ArgumentConversionResult item)
        {
            return new[] { item.Argument.Name };
        }

        protected override IReadOnlyList<string> GetRawAliases(ArgumentConversionResult item)
        {
            return new[] { item.Argument.Name };
        }
    }
}
