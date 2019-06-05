// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public class ArgumentResult : SymbolResult
    {
        internal ArgumentResult(
            IArgument argument,
            Token token,
            SymbolResult parent) : base(argument, token, parent)
        {
            Argument = argument;
        }

        public IArgument Argument { get; }
    }
}
