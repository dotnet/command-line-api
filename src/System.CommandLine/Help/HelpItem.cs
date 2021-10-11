// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Help
{
    /// <summary>
    /// Provides details about an item to be formatted to output in order to display command line help.
    /// </summary>
    public class HelpItem : IEquatable<HelpItem?>
    {
        /// <param name="descriptor">The name and invocation details, typically displayed in the first help column.</param>
        /// <param name="description">The description of a symbol, typically displayed in the second help column.</param>
        public HelpItem(string descriptor, string description)
        {
            Descriptor = descriptor;
            Description = description;
        }

        /// <summary>
        /// The name and other usage details about the item.
        /// </summary>
        public string Descriptor { get; }

        /// <summary>
        /// The description of what the item does.
        /// </summary>
        public string Description { get; }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equals(obj as HelpItem);
        }

        /// <inheritdoc />
        public bool Equals(HelpItem? other)
        {
            return other is not null &&
                   Descriptor == other.Descriptor &&
                   Description == other.Description;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int hashCode = -244751520;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Descriptor);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Description);
            return hashCode;
        }

        /// <summary>
        /// Checks if two specified <see cref="HelpItem"/> instances have the same value.
        /// </summary>
        /// <param name="left">The first <see cref="HelpItem"/>.</param>
        /// <param name="right">The second <see cref="HelpItem"/>.</param>
        /// <returns><see langword="true" /> if the objects are equal.</returns>
        public static bool operator ==(HelpItem? left, HelpItem? right)
        {
            return EqualityComparer<HelpItem?>.Default.Equals(left, right);
        }

        /// <summary>
        /// Checks if two specified <see cref="HelpItem"/> instances have different values.
        /// </summary>
        /// <param name="left">The first <see cref="HelpItem"/>.</param>
        /// <param name="right">The second <see cref="HelpItem"/>.</param>
        /// <returns><see langword="true" /> if the objects are not equal.</returns>
        public static bool operator !=(HelpItem? left, HelpItem? right)
        {
            return !(left == right);
        }
    }
}
