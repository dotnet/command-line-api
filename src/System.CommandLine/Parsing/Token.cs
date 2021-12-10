// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// A unit of significant text on the command line.
    /// </summary>
    public readonly struct Token : IEquatable<Token>
    {
        internal const int ImplicitPosition = -1;

        /// <param name="value">The string value of the token.</param>
        /// <param name="type">The type of the token.</param>
        public Token(string? value, TokenType type)
        {
            Value = value ?? "";
            Type = type;
            Position = ImplicitPosition;
            WasBundled = false;
        }
       
        internal Token(string? value, TokenType type, int position)
        {
            Value = value ?? "";
            Type = type;
            Position = position;
            WasBundled = false;
        }

        internal Token(string value, int position = ImplicitPosition, bool wasBundled = false)
        {
            Value = value;
            Type = TokenType.Option;
            Position = position;
            WasBundled = wasBundled;
        }

        internal int Position { get; }

        /// <summary>
        /// The string value of the token.
        /// </summary>
        public string Value { get; }

        internal bool WasBundled { get; }

        internal bool IsImplicit => Position == ImplicitPosition;

        internal bool IsDefault => Value is null && Position == default && WasBundled == default && Type == default;

        /// <summary>
        /// The type of the token.
        /// </summary>
        public TokenType Type { get; }

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is Token other && Equals(other);

        /// <inheritdoc />
        public bool Equals(Token other) => Value == other.Value && Type == other.Type;

        /// <inheritdoc />
        public override int GetHashCode() => Value.GetHashCode() ^ (int)Type;

        /// <inheritdoc />
        public override string ToString() => $"{Type}: {Value}";

        /// <summary>
        /// Checks if two specified <see cref="Token"/> instances have the same value.
        /// </summary>
        /// <param name="left">The first <see cref="Token"/>.</param>
        /// <param name="right">The second <see cref="Token"/>.</param>
        /// <returns><see langword="true" /> if the objects are equal.</returns>
        public static bool operator ==(Token left, Token right) => left.Equals(right);

        /// <summary>
        /// Checks if two specified <see cref="Token"/> instances have different values.
        /// </summary>
        /// <param name="left">The first <see cref="Token"/>.</param>
        /// <param name="right">The second <see cref="Token"/>.</param>
        /// <returns><see langword="true" /> if the objects are not equal.</returns>
        public static bool operator !=(Token left, Token right) => !left.Equals(right);

        internal bool IsFirstCharOfTheUnprefixedValue(char c)
        {
            if (Value.Length == 0)
            {
                return false;
            }

            int index = Value.GetPrefixLength();
            return index < Value.Length && Value[index] == c;
        }
    }
}
