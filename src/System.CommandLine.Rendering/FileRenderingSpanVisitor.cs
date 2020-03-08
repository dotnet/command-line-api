// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.IO;

namespace System.CommandLine.Rendering
{
    internal class FileRenderingSpanVisitor : ContentRenderingSpanVisitor
    {
        public FileRenderingSpanVisitor(
            IStandardStreamWriter writer,
            Region region) : base(writer, region)
        {
        }

        protected override void SetCursorPosition(int? left = null, int? top = null)
        {
            if (top > 0 && left == 0)
            {
                Writer.WriteLine();
            }
        }

        protected override void TryClearRemainingWidth()
        {
            ClearRemainingWidth();
        }
    }
}
