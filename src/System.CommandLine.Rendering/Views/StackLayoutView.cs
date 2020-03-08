// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Rendering.Views
{
    public class StackLayoutView : LayoutView<View>
    {
        public StackLayoutView()
                : this(Orientation.Vertical)
        {
        }

        public StackLayoutView(Orientation orientation)
        {
            Orientation = orientation;
        }

        public Orientation Orientation { get; }

        public override void Render(ConsoleRenderer renderer, Region region = null)
        {
            switch (Orientation)
            {
                case Orientation.Vertical:
                    RenderVertical(region, renderer);
                    break;
                case Orientation.Horizontal:
                    RenderHorizontal(region, renderer);
                    break;
            }
        }

        private void RenderVertical(Region region, ConsoleRenderer renderer)
        {
            var left = region.Left;
            var top = region.Top;
            var height = region.Height;

            foreach (var child in Children)
            {
                if (height <= 0)
                {
                    break;
                }
                var size = child.Measure(renderer, new Size(region.Width, height));
                int renderHeight = Math.Min(height, size.Height);
                var r = new Region(left, top, size.Width, renderHeight);
                child.Render(renderer, r);
                top += size.Height;
                height -= renderHeight;
            }
        }

        private void RenderHorizontal(Region region, ConsoleRenderer renderer)
        {
            var left = region.Left;
            var top = region.Top;
            var width = region.Width;

            foreach (var child in Children)
            {
                if (width <= 0)
                {
                    break;
                }
                var size = child.Measure(renderer, new Size(width, region.Height));
                var r = new Region(left, top, width, size.Height);
                child.Render(renderer, r);
                left += size.Width;
                width -= size.Width;
            }
        }

        public override Size Measure(ConsoleRenderer renderer, Size maxSize)
        {
            switch (Orientation)
            {
                case Orientation.Vertical:
                    return GetAdjustedSizeVertical(renderer, maxSize);
                case Orientation.Horizontal:
                    return GetAdjustedSizeHorizontal(renderer, maxSize);
                default:
                    throw new InvalidOperationException($"Orientation {Orientation} is not implemented");
            }
        }

        private Size GetAdjustedSizeVertical(ConsoleRenderer renderer, Size maxSize)
        {
            var maxWidth = 0;
            var totHeight = 0;

            var height = maxSize.Height;

            foreach (var child in Children)
            {
                if (height <= 0)
                {
                    break;
                }
                var size = child.Measure(renderer, new Size(maxSize.Width, height));
                height -= size.Height;
                totHeight += size.Height;
                maxWidth = Math.Max(maxWidth, size.Width);
            }

            return new Size(maxWidth, totHeight);
        }

        private Size GetAdjustedSizeHorizontal(ConsoleRenderer renderer, Size maxSize)
        {
            var maxHeight = 0;
            var totalWidth = 0;

            var width = maxSize.Width;

            foreach (var child in Children)
            {
                if (width <= 0)
                {
                    break;
                }
                var size = child.Measure(renderer, new Size(width, maxSize.Height));
                width -= size.Width;
                totalWidth += size.Width;
                maxHeight = Math.Max(maxHeight, size.Height);
            }

            return new Size(totalWidth, maxHeight);
        }
    }
}

