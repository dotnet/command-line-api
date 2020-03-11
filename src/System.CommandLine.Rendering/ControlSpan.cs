// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.IO;

namespace System.CommandLine.Rendering
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class ControlSpan : TextSpan
    {
        public ControlSpan(string name, AnsiControlCode ansiControlCode)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(nameof(name));
            }

            Name = name;
            AnsiControlCode = ansiControlCode;
        }

        public string Name { get; }

        public AnsiControlCode AnsiControlCode { get; }

        public override int ContentLength => 0;
        
        public override string ToString() => "";

        public override void WriteTo(TextWriter writer, OutputMode outputMode)
        {
            switch (outputMode)
            {
                case OutputMode.Ansi:
                    writer.Write(AnsiControlCode.EscapeSequence);
                    break;
                default:
                    writer.Write(ToString());
                    break;
            }
        }

        protected bool Equals(ControlSpan other) => string.Equals(Name, other.Name);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((ControlSpan)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return GetType().GetHashCode() * 397 ^ Name.GetHashCode();
            }
        }
    }
}
