using System.CommandLine.Rendering.Models;

namespace System.CommandLine.Rendering.Views
{
    public class StackedLayoutView : LayoutView<View>
    {
        public override Size GetContentSize() => throw new NotImplementedException();

        public override Size GetAdjustedSize(Size maxSize)
        {
            return maxSize;
        }
    }
}
