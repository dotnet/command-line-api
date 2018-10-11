// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Builder
{
    public class OptionBuilder : SymbolBuilder
    {
        public OptionBuilder(
            IReadOnlyCollection<string> aliases,
            CommandBuilder parent) : base(parent)
        {
            Aliases.AddRange(aliases);
        }

        public List<string> Aliases { get; } = new List<string>();

        internal HelpDetail Help { get; set; }

        public Option BuildOption()
        {
            return new Option(Aliases, Description, BuildArguments(), Help);
        }
    }
}
