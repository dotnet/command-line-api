﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace System.CommandLine.Rendering
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class CursorControlSpan : Span
    {
        public CursorControlSpan(string name)
        {
            Name = name;
        }
        public override int ContentLength => 0;

        public string Name { get; }
        public static CursorControlSpan Hide() => new CursorControlSpan(nameof(Hide));
        public static CursorControlSpan Show() => new CursorControlSpan(nameof(Show));

    }
}
