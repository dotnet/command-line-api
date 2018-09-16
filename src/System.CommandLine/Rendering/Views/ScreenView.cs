using System.CommandLine.Invocation;

namespace System.CommandLine.Rendering.Views
{
    public class ScreenView
    {
        public ScreenView(IConsole console = null)
        {
            Console = console ?? SystemConsole.Instance;
        }

        private IConsole Console { get; }
        public View Child { get; set; }

        // may not want this?
        public void Render(Region region, IRenderer renderer)
        {
            Child?.Render(region, renderer);
        }

        public void Render(IRenderer renderer)
        {
            Render(Console.GetRegion(), renderer);
        }
    }
}
