// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

namespace System.CommandLine
{
    public class ParsedOption : ParsedSymbol
    {
        public ParsedOption(Option option, string token = null, ParsedCommand parent = null) :
            base(option, token ?? option?.ToString(), parent)
        {
        }

        public override ParsedSymbol TryTakeToken(Token token) => 
            TryTakeArgument(token);
    }
}
