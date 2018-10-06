using System.CommandLine.Invocation;
using System.Threading;

namespace System.CommandLine.Rendering.Views
{
    public class ScreenView
    {
        private View _child;
        private int _renderRequested;
        private int _renderInProgress;
        private readonly SynchronizationContext _context;

        public ScreenView(IConsole console = null, ConsoleRenderer renderer = null, OutputMode outputMode = OutputMode.Ansi)
        {
            _context = SynchronizationContext.Current ?? new SynchronizationContext();
            Console = console ?? SystemConsole.Instance;
            Renderer = renderer ?? new ConsoleRenderer(Console, outputMode);
        }

        private IConsole Console { get; }
        private ConsoleRenderer Renderer { get; }

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

        private void ChildUpdated(object sender, EventArgs e)
        {
            if (Interlocked.CompareExchange(ref _renderRequested, 1, 0) == 0)
            {
                _context.Post(new SendOrPostCallback(x =>
                {
                    while (Interlocked.CompareExchange(ref _renderRequested, 0, 1) == 1)
                    {
                        if (Interlocked.CompareExchange(ref _renderInProgress, 1, 0) == 0)
                        {
                            Render();
                            Interlocked.Exchange(ref _renderInProgress, 0);
                        }
                    }
                }), null);
            }
        }

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
