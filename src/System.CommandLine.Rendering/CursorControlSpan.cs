// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace System.CommandLine.Rendering
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class CursorControlSpan : ControlSpan
    {
        public CursorControlSpan(string name, AnsiControlCode ansiControlCode) :
            base(name, ansiControlCode)
        {
        }

        public override int ContentLength => 0;

        public static CursorControlSpan Hide() => new(nameof(Hide), Ansi.Cursor.Hide);

        public static CursorControlSpan Show() => new(nameof(Show), Ansi.Cursor.Show);
    }
}