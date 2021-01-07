// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Parsing
{
    internal class ImplicitToken : Token
    {
        public ImplicitToken(object? value, TokenType type) : base(value?.ToString(), type)
        {
            ActualValue = value;
        }

        public object? ActualValue { get; }
    }
}