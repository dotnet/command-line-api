﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    public class Option : Symbol, IOption
    {
        public Option(
            IReadOnlyCollection<string> aliases,
            string description = null,
            Argument argument = null,
            bool isHidden = false)
            : base(aliases, description, argument, isHidden)
        { }

        public Option(
            string alias,
            string description = null,
            Argument argument = null,
            bool isHidden = false)
            : base(new [] {alias}, description, argument, isHidden)
        { }
    }
}
