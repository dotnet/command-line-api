// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    public class OptionDefinition : SymbolDefinition
    {
        public OptionDefinition(
            IReadOnlyCollection<string> aliases,
            string description,
            ArgumentDefinition argumentDefinition = null)
            : base(aliases, description, argumentDefinition)
        { }

        public OptionDefinition(
            string alias,
            string description,
            ArgumentDefinition argumentDefinition = null)
            : base(new [] {alias}, description, argumentDefinition)
        { }
    }
}
