// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// A unit of significant text on the command line.
    /// </summary>
    public sealed class Token : IEquatable<Token>
    {
        internal const int ImplicitPosition = -1;

        /// <param name="value">The string value of the token.</param>
        /// <param name="type">The type of the token.</param>
        /// <param name="symbol">The symbol represented by the token</param>
        public Token(string? value, TokenType type, Symbol symbol)
        {
            Value = value ?? "";
            Type = type;
            Symbol = symbol;
            Position = ImplicitPosition;
        }
       
        internal Token(string? value, TokenType type, Symbol? symbol, int position)
        {
            Value = value ?? "";
            Type = type;
            Symbol = symbol;
            Position = position;
        }

        internal int Position { get; }

        /// <summary>
        /// The string value of the token.
        /// </summary>
        public string Value { get; }

        internal bool IsImplicit => Position == ImplicitPosition;

        /// <summary>
        /// The type of the token.
        /// </summary>
        public TokenType Type { get; }

        /// <summary>
        /// The Symbol represented by the token (if any).
        /// </summary>
        internal Symbol? Symbol { get; set; }

        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as Token);

        /// <inheritdoc />
        public bool Equals(Token? other) => other is not null && Value == other.Value && Type == other.Type && ReferenceEquals(Symbol, other.Symbol);

        /// <inheritdoc />
        public override int GetHashCode() => Value.GetHashCode() ^ (int)Type;

        /// <inheritdoc />
        public override string ToString() => Value;
    }
}
