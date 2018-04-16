// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class Option : Symbol
    {
        public Option(
            IReadOnlyCollection<string> aliases,
            string description,
            ArgumentsRule arguments = null,
            IReadOnlyCollection<Symbol> options = null) : base(aliases, description, arguments, options)
        {
        }
    }
}
