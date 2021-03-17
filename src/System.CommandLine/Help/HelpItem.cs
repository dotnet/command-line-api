// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Help
{
    public class HelpItem : IEquatable<HelpItem?>
    {
        public string Name { get; }
        public string Value { get; }

        public HelpItem(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public void Deconstruct(out string name, out string value)
        {
            name = Name;
            value = Value;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as HelpItem);
        }

        public bool Equals(HelpItem? other)
        {
            return other != null &&
                   Name == other.Name &&
                   Value == other.Value;
        }

        public override int GetHashCode()
        {
            int hashCode = -244751520;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Value);
            return hashCode;
        }

        public static bool operator ==(HelpItem? left, HelpItem? right)
        {
            return EqualityComparer<HelpItem>.Default.Equals(left, right);
        }

        public static bool operator !=(HelpItem? left, HelpItem? right)
        {
            return !(left == right);
        }
    }
}
