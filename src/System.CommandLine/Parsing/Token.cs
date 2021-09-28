// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// A unit of significant text on the command line.
    /// </summary>
    public class Token
    {
        /// <param name="value">The string value of the token.</param>
        /// <param name="type">The type of the token.</param>
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

        internal Token(string value, int position = -1, bool wasBundled = false)
        {
            Value = value;
            UnprefixedValue = value.RemovePrefix();
            Type = TokenType.Option;
            Position = position;
            WasBundled = wasBundled;
        }

        internal int Position { get; }

        /// <summary>
        /// The string value of the token.
        /// </summary>
        public string Value { get; }

        internal bool IsImplicit => Position == -1;

        internal string UnprefixedValue { get; }

        internal bool WasBundled { get; }

        /// <summary>
        /// The type of the token.
        /// </summary>
        public TokenType Type { get; }

        /// <inheritdoc />
        public override bool Equals(object obj) =>
            obj is Token token &&
            Value == token.Value &&
            Type == token.Type;

        /// <inheritdoc />
        public override int GetHashCode() => (Value, Type).GetHashCode();

        /// <inheritdoc />
        public override string ToString() => $"{Type}: {Value}";

        /// <summary>
        /// Checks if two specified <see cref="Token"/> instances have the same value.
        /// </summary>
        /// <param name="left">The first <see cref="Token"/>.</param>
        /// <param name="right">The second <see cref="Token"/>.</param>
        /// <returns><see langword="true" /> if the objects are equal.</returns>
        public static bool operator ==(Token left, Token right)
        {
            return EqualityComparer<Token>.Default.Equals(left, right);
        }

        /// <summary>
        /// Checks if two specified <see cref="Token"/> instances have different values.
        /// </summary>
        /// <param name="left">The first <see cref="Token"/>.</param>
        /// <param name="right">The second <see cref="Token"/>.</param>
        /// <returns><see langword="true" /> if the objects are not equal.</returns>
        public static bool operator !=(Token left, Token right)
        {
            return !(left == right);
        }
    }
}
