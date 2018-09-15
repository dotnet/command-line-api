// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    public class Option : Symbol
    {
        public Option(
            IReadOnlyCollection<string> aliases,
            string description,
            Argument argument = null,
            HelpDetail help = null)
            : base(aliases, description, argument, help)
        { }

        public Option(
            string alias,
            string description,
            Argument argument = null,
            HelpDetail help = null)
            : base(new [] {alias}, description, argument, help)
        { }
    }
}
