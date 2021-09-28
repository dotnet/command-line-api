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
        public HelpItem(string descriptor, string description)
        {
            Descriptor = descriptor;
            Description = description;
        }

        public string Descriptor { get; }

        public string Description { get; }

        public void Deconstruct(out string descriptor, out string description)
        {
            descriptor = Descriptor;
            description = Description;
        }

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

        public static bool operator ==(HelpItem? left, HelpItem? right)
        {
            return EqualityComparer<HelpItem?>.Default.Equals(left, right);
        }

        public static bool operator !=(HelpItem? left, HelpItem? right)
        {
            return !(left == right);
        }
    }
}
