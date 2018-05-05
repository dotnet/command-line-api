// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class Token
    {
        public Token(string value, TokenType type)
        {
            Value = value ?? "";
            Type = type;
        }

        public string Value { get; }

        public TokenType Type { get; }

        public override string ToString() => $"{Type}: {Value}";
    }
}
