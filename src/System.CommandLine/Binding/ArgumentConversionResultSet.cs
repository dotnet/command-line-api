// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Collections;

namespace System.CommandLine.Binding
{
    internal class ArgumentConversionResultSet : AliasedSet<ArgumentConversionResult>
    {
        protected override IReadOnlyCollection<string> GetAliases(ArgumentConversionResult item)
        {
            return new[] { item.Argument.Name };
        }

        protected override IReadOnlyCollection<string> GetRawAliases(ArgumentConversionResult item)
        {
            return new[] { item.Argument.Name };
        }
    }
}
