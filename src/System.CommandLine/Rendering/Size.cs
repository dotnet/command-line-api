// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace System.CommandLine.Rendering
{
    [DebuggerDisplay("{Width}x{Height}")]
    public class Size
    {
        public const int MaxValue = -1;

        public Size(int width, int height)
        {
            if (width < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            Width = width;
            Height = height;
        }

        public int Width { get; }

        public int Height { get; }
    }
}
