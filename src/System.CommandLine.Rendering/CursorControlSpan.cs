// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace System.CommandLine.Rendering
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class CursorControlSpan : Span
    {
        public CursorControlSpan(string name, int row = 0, int column = 0)
        {
            Name = name;
            Row = row;
            Column = column;
        }
        public override int ContentLength => 0;

        public string Name { get; }
        public int Row { get; }
        public int Column { get; }

        public static CursorControlSpan Up() => new CursorControlSpan(nameof(Up));
        public static CursorControlSpan Down() => new CursorControlSpan(nameof(Down));
        public static CursorControlSpan Left() => new CursorControlSpan(nameof(Left));
        public static CursorControlSpan Right() => new CursorControlSpan(nameof(Right));
        public static CursorControlSpan Position(int row, int column) => new CursorControlSpan(nameof(Position), row, column);
        public static CursorControlSpan Hide() => new CursorControlSpan(nameof(Hide));
        public static CursorControlSpan Show() => new CursorControlSpan(nameof(Show));

    }
}
