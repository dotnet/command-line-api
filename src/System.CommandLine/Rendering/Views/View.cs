using System.CommandLine.Rendering.Models;

namespace System.CommandLine.Rendering.Views
{
    public abstract class View
    {
        public event EventHandler Updated;

        public abstract void Render(Region region, IRenderer renderer);

        public abstract Size GetContentSize();

        public abstract Size GetAdjustedSize(IRenderer renderer, Size maxSize);

        protected void OnUpdated()
        {
            Updated?.Invoke(this, EventArgs.Empty);
        }
    }
}
