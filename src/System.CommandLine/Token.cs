// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
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

        public override bool Equals(object obj) =>
            obj is Token token &&
            Value == token.Value &&
            Type == token.Type;

        public override int GetHashCode()
        {
            var hashCode = 1574892647;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Value);
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            return hashCode;
        }

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
