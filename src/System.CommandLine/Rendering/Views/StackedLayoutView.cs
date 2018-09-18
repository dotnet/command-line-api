using System.Linq;

namespace System.CommandLine.Rendering.Views
{
    public class StackedLayoutView : LayoutView<View>
    {
        public StackedLayoutView()
                : this(Orientation.Vertical)
        {
        }

        public StackedLayoutView(Orientation orientation)
        {
            Orientation = orientation;
        }

        public Orientation Orientation { get; set; }

        public override void Render(Region region, IRenderer renderer)
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

        private void RenderVertical(Region region, IRenderer renderer)
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
                var size = child.GetAdjustedSize(renderer, new Size(region.Width, height));
                var r = new Region(left, top, size.Width, height);
                child.Render(r, renderer);
                top += size.Height;
                height -= size.Height;
            }
        }

        private void RenderHorizontal(Region region, IRenderer renderer)
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
                var size = child.GetAdjustedSize(renderer, new Size(width, region.Height));
                var r = new Region(left, top, width, size.Height);
                child.Render(r, renderer);
                left += size.Width;
                width -= size.Width;
            }
        }

        public override Size GetAdjustedSize(IRenderer renderer, Size maxSize)
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

        private Size GetAdjustedSizeVertical(IRenderer renderer, Size maxSize)
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
                var size = child.GetAdjustedSize(renderer, maxSize);
                height -= size.Height;
                totHeight += size.Height;
                maxWidth = Math.Max(maxWidth, size.Width);
            }

            return new Size(maxWidth, totHeight);
        }

        private Size GetAdjustedSizeHorizontal(IRenderer renderer, Size maxSize)
        {
            var maxHeight = 0;
            var totWidth = 0;

            var width = maxSize.Width;

            foreach (var child in Children)
            {
                if (width <= 0)
                {
                    break;
                }
                var size = child.GetAdjustedSize(renderer, maxSize);
                width -= size.Width;
                totWidth += size.Width;
                maxHeight = Math.Max(maxHeight, size.Height);
            }

            return new Size(totWidth, maxHeight);
        }
    }
}

