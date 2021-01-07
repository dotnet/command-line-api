// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    public class Token
    {
        public Token(string? value, TokenType type)
        {
            Value = value ?? "";
            UnprefixedValue = Value.RemovePrefix();
            Type = type;
            Position = -1;
        }
        
        internal Token(string? value, TokenType type, int position)
        {
            Value = value ?? "";
            UnprefixedValue = Value.RemovePrefix();
            Type = type;
            Position = position;
        }

        internal int Position { get; }

        public string Value { get; }

        internal string UnprefixedValue { get; }

        public TokenType Type { get; }

        public override bool Equals(object obj) =>
            obj is Token token &&
            Value == token.Value &&
            Type == token.Type;

        public override int GetHashCode() => (Value, Type).GetHashCode();

        public override string ToString() => $"{Type}: {Value}";

        public static bool operator ==(Token left, Token right)
        {
            return EqualityComparer<Token>.Default.Equals(left, right);
        }

        public static bool operator !=(Token left, Token right)
        {
            return !(left == right);
        }
    }
}
