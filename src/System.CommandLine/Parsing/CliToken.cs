// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Parsing
{
    // TODO: Include location in equality

    // FIXME: should CliToken be public or internal? made internal for now
    // FIXME: should CliToken be a struct?
    /// <summary>
    /// A unit of significant text on the command line.
    /// </summary>
    internal sealed class CliToken : IEquatable<CliToken>
    {
        public static CliToken CreateFromOtherToken(CliToken otherToken, string? arg, Location location)
            => new(arg, otherToken.Type, otherToken.Symbol, location);

        /*
        /// <param name="value">The string value of the token.</param>
        /// <param name="type">The type of the token.</param>
        /// <param name="symbol">The symbol represented by the token</param>

        public CliToken(string? value, CliTokenType type, CliSymbol symbol)
        {
            Value = value ?? "";
            Type = type;
            Symbol = symbol;
            Location = Location.CreateImplicit(value, value is null ? 0 : value.Length);
        }
        */

        /// <param name="value">The string value of the token.</param>
        /// <param name="type">The type of the token.</param>
        /// <param name="symbol">The symbol represented by the token</param>
        /// <param name="location">The location of the token in the args array or a response file</param>
        internal CliToken(string? value, CliTokenType type, CliSymbol? symbol, Location location)
        {
            Value = value ?? "";
            Type = type;
            Symbol = symbol;
            Location = location;
        }

        internal Location Location { get; }

        /// <summary>
        /// The string value of the token.
        /// </summary>
        public string Value { get; }

        internal bool Implicit => Location.IsImplicit;

        /// <summary>
        /// The type of the token.
        /// </summary>
        public CliTokenType Type { get; }

        /// <summary>
        /// The Symbol represented by the token (if any).
        /// </summary>
        internal CliSymbol? Symbol { get; set; }

        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as CliToken);

        /// <inheritdoc />
        public bool Equals(CliToken? other) => other is not null && Value == other.Value && Type == other.Type && ReferenceEquals(Symbol, other.Symbol);

        /// <inheritdoc />
        public override int GetHashCode() => Value.GetHashCode() ^ (int)Type;

        /// <inheritdoc />
        public override string ToString() => Value;

        /// <summary>
        /// Checks if two specified <see cref="CliToken"/> instances have the same value.
        /// </summary>
        /// <param name="left">The first <see cref="CliToken"/>.</param>
        /// <param name="right">The second <see cref="CliToken"/>.</param>
        /// <returns><see langword="true" /> if the objects are equal.</returns>
        public static bool operator ==(CliToken? left, CliToken? right) => left is null ? right is null : left.Equals(right);

        /// <summary>
        /// Checks if two specified <see cref="CliToken"/> instances have different values.
        /// </summary>
        /// <param name="left">The first <see cref="CliToken"/>.</param>
        /// <param name="right">The second <see cref="CliToken"/>.</param>
        /// <returns><see langword="true" /> if the objects are not equal.</returns>
        public static bool operator !=(CliToken? left, CliToken? right) => left is null ? right is not null : !left.Equals(right);
    }
}
