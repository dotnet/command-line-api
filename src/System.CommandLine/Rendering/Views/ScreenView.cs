using System.CommandLine.Invocation;

namespace System.CommandLine.Rendering.Views
{
    public class ScreenView
    {
        private View _child;

        public ScreenView(IConsole console = null, IRenderer renderer = null, OutputMode outputMode = OutputMode.Ansi)
        {
            Console = console ?? SystemConsole.Instance;
            Renderer = renderer ?? new ConsoleRenderer(Console, outputMode);
        }

        private IConsole Console { get; }
        private IRenderer Renderer { get; }

        public View Child
        {
            get => _child;
            set
            {
                if (_child != null)
                {
                    _child.Updated -= ChildUpdated;
                }
                _child = value;
                if (value != null)
                {
                    value.Updated += ChildUpdated;
                }
            }
        }

        private void ChildUpdated(object sender, EventArgs e) => Render();

        // may not want this?
        public void Render(Region region)
        {
            Child?.Render(Renderer, region);
        }

        public void Render()
        {
            Render(Console.GetRegion());
        }
    }
}
